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
using Quantler.Data.Ticks;
using Quantler.Interfaces;
using Quantler.Tracker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Quantler.Securities;

namespace Quantler.Data.Bars
{
    /// <summary>
    /// Holds a succession of bars. Will accept ticks and automatically create new bars as needed.
    /// </summary>
    public class BarListImpl : BarList
    {
        #region Private Fields

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        private static BarListImpl _fromepf;
        private static bool _usebid = true;

        private static bool _uselast = true;

        private readonly List<int> _custintervals;

        // holds all raw data
        private readonly List<IntervalData> _intdata;

        // holds available intervals
        private readonly int[] _intervaltypes;

        private readonly GenericTrackerImpl<int> _typesize2Idx = new GenericTrackerImpl<int>();

        private int _curintervalidx;

        // gets or sets the default interval in seconds
        private int _defaultcustint;

        private int _defaultint = (int)BarInterval.FiveMin;

        private string _sym;

        private int _symh;

        private bool _valid;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// creates barlist with defined symbol and requests all intervals
        /// </summary>
        /// <param name="symbol"></param>
        public BarListImpl(string symbol) : this(symbol, Allintervals) { }

        /// <summary>
        /// creates a barlist with requested interval and defined symbol
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="symbol"></param>
        public BarListImpl(BarInterval interval, string symbol) : this(symbol, new[] { interval }) { }

        /// <summary>
        /// creates a barlist with requested custom interval and defined symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        public BarListImpl(string symbol, int interval) : this(symbol, interval, BarInterval.CustomTime) { }

        /// <summary>
        /// creates a barlist with custom interval and a custom type (tick/vol)
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="type"></param>
        public BarListImpl(string symbol, int interval, BarInterval type) : this(symbol, new[] { interval }, new[] { type }) { }

        /// <summary>
        /// creates a barlist with requested interval.  symbol will be defined by first tick received
        /// </summary>
        /// <param name="interval"></param>
        public BarListImpl(BarInterval interval) : this(string.Empty, new[] { interval }) { }

        /// <summary>
        /// creates barlist with no symbol defined and requests 5min bars
        /// </summary>
        public BarListImpl() : this(string.Empty, new[] { BarInterval.FiveMin, BarInterval.Minute, BarInterval.Hour, BarInterval.ThirtyMin, BarInterval.FifteenMin, BarInterval.Day }) { }

        /// <summary>
        /// creates barlist with specified symbol and requested intervals
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="intervals"></param>
        public BarListImpl(string symbol, BarInterval[] intervals) : this(symbol, BarInterval2Int(intervals), intervals) { }

        /// <summary>
        /// creates a barlist with array of custom intervals
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="custintervals"></param>
        /// <param name="types"></param>
        public BarListImpl(string symbol, int[] custintervals, BarInterval[] types)
        {
            // set symbol
            _sym = symbol;
            _custintervals = custintervals.ToList();
            // set intervals requested
            _intervaltypes = BarInterval2Int(types);
            // size length of interval data to # of requested intervals
            _intdata = new List<IntervalData>(custintervals.Length);
            // index the pairs
            _typesize2Idx.Clear();
            // create interval data object for each interval
            for (int i = 0; i < _intervaltypes.Length; i++)
            {
                AddInterval(_custintervals[i], types[i]);
            }
        }

        #endregion Public Constructors

        #region Public Events

        /// <summary>
        /// this event is thrown when a new bar arrives
        /// </summary>
        public event SymBarIntervalDelegate GotNewBar;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// gets array of all possible non custom bar intevals
        /// </summary>
        public static BarInterval[] Allintervals { get { return new[] { BarInterval.FiveMin, BarInterval.Minute, BarInterval.Hour, BarInterval.ThirtyMin, BarInterval.FifteenMin, BarInterval.Day }; } }

        /// <summary>
        /// gets the # of bars in default interval
        /// </summary>
        public int Count { get { return _intdata[_curintervalidx].Count(); } }

