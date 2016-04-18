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
using Quantler.Data.TikFile;
using Quantler.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using Quantler.Securities;

namespace Quantler.Simulator
{
    /// <summary>
    /// historical simulation component. plays back many tickfiles insequence over time. also
    /// processes orders and executions against same tickfiles (via embedded Broker component).
    /// </summary>
    [DesignerCategory("")]
    public class MultiSimImpl : HistSim
    {
        #region Public Fields

        public static long Endsim = long.MaxValue;
        public static long Startsim = long.MinValue;

        #endregion Public Fields

        #region Private Fields

        private const int Completed = -1;
        private const string Tickext = TikConst.WildcardExt;
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();
        private readonly List<SimWorker> _workers = new List<SimWorker>();
        private readonly int _yieldtime = 1;
        private int _availticks;
        private int _cachepause = 10;
        private int[] _cidx;
        private int _executions;
        private TickFileFilter _filter = new TickFileFilter();

        // working variables
        private string _folder = string.Empty;

        private int[] _idx;
        private bool _inited;
        private bool _lastorderok = true;
        private long _lasttime = long.MinValue;
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
            if (_tickfiles.Length == 0)
            {
                // get our listings of historical files (idx and epf)
                string[] files = Directory.GetFiles(_folder, Tickext);
                _tickfiles = _filter.Allows(files);
            }

            // now we have our list, initialize instruments from files
            for (int i = 0; i < _tickfiles.Length; i++)
            {
                try
                {
                    SecurityImpl s = GetSecurity(i);
                    if ((s != null) && s.IsValid && s.HistSource.IsValid)
                        _workers.Add(new SimWorker(s));
                }
                catch (Exception ex)
                {
                    D("Unable to initialize: " + _tickfiles[i] + " error: " + ex.Message + ex.StackTrace);
                }
            }
            // setup our initial index
            _idx = Genidx(_workers.Count);
            _cidx = new int[_workers.Count];

            D("Initialized " + _tickfiles.Length + " tik files.");
            V(string.Join(Environment.NewLine, _tickfiles));
            
            // read in single tick just to get first time for user
            FillCache(1);

            // get total ticks represented by files
            _availticks = 0;
            foreach (SimWorker t in _workers)
                if (t.Workersec != null)
                    _availticks += t.Workersec.ApproxTicks;

            D("Approximately " + TicksPresent + " ticks to process...");
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
        }

        /// <summary>
        /// stops any running simulation and closes all data files
        /// </summary>
        public void Stop()
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

        #endregion Public Methods

        #region Private Methods

