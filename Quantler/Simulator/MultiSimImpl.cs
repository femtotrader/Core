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
using Quantler.Data.TikFile;
using Quantler.Interfaces;
using Quantler.Securities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Quantler.Simulator
{
    /// <summary>
    /// historical simulation component. plays back many tickfiles insequence over time. also
    /// processes orders and executions against same tickfiles (via embedded Broker component).
    /// </summary>
    public class MultiSimImpl : HistSim
    {
        #region Public Fields

        public static long Endsim = long.MaxValue;
        public static long Startsim = long.MinValue;

        #endregion Public Fields

        #region Private Fields

        private const int Completed = -1;
        private const string Tickext = TikConst.WildcardExt;
        private readonly Dictionary<string, TickFileInfo> _fileInfos = new Dictionary<string, TickFileInfo>();
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();
        private readonly int _yieldtime = 1;
        private int _availticks;
        private int _cachepause = 10;
        private int _currentindex;
        private int _executions;
        private bool _simrunning;
        private TickFileFilter _filter = new TickFileFilter();

        // working variables
        private string _folder = string.Empty;

        private bool _inited;
        private bool _lastorderok = true;
        private long _lasttime = long.MinValue;
        private int _nextindex;
        private long _nextticktime = Endsim;
        private bool _orderok = true;
        private int _readcache = 40;
        private long _simend;
        private long _simstart;
        private volatile int _tickcount;
        private string[] _tickfiles = new string[0];
        private List<SimWorker> _workers = new List<SimWorker>();
        private bool _importing = false;
        private static object _locker = new object();

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Create historical simulator with your own tick folder
        /// </summary>
        /// <param name="tickFolder"></param>
        public MultiSimImpl(string tickFolder)
            : this(tickFolder, null)
        {
        }

        /// <summary>
        /// Create a historical simulator
        /// </summary>
        /// <param name="tickFolder">tick folder to use</param>
        /// <param name="tff">filter to determine what tick files from folder to use</param>
        public MultiSimImpl(string tickFolder, TickFileFilter tff)
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
        public MultiSimImpl(string[] filenames)
        {
            _tickfiles = filenames;
        }

        #endregion Public Constructors

        #region Public Events

        // events
        public event TickDelegate GotTick;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// milliseconds to wait between starting I/O threads and trying to access data. is used
        /// only on multi processor machines.
        /// </summary>
        public int CacheWait { get { return _cachepause; } set { _cachepause = value; } }

        // user-facing interfaces
        public TickFileFilter FileFilter { get { return _filter; } set { _filter = value; D("Restarting simulator with " + _filter); Reset(); Initialize(); } }

        /// <summary>
        /// Total files available for processing, based on provided filter or tick files
        /// </summary>
        public int FilesPresent
        {
            get;
            private set;
        }

        /// <summary>
        /// Files processed in this simulation run.
        /// </summary>
        public int FilesProcessed
        {
            get;
            private set;
        }

        /// <summary>
        /// Fills executed during this simulation run.
        /// </summary>
        public int FillCount { get { return _executions; } }

        /// <summary>
        /// change the tickfolder histsim scans for historical data
        /// </summary>
        public string Folder { get { return _folder; } set { _folder = value; D("Restarting simulator with " + _filter); Reset(); Initialize(); } }

        public bool IsTickPlaybackOrdered { get { return _orderok; } }

        /// <summary>
        /// Gets next tick in the simulation
        /// </summary>
        public long NextTickTime { get { return _nextticktime; } }

        public double RunTimeSec { get { return Runtime.TotalSeconds; } }

        public double RunTimeSecMs { get { return Runtime.TotalMilliseconds; } }

        public double RunTimeTicksPerSec { get { return _tickcount / RunTimeSec; } }

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
            _fileInfos.Clear();

            if (_tickfiles.Length == 0)
            {
                // get our listings of historical files (idx and epf)
                string[] files = Directory.GetFiles(_folder, Tickext);
                _tickfiles = _filter.Allows(files);
            }

            //Check for cache size
            if (_tickfiles.Length < _readcache)
                _readcache = _tickfiles.Length;

            // now we have our list, initialize initial instruments from file
            for (int i = 0; i < _tickfiles.Length; i++)
                _fileInfos.Add(_tickfiles[i], Util.ParseFile(_tickfiles[i]));

            // order files according to date
            _tickfiles = _fileInfos.OrderBy(x => x.Value.Date).Select(x => x.Key).ToArray();

            // setup our initial index
            _currentindex = 0;
            FilesPresent = _fileInfos.Count;
            FilesProcessed = 0;

            // read in single tick just to get first time for user
            InitializeAhead(_readcache);
            _inited = true;

            // set first time as hint for user
            Setnexttime();
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
            if (!_inited)
                Initialize();
            if (_inited)
            {
                SecurityPlayTo(ftime);
            }
            else throw new Exception("Histsim was unable to initialize");
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
            _workers.Clear();
            _nextticktime = Startsim;
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
            lock (_workers)
            {
                foreach (SimWorker w in _workers)
                {
                    if (w.IsBusy)
                        w.CancelAsync();
                    if (w.Workersec.HistSource.BaseStream != null && w.Workersec.HistSource.BaseStream.CanRead)
                    {
                        try
                        {
                            w.Workersec.HistSource.Close();
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }

            //end sim
            _simrunning = false;
        }

        #endregion Public Methods

        #region Private Methods

        private void CancelWorkers()
        {
            lock (_workers)
            {
                foreach (SimWorker t in _workers)
                    t.CancelAsync();
            }
        }

        private void D(string message)
        {
            _log.Debug(message);
        }

        private void Debug(string msg)
        {
            _log.Debug(msg);
        }

        /// <summary>
        /// Flush data
        /// </summary>
        /// <param name="endsim"></param>
        private void FlushCache(long endsim)
        {
            _simrunning = true;
            var times = Nexttimes();
            _currentindex = times.ElementAt(0).Key;
            long nexttime = times.Count() > 1 ? times.ElementAt(1).Value : long.MaxValue;
            while (_simrunning)
            {
                // get next times of ticks in cache
                if (!_workers[_currentindex].HasTicks || _workers[_currentindex].NextTime() > nexttime)
                {
                    times = Nexttimes();

                    //Check if we are finished
                    if (times.Count() == 1 && times.ElementAt(0).Value == Completed)
                    {
                        V("No ticks left.");
                        break;
                    }

                    nexttime = times.Count() > 1 ? times.ElementAt(1).Value : long.MaxValue;

                    //Reset current index to earliest item
                    _currentindex = times.ElementAt(0).Key;
                }

                // get next time from all instruments we have loaded
                while (times.ElementAt(0).Value == -1)
                {
                    //Request next file
                    Task.Run(() => InitializeAhead(1));

                    //Refresh times
                    times = Nexttimes();

                    //Set new element
                    _currentindex = times.ElementAt(0).Key;
                }

                //Console.WriteLine("Using file {0}", _workers[_currentindex].Name);

                // test to see if ticks left in simulation
                bool ticksleft = _nextindex <= _tickfiles.Length && _workers[_currentindex].HasTicks;
                bool simtimeleft = ticksleft && times.ElementAt(_currentindex).Value <= endsim;
                _simrunning = ticksleft && simtimeleft;

                // if no ticks left or we exceeded simulation time, quit
                if (!_simrunning)
                {
                    if (!ticksleft)
                        V("No ticks left.");
                    if (!simtimeleft)
                        V("Hit end of simulation.");
                    else
                        V("Simulation was stopped");

                    break;
                }

                // get next tick
                Tick k = _workers[_currentindex].NextTick();

                // time check
                _orderok &= k.Datetime >= _lasttime;

                if (_orderok != _lastorderok)
                {
                    V("tick out of order: " + k.Symbol + " w/" + k.Datetime + " behind: " + _lasttime);
                    _lastorderok = _orderok;
                }

                // update time
                _lasttime = k.Datetime;

                // notify tick
                if (GotTick != null) GotTick(k);

                // count tick
                _tickcount++;
            }
            V("simulating exiting.");
        }

        /// <summary>
        /// Load security data (tik data) from file (either zipped or on disk)
        /// </summary>
        /// <param name="tickfileidx"></param>
        /// <returns></returns>
        private SecurityImpl GetSecurity(int tickfileidx)
        {
            if (tickfileidx < _tickfiles.Length)
                return GetSecurity(_tickfiles[tickfileidx]);
            else
                return null;
        }

        /// <summary>
        /// Load security data (tik data) from file (either zipped or on disk)
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private SecurityImpl GetSecurity(string file)
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
                Debug("error reading TIK file: " + file + " err: " + ex.Message + ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Read files into memory asynchronously
        /// </summary>
        /// <param name="files"></param>
        private void InitializeAhead(int files)
        {
            //Check Barrier
            lock (_locker)
            {
                if (_importing)
                    return;
                else
                    _importing = true;
            }

            //Dispose and clear memory by removing old and unused files
            if (_inited)
                RemoveLastUnusedReader();

            //Check for correct amount input
            if (_workers.Count < _readcache)
                files = _readcache - _workers.Count;
            else if (_workers.Count >= _readcache)
                files = 0;

            if (_nextindex + files > _fileInfos.Count)
                files = _fileInfos.Count - _nextindex + 1;

            // now we have our list, initialize instruments from files
            int max = files + _nextindex > _tickfiles.Length ? _tickfiles.Length : files + _nextindex;
            int ticksfound = 0;
            for (int i = _nextindex; i < max; i++)
            {
                try
                {
                    SecurityImpl s = GetSecurity(_nextindex);
                    if ((s != null) && s.IsValid && s.HistSource.IsValid)
                    {
                        lock (_workers)
                        {
                            var worker = new SimWorker(s);
                            worker.SingleCoreFillCache(int.MaxValue); //File is already loaded in memory due to zip reader
                            //worker.RunWorkerAsync(int.MaxValue); //NOT BEING FIRED ON SEPERATE THREAD
                            V(worker.Name + " worker#" + i + " now is working.");

                            _workers.Add(worker);
                            ticksfound = _workers.Skip(_workers.Count - files).Sum(x => x.Workersec.ApproxTicks);
                        }
                    }

                    _nextindex++;
                }
                catch (Exception ex)
                {
                    D("Unable to initialize: " + _tickfiles[i] + " error: " + ex.Message + ex.StackTrace);
                }
            }

            // log ticks found
            _availticks += ticksfound;

            //Barrier
            lock (_locker)
            {
                _importing = false;
            }
        }

        /// <summary>
        /// Returns an ordered list of next times for the workers currently active
        /// </summary>
        /// <returns></returns>
        private IEnumerable<KeyValuePair<int, long>> Nexttimes()
        {
            // setup a next entry for every instrument
            Dictionary<int, SimWorker> toreturn = new Dictionary<int, SimWorker>();
            int timesidx = 0;

            // loop through instrument's next time, set flag if no more ticks left in cache
            int amount = _readcache;
            if (_readcache >= 15)
                amount = _readcache / 2;

            List<SimWorker> workerscopy;
            lock (_workers)
            {
                workerscopy = _workers.Take(amount).ToList();
            }

            foreach (var file in workerscopy)
            {
                // check file
                if (file == null)
                    continue;

                // loop until worker has ticks
                while (!file.HasTicks)
                {
                    // or the worker is done reading tick stream
                    if (!file.IsWorking)
                    {
                        break;
                    }

                    // if we're not done, wait for the I/O thread to catch up
                    Thread.Sleep(_yieldtime);
                }

                // we should either have ticks or be finished with this worker, set the value of
                // this worker's next time value accordingly
                toreturn.Add(timesidx, file);
                timesidx++;
            }

            var items = toreturn
                .Select(x => new { Key = x.Key, Value = x.Value.HasTicks ? x.Value.NextTime() : Completed, Name = x.Value.Workersec.Name })
                .OrderBy(x => x.Value)
                .ThenBy(x => x.Name)
                .Select(x => new KeyValuePair<int, long>(x.Key, x.Value));
            return items;
        }

        /// <summary>
        /// Close old and unused readers (clears memory for new files)
        /// </summary>
        private void RemoveLastUnusedReader()
        {
            lock (_workers)
            {
                var items = _workers
                    .Where(x => x != null)
                    .Where(x => !x.HasTicks).ToArray();

                foreach (var key in items)
                {
                    _workers.Remove(key);
                    FilesProcessed++;
                }
            }
        }

        /// <summary>
        /// Flush cached files to a specific time
        /// </summary>
        /// <param name="ftime"></param>
        private void SecurityPlayTo(long ftime)
        {
            // start all the workers reading files in background
            //FillCache(int.MaxValue);

            // wait a moment to allow tick reading to start
            Thread.Sleep(_cachepause);

            // continuously loop through next ticks, playing most recent ones, until simulation
            // end is reached.
            FlushCache(ftime);

            // when we end simulation, stop reading but don't touch buffer
            CancelWorkers();

            // set next tick time as hint to user
            Setnexttime();
            // mark end of simulation
            _simend = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Get next times moments
        /// </summary>
        private void Setnexttime()
        {
            // get next times of ticks in cache
            var times = Nexttimes();
            int i = 0;

            // get first one available
            foreach (var item in times)
                if ((i < times.Count()) && (item.Value == Completed))
                    i++;

            // set next time to first available time, or end of simulation if none available
            _nextticktime = i == times.Count() ? Endsim : times.ElementAt(i).Value;
        }

        /// <summary>
        /// Trace logging method
        /// </summary>
        /// <param name="msg"></param>
        private void V(string msg)
        {
            _log.Trace("[MultiSimImpl] " + _lasttime + " " + msg);
        }

        #endregion Private Methods
    }
}