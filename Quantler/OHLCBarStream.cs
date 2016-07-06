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

using Quantler.Data.Bars;
using Quantler.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Quantler
{
    /// <summary>
    /// A stream that is used for internal data and creates OHLC bars
    /// </summary>
    public class OHLCBarStream : DataStream
    {
        #region Private Fields

        /// <summary>
        /// Requested intervals in integer
        /// </summary>
        private readonly List<int> _requested = new List<int>();

        /// <summary>
        /// Barlist data container
        /// </summary>
        private BarListImpl _bdata;

        /// <summary>
        /// Default interval = 5 minutes
        /// </summary>
        private int _default = (int)BarInterval.FiveMin;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Add a new local datastream using a security
        /// </summary>
        /// <param name="security"></param>
        /// <param name="Interval"></param>
        public OHLCBarStream(ISecurity security)
        {
            Construct(security, new int[] { });
        }

        /// <summary>
        /// Add a new local datastream using a security and an interval in seconds (int)
        /// </summary>
        /// <param name="security"></param>
        /// <param name="interval"></param>
        public OHLCBarStream(ISecurity security, int interval)
        {
            Construct(security, new[] { interval });
        }

        /// <summary>
        /// Add a new local datastream using a security and a collection of intervals in seconds (int)
        /// </summary>
        /// <param name="security"></param>
        /// <param name="intervals"></param>
        public OHLCBarStream(ISecurity security, params int[] intervals)
        {
            Construct(security, intervals);
        }

        /// <summary>
        /// Add a new local datastream using a security and an interval from default selections
        /// </summary>
        /// <param name="security"></param>
        /// <param name="interval"></param>
        public OHLCBarStream(ISecurity security, BarInterval interval)
        {
            Construct(security, new[] { (int)interval });
        }

        /// <summary>
        /// Add a new local datastream using a security and an interval from default selections
        /// </summary>
        /// <param name="security"></param>
        /// <param name="Interval"></param>
        public OHLCBarStream(ISecurity security, params BarInterval[] intervals)
        {
            Construct(security, intervals.Select(x => (int)x).ToArray());
        }

        /// <summary>
        /// Add a new local datastream using a security and an interval from timespan
        /// </summary>
        /// <param name="security"></param>
        /// <param name="interval"></param>
        public OHLCBarStream(ISecurity security, TimeSpan interval)
        {
            Construct(security, new[] { (int)interval.TotalSeconds });
        }

        /// <summary>
        /// Add a new local datastream using a security and a collection of intervals from timespan
        /// </summary>
        /// <param name="security"></param>
        /// <param name="Interval"></param>
        public OHLCBarStream(ISecurity security, params TimeSpan[] intervals)
        {
            Construct(security, intervals.Select(x => (int)x.TotalSeconds).ToArray());
        }

        #endregion Public Constructors

        #region Public Events

        /// <summary>
        /// Event is fired whenever a new bar is created on one of the timeframes
        /// </summary>
        public event SymBarIntervalDelegate GotNewBar;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Currently available and selected intervals for this data
        /// </summary>
        int[] BarListTracker.Intervals
        {
            get { return _requested.ToArray(); }
        }

        /// <summary>
        /// Currently available and selected intervals for this data
        /// </summary>
        public int[] CustomIntervals
        {
            get { return _requested.ToArray(); }
        }

        /// <summary>
        /// Set a new default interval, or request the default interval in timespan format
        /// </summary>
        public TimeSpan DefaultInterval
        {
            get
            {
                return TimeSpan.FromSeconds(_default);
            }
            set
            {
                _default = (int)value.TotalSeconds;
            }
        }

        /// <summary>
        /// End date and time for a backtesting, the end date is the last bar created dat for live trading
        /// </summary>
        public DateTime EndDateTime
        {
            get;
            set;
        }

        /// <summary>
        /// Unique id for this datastream
        /// </summary>
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// Current bar intervals in default format
        /// </summary>
        public BarInterval[] Intervals
        {
            get { return BarListImpl.Int2BarInterval(_requested.ToArray()); }
        }

        /// <summary>
        /// Associated security for this datastream
        /// </summary>
        public ISecurity Security
        {
            get;
            set;
        }

        /// <summary>
        /// The start date and time for this datastream, when live trading, this is the first known bar for this Bar List
        /// </summary>
        public DateTime StartDateTime
        {
            get;
            set;
        }

        /// <summary>
        /// Amount of symbols associated to this datastream
        /// </summary>
        public int SymbolCount
        {
            get { return 1; }
        }

        #endregion Public Properties

        #region Private Properties

        private bool IsInitialized { get { return _bdata != null; } }

        #endregion Private Properties

        #region Public Indexers

        /// <summary>
        /// Retrieve a barlist from a specific interval in seconds
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public BarList this[int interval]
        {
            get
            {
                _bdata.SetDefaultInterval(BarInterval.CustomTime, interval);
                return _bdata;
            }
        }

        /// <summary>
        /// Retrieve a barlist from a specific interval in timespan format
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public BarList this[TimeSpan interval]
        {
            get { return this[(int)interval.TotalSeconds]; }
        }

        public BarList this[BarInterval interval]
        {
            get
            {
                return this[(int)interval];
            }
        }

        #endregion Public Indexers

        #region Public Methods

        /// <summary>
        /// Add a new interval in seconds format (int)
        /// </summary>
        /// <param name="interval"></param>
        public void AddInterval(int interval)
        {
            if (_default == (int)BarInterval.FiveMin)
                _default = interval;

            if (!_requested.Contains(interval))
                _requested.Add(interval);

            if (IsInitialized)
                _bdata.AddInterval(interval, BarInterval.CustomTime);
        }

        /// <summary>
        /// Simultaniously add multiple intervals in seconds format (int)
        /// </summary>
        /// <param name="intervals"></param>
        public void AddInterval(int[] intervals)
        {
            for (int i = 0; i < intervals.Length; i++)
                AddInterval(intervals[i]);
        }

        /// <summary>
        /// Add interval in default format
        /// </summary>
        /// <param name="interval"></param>
        public void AddInterval(BarInterval interval)
        {
            AddInterval((int)interval);
        }

        /// <summary>
        /// Add an interval in timespan data format
        /// </summary>
        /// <param name="interval"></param>
        public void AddInterval(TimeSpan interval)
        {
            AddInterval((int)interval.TotalSeconds);
        }

        public IEnumerator GetEnumerator()
        {
            return null;
        }

        /// <summary>
        /// Process a new tick for this DataStream
        /// </summary>
        /// <param name="k"></param>
        public void GotTick(Tick k)
        {
            _bdata.NewTick(k);
        }

        /// <summary>
        /// Initialize this DataStraem
        /// </summary>
        public void Initialize()
        {
            _bdata = new BarListImpl(Security.Name, _requested.ToArray(), _requested.Select(x => BarInterval.CustomTime).ToArray());
            _bdata.GotNewBar += bl_GotNewBar;
        }

        /// <summary>
        /// Add a new ask to this DataStream
        /// </summary>
        /// <param name="k"></param>
        public void NewAsk(Tick k)
        {
            if (!k.HasAsk) return;
            NewPoint(k.Symbol, k.Ask, k.Time, k.Date, k.AskSize);
        }

        /// <summary>
        /// Add a new bid to this DataStream
        /// </summary>
        /// <param name="k"></param>
        public void NewBid(Tick k)
        {
            if (!k.HasBid) return;
            NewPoint(k.Symbol, k.Bid, k.Time, k.Date, k.BidSize);
        }

        /// <summary>
        /// Add a new data point to this DataStream
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="p"></param>
        /// <param name="time"></param>
        /// <param name="date"></param>
        /// <param name="size"></param>
        public void NewPoint(string symbol, decimal p, int time, int date, int size)
        {
            _bdata.NewPoint(symbol, p, time, date, size);
        }

        public void Reset()
        {
            _bdata.Reset();
        }

        #endregion Public Methods

        #region Private Methods

        private void bl_GotNewBar(string symbol, int interval)
        {
            if (GotNewBar != null)
                GotNewBar(symbol, interval);
        }

        private void Construct(ISecurity security, int[] intervals)
        {
            Security = security;
            AddInterval(intervals);
        }

        #endregion Private Methods
    }
}