        /// <summary>
        /// gets all available/requested intervals as a custom array of integers
        /// </summary>
        public int[] CustomIntervals { get { return _custintervals.ToArray(); } }

        public int DefaultCustomInterval
        {
            get
            {
                return _defaultcustint;
            }
            set
            {
                var nv = value;
                // verify present
                int foundidx = -1;
                for (int i = 0; i < _custintervals.Count; i++)
                {
                    if (nv == _custintervals[i])
                    {
                        foundidx = i;
                    }
                }
                if (foundidx >= 0)
                {
                    _curintervalidx = foundidx;
                    _defaultcustint = value;
                }
            }
        }

        /// <summary>
        /// gets or sets the default interval in bar intervals
        /// </summary>
        public BarInterval DefaultInterval
        {
            get
            {
                return Int2BarInterval(new[] { _defaultint })[0];
            }
            set
            {
                var nv = (int)value;
                // verify present
                int foundidx = -1;
                for (int i = 0; i < _intervaltypes.Length; i++)
                {
                    if (nv == _intervaltypes[i])
                    {
                        foundidx = i;
                    }
                }
                if (foundidx >= 0)
                {
                    _curintervalidx = foundidx;
                    _defaultint = nv;
                }
            }
        }

        /// <summary>
        /// index to current default interval pair (type/size)
        /// </summary>
        public int DefaultIntervalIndex
        {
            get { return _curintervalidx; }
            set
            {
                var nv = value;
                if ((nv >= 0) && (nv < _custintervals.Count))
                    _curintervalidx = nv;
            }
        }

        public bool DoubleIncludesRecent { get; set; }

        /// <summary>
        /// gets first bar in any interval
        /// </summary>
        public int First { get { return 0; } }

        /// <summary>
        /// gets intervals available/requested by this barlist when it was created
        /// </summary>
        public BarInterval[] Intervals { get { return Int2BarInterval(_intervaltypes); } }

        /// <summary>
        /// returns true if bar has symbol and has requested intervals
        /// </summary>
        public bool IsValid { get { return (_sym != string.Empty) && (_intdata.Count > 0); } }

        /// <summary>
        /// gets the last bar in default interval
        /// </summary>
        public int Last { get { return _intdata[_curintervalidx].Last(); } }

        /// <summary>
        /// gets most recent bar from default interval
        /// </summary>
        public Bar RecentBar { get { return this[Last]; } }

        // standard accessors
        /// <summary>
        /// symbol for bar
        /// </summary>
        public string Symbol { get { return _sym; } set { _sym = value; } }

        #endregion Public Properties

        #region Public Indexers

        /// <summary>
        /// gets specific bar in specified interval
        /// </summary>
        /// <param name="barnumber"></param>
        /// <returns></returns>
        public Bar this[int barnumber]
        {
            get
            {
                return _intdata[_curintervalidx].GetBar(barnumber, Symbol);
            }
        }

        /// <summary>
        /// gets a specific bar in specified interval
        /// </summary>
        /// <param name="barnumber"></param>
        /// <param name="interval"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public Bar this[int barnumber, BarInterval interval, int size] { get { var idx = Gettypesizeidx(interval, size); if (idx < 0) return new BarImpl(); return _intdata[idx].GetBar(barnumber, Symbol); } }

        /// <summary>
        /// gets a specific bar in specified seconds interval
        /// </summary>
        /// <param name="barnumber"></param>
        /// <param name="intervalidx"></param>
        /// <returns></returns>
        public Bar this[int barnumber, int intervalidx]
        {
            get
            {
                var found = _intdata.FirstOrDefault(x => x.IntSize == intervalidx);
                if (found != null)
                    return found.GetBar(barnumber, Symbol);
                return this[barnumber];
            }
        }

        #endregion Public Indexers

        #region Public Methods

