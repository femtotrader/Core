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
using System.Collections.Generic;

namespace Quantler.Data.Bars
{
    public class TimeIntervalData : IntervalData
    {
        #region Public Fields

        public static bool IsOldTickBackfillEnabled = true;

        #endregion Public Fields

        #region Internal Fields

        internal bool _IsRecentNew;
        internal List<decimal> Closes = new List<decimal>();
        internal List<int> Dates = new List<int>();
        internal List<decimal> Highs = new List<decimal>();
        internal List<long> Ids = new List<long>();
        internal List<decimal> Lows = new List<decimal>();

        internal List<decimal> Opens = new List<decimal>();

        internal List<int> Ticks = new List<int>();

        internal List<int> Times = new List<int>();

        internal List<long> Vols = new List<long>();

        #endregion Internal Fields

        #region Private Fields

        private readonly int _intervallength;
        private readonly int _intervaltype = (int)BarInterval.CustomTime;
        private long _currBarid = -1;
        private int _maxbars = 200;

        #endregion Private Fields

        #region Public Constructors

        public TimeIntervalData(int unitsPerInterval)
        {
            _intervallength = unitsPerInterval;
            PriceType = BarPriceType.Bid;
        }

        #endregion Public Constructors

        #region Public Events

        public event SymBarIntervalDelegate NewBar;

        #endregion Public Events

        #region Public Properties

        public int IntSize { get { return _intervallength; } }

        public int IntType { get { return _intervaltype; } }

        public int MaxHistory { get { return _maxbars; } set { _maxbars = value; } }

        /// <summary>
        /// Determine price type bars are based on (for tick data only)
        /// </summary>
        public BarPriceType PriceType { get; set; }

        #endregion Public Properties

        #region Public Methods

        public void AddBar(Bar mybar)
        {
            Closes.Add(mybar.Close);
            Opens.Add(mybar.Open);
            Dates.Add(mybar.Bardate);
            Highs.Add(mybar.High);
            Lows.Add(mybar.Low);
            Vols.Add(mybar.Volume);
            Times.Add(mybar.Bartime);
            Ids.Add(Getbarid(mybar.Bartime, mybar.Bardate, _intervallength));
        }

        public List<decimal> Close()
        {
            return Closes;
        }

        public int Count()
        {
            return Opens.Count;
        }

        public List<int> Date()
        {
            return Dates;
        }

        public Bar GetBar(int index, string symbol)
        {
            BarImpl b = new BarImpl();
            if (index >= Count()) return b;
            if (index < 0)
            {
                index = Count() - 1 + index;
                if (index < 0) return b;
            }
            b = new BarImpl(Opens[index], Highs[index], Lows[index], Closes[index], Vols[index], Dates[index], Times[index], symbol, _intervaltype, _intervallength);
            if (index == Last()) b.IsNew = _IsRecentNew;
            return b;
        }

        public Bar GetBar(string symbol)
        {
            return GetBar(Last(), symbol);
        }

        public List<decimal> High()
        {
            return Highs;
        }

        public bool IsRecentNew()
        {
            return _IsRecentNew;
        }

        public int Last()
        {
            return Count() - 1;
        }

        public List<decimal> Low()
        {
            return Lows;
        }

        public void NewPoint(string symbol, decimal p, int time, int date, int size)
        {
            Appendpoint(symbol, p, time, date, size);
        }

        public void NewTick(Tick k)
        {
            decimal price = k.Trade;

            if (k.IsQuote)
            {
                //Check for price usage
                if (!k.IsTrade && PriceType == BarPriceType.Ask)
                    price = k.Ask;
                else if (!k.IsTrade && PriceType == BarPriceType.Bid)
                    price = k.Bid;
                else if (!k.IsTrade && PriceType == BarPriceType.MidPoint)
                    price = (k.Bid + k.Ask) / 2;
            }

            NewPoint(k.Symbol, price, k.Time, k.Date, k.Size);
        }

