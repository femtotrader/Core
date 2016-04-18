/*
Copyright (c) Quantler B.V., All rights reserved.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 3.0 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.
*/

using Quantler.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Quantler.Broker;
using Quantler.Data.TikFile;
using Quantler.Data;
using System.Linq;

namespace Quantler.Simulator
{
    public class HistorySimImpl : HistSim
    {
        #region Public Fields

        public static long ENDSIM = long.MaxValue;

        public static long STARTSIM = long.MinValue;

        #endregion Public Fields

        #region Private Fields

        private string[] _tickfiles = new string[0];

        private long simend = 0;

        private long simstart = 0;

        private bool initialized = false;

        private SecurityImpl[] datafiles = new SecurityImpl[0];

        private List<SimWorker> Workers = new List<SimWorker>();

        #endregion Private Fields

        #region Public Constructors

        public HistorySimImpl(string[] filenames)
        {
            _tickfiles = filenames;
        }

        public HistorySimImpl(SecurityImpl[] datafiles)
        {
            this.datafiles = datafiles;
        }

        #endregion Public Constructors

        #region Public Events

        public event DebugDelegate GotDebug;

        public event TickDelegate GotTick;

        #endregion Public Events

        #region Public Properties

        public long NextTickTime
        {
            get { throw new NotImplementedException(); }
        }

        public int TicksPresent
        {
            get { throw new NotImplementedException(); }
        }

        public int TicksProcessed
        {
            get { throw new NotImplementedException(); }
        }

        #endregion Public Properties

        #region Private Properties

        private TimeSpan runtime
        {
            get
            {
                DateTime start = new DateTime(simstart);
                DateTime end = new DateTime(simend);
                return end.Subtract(start);
            }
        }

        #endregion Private Properties

        #region Public Methods

        public void PlayTo(long ftime)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reinitialize the cache
        /// </summary>
        public void Initialize()
        {
            if (initialized) return; // only init once

            // now we have our list, initialize instruments from files
            if (this.datafiles.Length == 0)
            {
                for (int i = 0; i < _tickfiles.Length; i++)
                {
                    try
                    {
                        SecurityImpl s = getsec(i);
                        if ((s != null) && s.isValid && s.HistSource.isValid)
                            Workers.Add(new SimWorker(s));
                    }
                    catch (Exception ex)
                    {
                        debug("Unable to initialize: " + _tickfiles[i] + " error: " + ex.Message + ex.StackTrace);
                        continue;
                    }
                }
            }

            // setup our initial index
            idx = genidx(Workers.Count);
            cidx = new int[Workers.Count];

            debug("Initialized " + (_tickfiles.Length) + " instruments.");
            debug(string.Join(Environment.NewLine.ToString(), _tickfiles));
            // read in single tick just to get first time for user
            FillCache(1);

            // get total ticks represented by files
            _availticks = 0;
            for (int i = 0; i < Workers.Count; i++)
                if (Workers[i].workersec != null)
                    _availticks += Workers[i].workersec.ApproxTicks;

            debug("Approximately " + TicksPresent + " ticks to process...");
            _inited = true;
            // set first time as hint for user
            setnexttime();
        }

        #endregion Public Methods

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

                    return SecurityImpl.FromZIP(zipfile, tickfile);
                }
                else
                    return SecurityImpl.FromTIK(file);
            }
            catch (Exception ex)
            {
                debug("error reading TIK file: " + file + " err: " + ex.Message + ex.StackTrace);
                return null;
            }
        }

        private void debug(string msg)
        {
            if (GotDebug != null)
                GotDebug(msg);
        }
    }
}