        /// <summary>
        /// fill bars with arbitrary price data for a symbol
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="prices"></param>
        /// <param name="startdate"></param>
        /// <param name="starttime"></param>
        /// <param name="blt"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static bool Backfillbars(string sym, decimal[] prices, int startdate, int starttime, ref BarListTrackerImpl blt, int interval)
        {
            // ensure we have closing data
            if (prices.Length == 0)
            {
                Log.Debug(sym + " no price data provided/available, will have to wait until bars are created from market.");
                return false;
            }
            // make desired numbers of ticks
            DateTime n = Util.ToDateTime(startdate, starttime);
            bool ok = true;
            List<string> bfd = new List<string>();
            for (int i = prices.Length - 1; i >= 0; i--)
            {
                // get time now - exitlen*60
                var ndt = n.Subtract(new TimeSpan(0, i * interval, 0));
                int nt = Util.ToQLTime(ndt);
                int nd = Util.ToQLDate(ndt);
                Tick k = TickImpl.NewTrade(sym, nd, nt, prices[i], 100, string.Empty);
                ok &= k.IsValid && k.IsTrade;
                bfd.Add(nd + nt.ToString() + "=" + prices[i]);
                if (i <= 2)
                    Log.Debug(nd + " " + nt);
                blt.NewTick(k);
            }
            if (ok)
                Log.Debug("{0} bars backfilled using: {1} bars", sym, bfd.Count);
            return ok;
        }

        /// <summary>
        /// converts array of BarIntervals to integer intervals.
        /// </summary>
        /// <param name="ints"></param>
        /// <returns></returns>
        public static int[] BarInterval2Int(BarInterval[] ints)
        {
            return ints.Select(bi => (int)bi).ToArray();
        }

        /// <summary>
        /// get a barlist from tick data
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static BarList FromTIK(string filename) { return FromTIK(filename, true, true); }

        /// <summary>
        /// get a barlist from tick data and optionally use bid/ask data to construct bars
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="uselast"></param>
        /// <param name="usebid"></param>
        /// <returns></returns>
        public static BarList FromTIK(string filename, bool uselast, bool usebid)
        {
            _uselast = uselast;
            _usebid = usebid;
            SecurityImpl s = SecurityImpl.FromTik(filename);
            s.HistSource.GotTick += HistSource_gotTick;
            _fromepf = new BarListImpl(s.Name);
            while (s.HistSource.NextTick()) ;
            s.HistSource.Close();
            return _fromepf;
        }

        /// <summary>
        /// create barlist from a tik file using given intervals/types
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="uselast"></param>
        /// <param name="usebid"></param>
        /// <param name="intervals"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static BarList FromTIK(string filename, bool uselast, bool usebid, int[] intervals, BarInterval[] types)
        {
            _uselast = uselast;
            _usebid = usebid;
            SecurityImpl s = SecurityImpl.FromTik(filename);
            s.HistSource.GotTick += HistSource_gotTick;
            _fromepf = new BarListImpl(s.Name, intervals, types);
            while (s.HistSource.NextTick()) ;
            return _fromepf;
        }

        /// <summary>
        /// gets preceeding bar by time (assumes same day)
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static int GetBarIndexPreceeding(BarList chart, int date, int time)
        {
            // look for previous day's close
            for (int j = chart.Last; j >= chart.First; j--)
            {
                if (chart.Date()[j] > date) continue;
                if (chart.Time()[j] < time || date > chart.Date()[j])
                {
                    return j;
                }
            }
            // first bar
            return -1;
        }