        private void CancelWorkers()
        {
            foreach (SimWorker t in _workers)
                t.CancelAsync();
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
        /// Fill cache by reading x amount of ticks ahead
        /// </summary>
        /// <param name="readahead">Amount of ticks to read ahead</param>
        private void FillCache(int readahead)
        {
            // start all the workers not running have them read 'readahead' ticks in advance
            for (int i = 0; i < _workers.Count; i++)
            {
                // for some reason background worker is slow exiting, recreate
                if (_workers[i].IsBusy)
                {
                    V(_workers[i].Name + " worker#" + i + " is busy, waiting till free...");
                    // retry
                    while (_workers[i].IsBusy)
                    {
                        Thread.Sleep(10);
                    }
                    V(_workers[i].Name + " is no longer busy.");
                    Thread.Sleep(10);
                }
                _workers[i].RunWorkerAsync(readahead);
                V(_workers[i].Name + " worker#" + i + " now is working.");
            }
        }

        private void FillCacheSingleCore(int readhead)
        {
            // loop through instruments and read 'readahead' ticks in advance
            foreach (SimWorker t in _workers)
                t.SingleCoreFillCache(readhead);
        }

        private void FlushCache(long endsim)
        {
            bool simrunning = true;
            while (simrunning)
            {
                // get next times of ticks in cache
                long[] times = Nexttimes();

                // copy our master index list into a temporary for sorting
                Buffer.BlockCopy(_idx, 0, _cidx, 0, _idx.Length * 4);

                // sort loaded instruments by time
                // TODO: come up with a faster sort algorithm to speed up backtesting (what if we do not need to sort?)
                // Everything above this statement should be above the while loop to increase speed
                Array.Sort(times, _cidx);
                int nextidx = 0;

                // get next time from all instruments we have loaded
                while ((nextidx < times.Length) && (times[nextidx] == -1))
                    nextidx++;

                // test to see if ticks left in simulation
                bool ticksleft = nextidx < times.Length;
                bool simtimeleft = ticksleft && (times[nextidx] <= endsim);
                simrunning = ticksleft && simtimeleft;

                // if no ticks left or we exceeded simulation time, quit
                if (!simrunning)
                {
                    if (!ticksleft)
                        V("No ticks left.");
                    if (!simtimeleft)
                        V("Hit end of simulation.");

                    break;
                }

                // get next tick
                Tick k = _workers[_cidx[nextidx]].NextTick();

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

                // update timescale
                // times[nextidx] = Workers[cidx[nextidx]].hasTicks ? Workers[cidx[nextidx]].NextTime() : COMPLETED;
            }
            V("simulating exiting.");
        }

        private void FlushCacheSingleCore(long endsim)
        {
            bool simrunning = true;
            while (simrunning)
            {
                // get next ticks
                FillCacheSingleCore(1);

                // get next times of ticks in cache
                long[] times = Nexttimes();

                // copy our master index list into a temporary for sorting
                Buffer.BlockCopy(_idx, 0, _cidx, 0, _idx.Length * 4);

                // sort loaded instruments by time
                Array.Sort(times, _cidx);
                int nextidx = 0;

                // get next time from all instruments we have loaded
                while ((nextidx < times.Length) && (times[nextidx] == -1))
                    nextidx++;

                // test to see if ticks left in simulation
                simrunning = (nextidx < times.Length) && (times[nextidx] <= endsim);

                // if no ticks left or we exceeded simulation time, quit
                if (!simrunning)
                    break;

                // get next tick
                Tick k = _workers[_cidx[nextidx]].NextTick();

                // notify tick
                if (GotTick != null) GotTick(k);

                // count tick
                _tickcount++;
            }
        }

        private static int[] Genidx(int length)
        {
            int[] idx = new int[length]; for (int i = 0; i < length; i++) idx[i] = i; return idx;
        }

        private SecurityImpl GetSecurity(int tickfileidx)
        {
            return GetSecurity(_tickfiles[tickfileidx]);
        }

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

        private long[] Nexttimes()
        {
            // setup a next entry for every instrument
            long[] times = new long[_workers.Count];

            // loop through instrument's next time, set flag if no more ticks left in cache
            for (int i = 0; i < times.Length; i++)
            {
                // loop until worker has ticks
                while (!_workers[i].HasTicks)
                {
                    // or the worker is done reading tick stream
                    if (!_workers[i].IsWorking)
                    {
                        break;
                    }
                    
                    // if we're not done, wait for the I/O thread to catch up
                    Thread.Sleep(_yieldtime);
                }
                
                // we should either have ticks or be finished with this worker, set the value of
                // this worker's next time value accordingly
                times[i] = _workers[i].HasTicks ? _workers[i].NextTime() : Completed;
            }
            return times;
        }

        private void SecurityPlayTo(long ftime)
        {
            // see if we can truely thread or not
            if (Environment.ProcessorCount > 1)
            {
                // start all the workers reading files in background
                FillCache(int.MaxValue);

                // wait a moment to allow tick reading to start
                Thread.Sleep(_cachepause);

                // continuously loop through next ticks, playing most recent ones, until simulation
                // end is reached.
                FlushCache(ftime);

                // when we end simulation, stop reading but don't touch buffer
                CancelWorkers();
            }
            else // if we're a single core machine, add some delays
            {
                // continuously loop through next ticks sequentially, playing most recent ones,
                // until simulation end is reached.
                FlushCacheSingleCore(ftime);
            }

            // set next tick time as hint to user
            Setnexttime();
            // mark end of simulation
            _simend = DateTime.Now.Ticks;
        }

        private void Setnexttime()
        {
            // get next times of ticks in cache
            long[] times = Nexttimes();
            int i = 0;

            // get first one available
            while ((i < times.Length) && (times[i] == Completed))
                i++;

            // set next time to first available time, or end of simulation if none available
            _nextticktime = i == times.Length ? Endsim : times[i];
        }

        private void V(string msg)
        {
            _log.Trace("[MultiSimImpl] " + _lasttime + " " + msg);
        }

        #endregion Private Methods
    }
}