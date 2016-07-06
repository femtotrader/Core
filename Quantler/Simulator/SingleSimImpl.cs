/*
Copyright Quantler BV, based on original code copyright Tradelink.org.
This file is released under the GNU Lesser General Public License v3. http://www.gnu.org/copyleft/lgpl.html

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 3.0 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.
*/

using NLog;
using Quantler.Broker;
using Quantler.Data.Ticks;
using Quantler.Data.TikFile;
using Quantler.Interfaces;
using Quantler.Securities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Quantler.Simulator
{
    /// <summary>
    /// historical simulation component. plays back many tickfiles insequence over time. different
    /// than multi-sim, multiple symbols in the same day are not guaranteed to be played back in
    /// sequence. also processes orders and executions against same tickfiles (via embedded Broker component).
    /// </summary>
    public class SingleSimImpl : HistSim
    {
        #region Public Fields

        public static long Endsim = long.MaxValue;
        public static long Startsim = long.MinValue;

        #endregion Public Fields

        #region Private Fields

        private const string Tickext = TikConst.WildcardExt;
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        private readonly SimBroker _broker = new SimBroker();
        private readonly List<SecurityImpl> _securityfiles = new List<SecurityImpl>();
        private int _availticks;
        private int _currentindex;
        private int _doneindex;
        private int _executions;
        private TickFileFilter _filter = new TickFileFilter();

        // working variables
        private string _folder = string.Empty;

        private bool _go = true;
        private bool _hasevent;
        private bool _hasnext;
        private bool _inited;
        private bool _lasttick;
        private TickImpl _next;
        private long _nextticktime = Endsim;
        private bool _orderok = true;
        private long _simend;
        private long _simstart;
        private volatile int _tickcount;
        private string[] _tickfiles = new string[0];
        private const int _cacheahead = 5;
        private Dictionary<string, TickFileInfo> _fileInfos;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Create historical simulator with your own tick folder
        /// </summary>
        /// <param name="tickFolder"></param>
        public SingleSimImpl(string tickFolder)
            : this(tickFolder, null)
        {
        }

        /// <summary>
        /// Create a historical simulator
        /// </summary>
        /// <param name="tickFolder">tick folder to use</param>
        /// <param name="tff">filter to determine what tick files from folder to use</param>
        public SingleSimImpl(string tickFolder, TickFileFilter tff)
        {
            _folder = tickFolder;
            if (tff != null)
                _filter = tff;
            else
            {
                _filter.DefaultDeny = false;
            }
        }

        /// <summary>
        /// Create a historical simulator
        /// </summary>
        /// <param name="filenames">list of tick files to use</param>
        public SingleSimImpl(string[] filenames)
        {
            _tickfiles = filenames;
        }

        #endregion Public Constructors

        #region Public Events

        // events
        public event TickDelegate GotTick;

        #endregion Public Events

        #region Public Properties

        // user-facing interfaces
        public TickFileFilter FileFilter { get { return _filter; } set { _filter = value; Log.Debug("Restarting simulator with " + _filter); Reset(); Initialize(); } }

        /// <summary>
        /// Fills executed during this simulation run.
        /// </summary>
        public int FillCount { get { return _executions; } }

        /// <summary>
        /// change the tickfolder histsim scans for historical data
        /// </summary>
        public string Folder { get { return _folder; } set { _folder = value; Log.Debug("Restarting simulator with " + _filter); Reset(); Initialize(); } }

        public bool IsTickPlaybackOrdered { get { return _orderok; } }

        /// <summary>
        /// Gets next tick in the simulation
        /// </summary>
        public long NextTickTime { get { return _nextticktime; } }

        public double RunTimeSec { get { return Runtime.TotalSeconds; } }

        public double RunTimeSecMs { get { return Runtime.TotalMilliseconds; } }

        public double RunTimeTicksPerSec { get { return _tickcount / RunTimeSec; } }

        /// <summary>
        /// Total files available for processing, based on provided filter or tick files
        /// </summary>
        public int FilesPresent { get; private set; }

        /// <summary>
        /// Files processed in this simulation run.
        /// </summary>
        public int FilesProcessed { get; private set; }

        /// <summary>
        /// Gets broker used in the simulation
        /// </summary>
        public SimBroker SimBroker { get { return _broker; } }

        /// <summary>
        /// Total ticks available for processing, based on provided filter or tick files.
        /// </summary>
        public int TicksPresent { get { return _availticks; } }

        /// <summary>
        /// Ticks processed in this simulation run.
        /// </summary>
        public int TicksProcessed { get { return _tickcount; } }

        #endregion Public Properties

        #region Private Properties

        private TimeSpan Runtime
        {
            get
            {
                DateTime start = new DateTime(_simstart);
                DateTime end = new DateTime(_simend);
                return end.Subtract(start);
            }
        }

        #endregion Private Properties

        #region Public Methods

        /// <summary>
        /// Reinitialize the cache
        /// </summary>
        public void Initialize()
        {
            if (_inited) return; // only init once

            //Clear current data
            _securityfiles.Clear();

            if (_tickfiles.Length == 0)
            {
                // get our listings of historical files (idx and epf)
                string[] files = Directory.GetFiles(_folder, Tickext);
                _tickfiles = _filter.Allows(files);
            }

            _fileInfos = new Dictionary<string, TickFileInfo>();
            // get dates
            for (int i = 0; i < _tickfiles.Length; i++)
                _fileInfos.Add(_tickfiles[i], Util.ParseFile(_tickfiles[i]));

            // setup our initial index (order all files by dates)
            _tickfiles = _fileInfos.OrderBy(x => x.Value.Date).Select(x => x.Key).ToArray();

            // save index and objects in order
            _doneindex = _tickfiles.Length - 1;
            FilesPresent = _tickfiles.Length;
            FilesProcessed = 0;

            // initialize initial files (readahead x number of files)
            InitializeAhead(_cacheahead);

            // check for event
            if (GotTick != null)
                _hasevent = true;
            else
                Log.Debug("No GotTick event defined!");

            // read in single tick just to get first time for user
            Isnexttick();

            //Done for now
            _inited = true;
        }

        /// <summary>
        /// Run simulation to specific time
        /// </summary>
        /// <param name="ftime">
        /// Simulation will run until this time (use HistSim.ENDSIM for last time)
        /// </param>
        public void PlayTo(long ftime)
        {
            _simstart = DateTime.Now.Ticks;
            _orderok = true;
            _go = true;
            if (!_inited)
                Initialize();
            if (_inited)
            {
                // process
                while (_go && (NextTickTime < ftime) && Isnexttick())
                {
                }
            }
            else throw new Exception("Histsim was unable to initialize");

            // mark end of simulation
            _simend = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Reset the simulation
        /// </summary>
        public void Reset()
        {
            _orderok = true;
            _simstart = 0;
            _simend = 0;
            _inited = false;
            _tickfiles = new string[0];
            _fileInfos.Clear();
            _securityfiles.Clear();
            _nextticktime = Startsim;
            _broker.Reset();
            _executions = 0;
            _availticks = 0;
            _tickcount = 0;
            FilesProcessed = 0;
            FilesPresent = 0;
        }

        /// <summary>
        /// stops any running simulation and closes all data files
        /// </summary>
        public void Stop()
        {
            _go = false;
        }

        #endregion Public Methods

        #region Private Methods

        private SecurityImpl getsec(int tickfileidx)
        {
            return getsec(_tickfiles[tickfileidx]);
        }

        private void InitializeAhead(int files)
        {
            if (_currentindex + files > _doneindex)
                files = _doneindex - _currentindex + 1;

            // now we have our list, initialize instruments from files
            for (int i = _currentindex; i < files + _currentindex; i++)
            {
                SecurityImpl s = getsec(i);
                if ((s != null) && s.IsValid && s.HistSource.IsValid)
                {
                    s.HistSource.GotTick += HistSource_gotTick;
                    _securityfiles.Add(s);
                }
            }

            // log currently read files
            int ticksfound = _securityfiles.Skip(_securityfiles.Count - files).Sum(x => x.ApproxTicks);
            _availticks += ticksfound;

            //Dispose and clear memory by removing old and unused files
            if (_currentindex > 0)
                CloseUnusedReaders(_currentindex - files, _currentindex - 1);
        }

        /// <summary>
        /// Close old and unused readers (clears memory for new files)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void CloseUnusedReaders(int from, int to)
        {
            for (int i = from; i < to; i++)
            {
                _securityfiles[i].HistSource.Close();
                FilesProcessed++;
            }
        }

        private SecurityImpl getsec(string file)
        {
            try
            {
                //Check if file contains a zip definition
                if (file.ToLower().Contains("zip"))
                {
                    //Get zip file and tik file from filename
                    string[] parts = file.ToLower().Split('\\');
                    string tickfile = parts.Skip(parts.Length - 1).First();
                    string zipfile = string.Join(@"\", parts.Take(parts.Length - 1));

                    return SecurityImpl.FromZip(zipfile, tickfile);
                }
                return SecurityImpl.FromTik(file);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "error reading TIK file: " + file);
                return null;
            }
        }

        private void HistSource_gotTick(Tick t)
        {
            // process next tick if present
            if (_hasnext)
            {
                // execute any pending orders
                //SimBroker.Execute(next);
                // send existing tick
                if (_hasevent && GotTick != null)
                    GotTick(_next);
                // update last time
                _orderok &= _lasttick || (t.Datetime >= _next.Datetime) || (_currentindex != _next.Symidx);
            }
            if (_lasttick)
            {
                _hasnext = false;
                return;
            }
            // update next tick
            _next = (TickImpl)t;
            _next.Symidx = _currentindex;
            _hasnext = true;
            _nextticktime = t.Datetime;
            _tickcount++;
        }

        private bool Isnexttick()
        {
            if (_currentindex > _doneindex || _currentindex >= _securityfiles.Count)
            {
                _lasttick = true;
                HistSource_gotTick(null);
                return false;
            }
            if (!_securityfiles[_currentindex].NextTick())
            {
                _currentindex++;
                if (_currentindex % _cacheahead == 0)
                    InitializeAhead(_cacheahead);
            }
            return true;
        }

        #endregion Private Methods
    }
}