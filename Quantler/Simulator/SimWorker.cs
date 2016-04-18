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

using Quantler.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Quantler.Securities;

namespace Quantler.Simulator
{
    // reads ticks from file into queue
    internal class SimWorker : BackgroundWorker
    {
        #region Public Fields

        public bool IsWorking;
        public SecurityImpl Workersec;

        #endregion Public Fields

        #region Private Fields

        private bool _lastworking;
        private volatile int _readcount;
        private readonly Queue<Tick> _ticks = new Queue<Tick>(100000);

        #endregion Private Fields

        #region Public Constructors

        public SimWorker(SecurityImpl sec)
        {
            Workersec = sec;
            WorkerSupportsCancellation = true;
            RunWorkerCompleted += simworker_RunWorkerCompleted;
            // if we're multi-core prepare to start I/O thread for this security
            if (Environment.ProcessorCount > 1)
            {
                DoWork += simworker_DoWork;
                Workersec.HistSource.GotTick += HistSource_gotTick2;
            }
            else
            {
                Workersec.HistSource.GotTick += HistSource_gotTick;
            }
        }

        #endregion Public Constructors

        #region Public Properties

        public bool HasTicks { get { lock (_ticks) { return _ticks.Count > 0; } } }

        public bool IsWorkingChange { get { bool r = _lastworking != IsWorking; _lastworking = IsWorking; return r; } }

        public string Name { get { return Workersec.Name + Workersec.Date; } }

        #endregion Public Properties

        #region Public Methods

        public Tick NextTick()
        {
            lock (_ticks) { return _ticks.Dequeue(); }
        }

        public long NextTime()
        {
            return _ticks.Peek().Datetime;
        }

        // here is cache filling for single core
        public void SingleCoreFillCache(int readahead)
        {
            IsWorking = true;
            _lastworking = true;
            _readcount = 0;
            while (!CancellationPending && Workersec.HistSource.NextTick()
                && (_readcount++ < readahead)) ;
            IsWorking = false;
        }

        #endregion Public Methods

        #region Private Methods

        private void HistSource_gotTick(Tick t)
        {
            _ticks.Enqueue(t);
        }

        private void HistSource_gotTick2(Tick t)
        {
            lock (_ticks)
            {
                _ticks.Enqueue(t);
            }
        }

        // fill cache for multi-core
        private void simworker_DoWork(object sender, DoWorkEventArgs e)
        {
            IsWorking = true;
            _lastworking = true;
            int readahead = (int)e.Argument;
            // while simulation hasn't been canceled, we still have historical ticks to read and we
            // haven't read too many, cache a tick
            while (!e.Cancel && Workersec.NextTick()
                   && (_readcount++ < readahead)) ;
        }

        // this is run when I/O thread completes on multi core
        private void simworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // reset counts
            _readcount = 0;
            // mark as done
            IsWorking = false;
            Dispose();
        }

        #endregion Private Methods
    }
}