        /// <summary>
        /// insert a bar at particular place in the list.
        /// REMEMBER YOU MUST REHANDLE GOTNEWBAR EVENT AFTER CALLING THIS.
        /// </summary>
        /// <param name="bl"></param>
        /// <param name="b"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static BarListImpl InsertBar(BarList bl, Bar b, int position)
        {
            BarListImpl copy = new BarListImpl(bl.Symbol, bl.CustomIntervals, bl.Intervals);
            // verify bar is valid
            if (!b.IsValid)
            {
                return copy;
            }
            for (int j = 0; j < bl.CustomIntervals.Length; j++)
            {
                if ((bl.CustomIntervals[j] != b.CustomInterval) && (bl.Intervals[j] != b.BarInterval))
                    continue;
                int count = bl.IntervalCount(b.BarInterval, b.CustomInterval);
                if (count < 0)
                    throw new Exception("bar has interval: " + b.BarInterval + "/" + b.CustomInterval + " not defined in your barlist for: " + bl.Symbol);
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (i == position)
                        {
                            Addbar(copy, b, j);
                        }
                        Addbar(copy, bl[i, b.BarInterval, b.CustomInterval], j);
                    }
                }
                else
                    Addbar(copy, b, j);
            }
            return copy;
        }

        /// <summary>
        /// converts integer array of intervals to BarIntervals... supplying custom interval for any unrecognized interval types.
        /// </summary>
        /// <param name="intervals"></param>
        /// <returns></returns>
        public static BarInterval[] Int2BarInterval(int[] intervals) { List<BarInterval> o = new List<BarInterval>(); foreach (int i in intervals) { try { BarInterval bi = (BarInterval)i; o.Add(bi); } catch (Exception) { o.Add(BarInterval.CustomTime); } } return o.ToArray(); }

        public void AddInterval(int interval, BarInterval barIntervalType)
        {
            // set default interval to first one
            if (_intdata.Count == 0)
            {
                _defaultint = (int)barIntervalType;
                _defaultcustint = interval;
                _curintervalidx = 0;
            }

            //Check if we are adding a known interval size, twice
            if (_intdata.Count(x => x.IntSize == interval) > 0)
                return;

            //Item to add
            IntervalData newstream;

            // save index to this size for the interval
            switch (barIntervalType)
            {
                case BarInterval.CustomTicks:
                    newstream = new TickIntervalData(interval);
                    break;

                case BarInterval.CustomTime:
                    newstream = new TimeIntervalData(interval);
                    break;

                default:
                    newstream = new TimeIntervalData(interval);
                    break;
            }

            //Add new item
            _intdata.Add(newstream);

            // subscribe to bar events
            newstream.NewBar += BarListImpl_NewBar;

            // index the pair
            _typesize2Idx.addindex((int)barIntervalType + interval.ToString(), _intdata.Count - 1);

            // add known interval
            if(!_custintervals.Contains(interval))
                _custintervals.Add(interval);
        }

        public decimal[] Close()
        {
            return _intdata[_curintervalidx].Close().ToArray();
        }

        public decimal[] Close(int intervalidx)
        {
            return _intdata[intervalidx].Close().ToArray();
        }

        public int[] Date()
        {
            return _intdata[_curintervalidx].Date().ToArray();
        }

        public int[] Date(int intervalidx)
        {
            return _intdata[intervalidx].Date().ToArray();
        }

        public IEnumerator GetEnumerator()
        {
            var data = _intdata[_curintervalidx];
            int max = data.Count();
            for (int i = 0; i < max; i++)
                yield return data.GetBar(i, Symbol);
        }

        /// <summary>
        /// returns true if barslist has at least minimum # of bars for specified interval
        /// </summary>
        /// <param name="minBars"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public bool Has(int minBars, BarInterval interval, int size)
        {
            var idx = Gettypesizeidx(interval, size);
            if (idx < 0)
                return false;
            return _intdata[idx].Count() >= minBars;
        }

        /// <summary>
        /// returns true if barlist has at least minimum # of bars for default interval
        /// </summary>
        /// <param name="minBars"></param>
        /// <returns></returns>
        public bool Has(int minBars) { return _intdata[_curintervalidx].Count() >= minBars; }

        public decimal[] High()
        {
            return _intdata[_curintervalidx].High().ToArray();
        }

        public decimal[] High(int intervalidx)
        {
            return _intdata[intervalidx].High().ToArray();
        }

        /// <summary>
        /// gets count for given bar interval
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public int IntervalCount(BarInterval interval, int size)
        {
            var idx = Gettypesizeidx(interval, size);
            // interval not in this tracker
            if (idx < 0)
                return -1;
            return _intdata[idx].Count();
        }

        public int IntervalCount(BarInterval interval)
        {
            var idx = Gettypesizeidx(interval, (int)interval);
            if (idx < 0)
                return -1;
            return _intdata[idx].Count();
        }

        public decimal[] Low()
        {
            return _intdata[_curintervalidx].Low().ToArray();
        }

        public decimal[] Low(int intervalidx)
        {
            return _intdata[intervalidx].Low().ToArray();
        }

        public void NewPoint(string symbol, decimal p, int time, int date, int size)
        {
            // add tick to every requested bar interval
            for (int i = 0; i < _intdata.Count; i++)
                _intdata[i].NewPoint(symbol, p, time, date, size);
        }

        public void NewTick(Tick k)
        {
            // make sure we have a symbol defined
            if (!_valid)
            {
                _symh = k.Symbol.GetHashCode();
                _sym = k.Symbol;
                _valid = true;
            }
            // make sure tick is from our symbol
            if (_symh != k.Symbol.GetHashCode()) return;
            // add tick to every requested bar interval
            for (int i = 0; i < _intdata.Count; i++)
                _intdata[i].NewTick(k);
        }

        // array functions
        public decimal[] Open() { return _intdata[_curintervalidx].Open().ToArray(); }

        public decimal[] Open(int intervalidx)
        {
            return _intdata[intervalidx].Open().ToArray();
        }

        /// <summary>
        /// erases all bar data
        /// </summary>
        public void Reset()
        {
            foreach (IntervalData id in _intdata)
            {
                id.Reset();
            }
        }

        public bool SetDefaultInterval(BarInterval type, int size)
        {
            bool found = false;
            for (int i = 0; i < _intervaltypes.Length; i++)
            {
                var it = (BarInterval)_intervaltypes[i];
                for (int x = 0; x < _custintervals.Count; x++)
                {
                    var ints = _custintervals[x];
                    if ((it == type) && (size == ints))
                    {
                        _curintervalidx = x;
                        return true;
                    }   
                }
            }

            //interval size is unkown thus add it
            AddInterval(size, type);

            //Return false
            return found;
        }

        public int[] Time()
        {
            return _intdata[_curintervalidx].Time().ToArray();
        }

        public int[] Time(int intervalidx)
        {
            return _intdata[intervalidx].Time().ToArray();
        }

        public long[] Vol()
        {
            return _intdata[_curintervalidx].Vol().ToArray();
        }

        #endregion Public Methods

        #region Internal Methods

        internal static void Addbar(BarListImpl b, Bar mybar, int instdataidx)
        {
            b._intdata[instdataidx].AddBar(mybar);
        }

        #endregion Internal Methods

        #region Private Methods

        /// <summary>
        /// Populate the day-interval barlist of this instance from a URL, where the results are returned as a CSV file.  URL should accept requests in the form of http://url/get.py?sym=IBM
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private static void HistSource_gotTick(Tick t)
        {
            if (_uselast)
                _fromepf.NewTick(t);
            else
            {
                if (t.HasAsk && !_usebid)
                    _fromepf.NewPoint(t.Symbol, t.Ask, t.Time, t.Date, t.AskSize);
                else if (t.HasBid && _usebid)
                    _fromepf.NewPoint(t.Symbol, t.Bid, t.Time, t.Date, t.BidSize);
            }
        }

        private void BarListImpl_NewBar(string symbol, int interval)
        {
            // if event is handled by user, pass the event
            if (GotNewBar != null)
                GotNewBar(symbol, interval);
        }

        private int Gettypesizeidx(BarInterval type, int size)
        {
            var types = ((int)type).ToString();
            return _typesize2Idx.getindex(types + size);
        }

        #endregion Private Methods
    }
}