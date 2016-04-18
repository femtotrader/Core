#region License
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
#endregion

using NLog;
using Quantler.Broker;
using Quantler.Data.Ticks;
using Quantler.Data.TikFile;
using Quantler.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Quantler.Securities;

namespace Quantler.Simulator
{
    /// <summary>
    /// historical simulation component. plays back many tickfiles insequence over time. different
    /// than multi-sim, multiple symbols in the same day are not guaranteed to be played back in
    /// sequence. also processes orders and executions against same tickfiles (via embedded Broker component).
    /// </summary>
    [DesignerCategory("")]
    public class SingleSimImpl : HistSim
    {
        #region Public Fields

        public static long Endsim = long.MaxValue;
        public static long Startsim = long.MinValue;

        #endregion Public Fields

        #region Private Fields

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        private const string Tickext = TikConst.WildcardExt;
        private readonly SimBroker _broker = new SimBroker();
        private readonly List<long> _dates = new List<long>();
        private readonly List<SecurityImpl> _secs = new List<SecurityImpl>();
        private int _availticks;
        private int _cidx;
        private int _doneidx;
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
            if (_tickfiles.Length == 0)
            {
                // get our listings of historical files (idx and epf)
                string[] files = Directory.GetFiles(_folder, Tickext);
                _tickfiles = _filter.Allows(files);
            }
            List<long> d = new List<long>(_tickfiles.Length);
            List<SecurityImpl> ss = new List<SecurityImpl>(_tickfiles.Length);
            // now we have our list, initialize instruments from files
            for (int i = 0; i < _tickfiles.Length; i++)
            {
                SecurityImpl s = getsec(i);
                if ((s != null) && s.IsValid && s.HistSource.IsValid)
                {
                    s.HistSource.GotTick += HistSource_gotTick;
                    ss.Add(s);
                    d.Add(s.Date);
                }
            }
            // setup our initial index
            long[] didx = d.ToArray();
            SecurityImpl[] sidx = ss.ToArray();
            Array.Sort(didx, sidx);
            // save index and objects in order
            _secs.Clear();
            _dates.Clear();
            _secs.AddRange(sidx);
            _dates.AddRange(didx);
            _doneidx = _tickfiles.Length - 1;

            Log.Debug("Initialized " + _tickfiles.Length + " instruments.");
            Log.Debug(string.Join(Environment.NewLine, _tickfiles));
            // check for event
            if (GotTick != null)
                _hasevent = true;
            else
                Log.Debug("No GotTick event defined!");
            // read in single tick just to get first time for user
            Isnexttick();

            // get total ticks represented by files
            _availticks = 0;
            for (int i = 0; i < _secs.Count; i++)
                if (_secs[i] != null)
                    _availticks += _secs[i].ApproxTicks;

            Log.Debug("Approximately " + TicksPresent + " ticks to process...");
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
            _dates.Clear();
            _secs.Clear();
            _nextticktime = Startsim;
            _broker.Reset();
            _executions = 0;
            _availticks = 0;
            _tickcount = 0;
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

        #endregion Private Methods

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
                _orderok &= _lasttick || (t.Datetime >= _next.Datetime) || (_cidx != _next.Symidx);
            }
            if (_lasttick)
            {
                _hasnext = false;
                return;
            }
            // update next tick
            _next = (TickImpl)t;
            _next.Symidx = _cidx;
            _hasnext = true;
            _nextticktime = t.Datetime;
            _tickcount++;
        }

        private bool Isnexttick()
        {
            if (_cidx > _doneidx || _cidx >= _secs.Count)
            {
                _lasttick = true;
                HistSource_gotTick(null);
                return false;
            }
            if (!_secs[_cidx].NextTick())
                _cidx++;
            return true;
        }
    }
}