        public List<decimal> Open()
        {
            return Opens;
        }

        public void Reset()
        {
            Opens.Clear();
            Closes.Clear();
            Highs.Clear();
            Lows.Clear();
            Dates.Clear();
            Times.Clear();
            Vols.Clear();
        }

        public List<int> Tick()
        {
            return Ticks;
        }

        public List<int> Time()
        {
            return Times;
        }

        public List<long> Vol()
        {
            return Vols;
        }

        #endregion Public Methods

        #region Internal Methods

        internal static long Getbarid(int time, int date, int intervallength)
        {
            // get time elapsed to this point
            int elap = Util.Ft2Fts(time);
            // get number of this bar in the day for this interval
            long bcount = (int)((double)elap / (intervallength * 1000));
            // add the date to the front of number to make it unique
            bcount += (long)date * 100000;
            return bcount;
        }

        #endregion Internal Methods

        #region Private Methods

        private void Appendpoint(string symbol, decimal p, int time, int date, int size)
        {
            // get the barcount
            long barid = Getbarid(time, date, _intervallength); //TODO: include date as a measure, this is more accurate and allows for timeframes such as 1 day 12:01:12
            int index;
            // if not current bar
            if (barid == _currBarid)
            {
                _IsRecentNew = false;
                index = Last();
            }
            // if bar is a new one
            else if (barid > _currBarid)
            {
                // create a new one
                Newbar(barid);
                // mark it
                _IsRecentNew = true;
                // make it current
                _currBarid = barid;
                // set time
                Times[Times.Count - 1] = time;
                // set date
                Dates[Dates.Count - 1] = date;
                index = Last();
            }
            else // otherwise it's a backfill
            {
                _IsRecentNew = false;

                // find the appropriate index to insert the bar (by id)
                int place = 0;
                bool found = false;
                for (int x = 0; x <= Ids.Count; x++)
                {
                    // if the bar already exists
                    if (Ids[x] == barid)
                    {
                        place = x;
                        found = true;
                        break;
                    }
                    // if there's a gap where the bar should exist
                    if (Ids[x] > barid)
                    {
                        place = x;
                        break;
                    }
                }
                // older than oldest
                if (!found)
                {
                    Opens.Insert(place, 0);
                    Closes.Insert(place, 0);
                    Highs.Insert(place, 0);
                    Lows.Insert(place, decimal.MaxValue);
                    Vols.Insert(place, 0);
                    Times.Insert(place, time);
                    Dates.Insert(place, date);
                    Ids.Insert(place, barid);
                }

                index = place;
            }
            // blend tick into bar open
            if (Opens[index] == 0)
                Opens[index] = p;
            // high
            if (p > Highs[Last()])
                Highs[Last()] = p;
            // low
            if (p < Lows[index])
                Lows[index] = p;
            // close
            Closes[index] = p;
            // volume
            if (p >= 0)
                Vols[index] += size;
            // notify barlist
            if (_IsRecentNew && NewBar != null)
                NewBar(symbol, _intervallength);
        }

        private void Newbar(long id)
        {
            Opens.Add(0);
            Closes.Add(0);
            Highs.Add(0);
            Lows.Add(decimal.MaxValue);
            Vols.Add(0);
            Times.Add(0);
            Dates.Add(0);
            Ids.Add(id);

            //Check if we need to remove old bars to safe on memory usage
            if (Opens.Count > _maxbars)
            {
                int deleteto = Opens.Count - _maxbars;
                Opens.RemoveRange(0, deleteto);
                Closes.RemoveRange(0, deleteto);
                Highs.RemoveRange(0, deleteto);
                Lows.RemoveRange(0, deleteto);
                Vols.RemoveRange(0, deleteto);
                Times.RemoveRange(0, deleteto);
                Dates.RemoveRange(0, deleteto);
                Ids.RemoveRange(0, deleteto);
            }
        }

        #endregion Private Methods
    }
}