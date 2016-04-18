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

using Quantler.Data.Bars;
using Quantler.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Quantler.Tracker
{
    /// <summary>
    /// used to track lists of bars for MANY symbols. BarListTracker (blt) will accept ticks and
    /// auto-create bars as barlists as needed. Access bars via blt["IBM"].RecentBar.Close
    /// </summary>
    public class BarListTrackerImpl : BarListTracker
    {
        #region Private Fields

        private readonly Dictionary<string, BarListImpl> _bdict = new Dictionary<string, BarListImpl>();

        /// <summary>
        /// default custom interval used by this tracker
        /// </summary>
        private readonly int _defaultcust = 0;

        private readonly BarInterval[] _reqtype;
        private readonly int[] _requested;
        private int _default;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// create a barlist tracker with all the intervals available (specify only intervals you
        /// need to get faster performance)
        /// </summary>
        public BarListTrackerImpl()
            : this(BarListImpl.Allintervals)
        {
        }

        public BarListTrackerImpl(BarInterval interval)
            : this(new[] { interval })
        {
        }

        /// <summary>
        /// creates tracker for single custom interval
        /// </summary>
        /// <param name="custominterval"></param>
        public BarListTrackerImpl(int custominterval)
            : this(new[] { custominterval }, new[] { BarInterval.CustomTime })
        {
        }

        /// <summary>
        /// creates tracker for number of custom intervals. (use this if you want to mix standard
        /// and custom intervals)
        /// </summary>
        /// <param name="customintervals"></param>
        public BarListTrackerImpl(int[] customintervals, BarInterval[] intervaltypes)
        {
            _default = (int)intervaltypes[0];
            _requested = customintervals;
            _reqtype = intervaltypes;
        }

        /// <summary>
        /// creates tracker for specified number of standard intervals
        /// </summary>
        /// <param name="intervals"></param>
        public BarListTrackerImpl(BarInterval[] intervals)
        {
            _requested = BarListImpl.BarInterval2Int(intervals);
            _reqtype = intervals;
            _default = (int)intervals[0];
        }

        #endregion Public Constructors

        #region Public Events

        public event SymBarIntervalDelegate GotNewBar;

        #endregion Public Events

        #region Public Properties

        TimeSpan BarListTracker.DefaultInterval
        {
            get { throw new NotImplementedException(); }
        }

        int[] BarListTracker.Intervals
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// custom bar intervals used by this tracker
        /// </summary>
        public int[] CustomIntervals { get { return _requested; } }

        public BarInterval DefaultInterval
        {
            get
            {
                return BarListImpl.Int2BarInterval(new[] { _default })[0];
            }
            set
            {
                _default = BarListImpl.BarInterval2Int(new[] { value })[0];
                foreach (string sym in _bdict.Keys)
                {
                    _bdict[sym].DefaultInterval = value;
                    _bdict[sym].DefaultCustomInterval = DefaultCustomInterval;
                }
            }
        }

        /// <summary>
        /// intervals requested when tracker was created
        /// </summary>
        public BarInterval[] Intervals { get { return _reqtype; } }

        public int SymbolCount { get { return _bdict.Count; } }

        #endregion Public Properties

        #region Private Properties

        private int DefaultCustomInterval { get { return _defaultcust; } }

        #endregion Private Properties

        #region Public Indexers

        /// <summary>
        /// gets barlist for a given symbol. will return an invalid barlist if no ticks have been
        /// received for symbol
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        public BarList this[string sym]
        {
            get
            {
                return this[sym, _default];
            }

            set
            {
                BarListImpl bl;
                if (_bdict.TryGetValue(sym, out bl))
                    _bdict[sym] = (BarListImpl)value;
                else
                    _bdict.Add(sym, (BarListImpl)value);
            }
        }

        public BarList this[string sym, int interval]
        {
            get
            {
                BarListImpl bl;
                if (_bdict.TryGetValue(sym, out bl))
                {
                    bl.DefaultCustomInterval = interval;
                    return bl;
                }
                bl = new BarListImpl(sym, _requested, _reqtype) { DefaultCustomInterval = interval };
                bl.GotNewBar += bl_GotNewBar;
                _bdict.Add(sym, bl);
                return bl;
            }
        }

        public BarList this[int interval]
        {
            get { throw new NotImplementedException(); }
        }

        public BarList this[TimeSpan interval]
        {
            get { throw new NotImplementedException(); }
        }

        #endregion Public Indexers

        #region Public Methods

        public IEnumerator GetEnumerator()
        {
            return _bdict.Keys.GetEnumerator();
        }

        public void GotTick(Tick k)
        {
            NewTick(k);
        }

        /// <summary>
        /// build bar with ask data rather than trades
        /// </summary>
        /// <param name="k"></param>
        public void NewAsk(Tick k)
        {
            if (!k.HasAsk) return;
            NewPoint(k.Symbol, k.Ask, k.Time, k.Date, k.AskSize);
        }

        /// <summary>
        /// build bar with bid data rather than trades
        /// </summary>
        /// <param name="k"></param>
        public void NewBid(Tick k)
        {
            if (!k.HasBid) return;
            NewPoint(k.Symbol, k.Bid, k.Time, k.Date, k.BidSize);
        }

        /// <summary>
        /// add any data point to bar
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="p"></param>
        /// <param name="time"></param>
        /// <param name="date"></param>
        /// <param name="size"></param>
        public void NewPoint(string symbol, decimal p, int time, int date, int size)
        {
            BarListImpl bl;
            if (!_bdict.TryGetValue(symbol, out bl))
            {
                bl = new BarListImpl(symbol, _requested, _reqtype) { DefaultCustomInterval = _default };
                bl.GotNewBar += bl_GotNewBar;
                _bdict.Add(symbol, bl);
            }
            bl.NewPoint(symbol, p, time, date, size);
        }

        /// <summary>
        /// give any ticks (trades) to this symbol and tracker will create barlists automatically
        /// </summary>
        /// <param name="k"></param>
        public void NewTick(Tick k)
        {
            BarListImpl bl;
            if (!_bdict.TryGetValue(k.Symbol, out bl))
            {
                bl = new BarListImpl(k.Symbol, _requested, _reqtype) { DefaultCustomInterval = _default };
                bl.GotNewBar += bl_GotNewBar;
                _bdict.Add(k.Symbol, bl);
            }
            bl.NewTick(k);
        }

        /// <summary>
        /// clears all data from tracker
        /// </summary>
        public void Reset()
        {
            foreach (BarListImpl bl in _bdict.Values)
                bl.Reset();
            _bdict.Clear();
        }

        #endregion Public Methods

        #region Private Methods

        // pass bar events out of the barlist tracker
        private void bl_GotNewBar(string symbol, int interval)
        {
            if (GotNewBar != null)
                GotNewBar(symbol, interval);
        }

        #endregion Private Methods
    }
}