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
    public class TickIntervalData : IntervalData
    {
        #region Internal Fields

        internal List<decimal> Closes = new List<decimal>();
        internal List<int> Dates = new List<int>();
        internal List<decimal> Highs = new List<decimal>();
        internal bool _IsRecentNew;
        internal List<decimal> Lows = new List<decimal>();
        internal int Maxhistory = 200;
        internal List<decimal> Opens = new List<decimal>();

        internal List<int> Ticks = new List<int>();

        internal List<int> Times = new List<int>();

        internal List<long> Vols = new List<long>();

        #endregion Internal Fields

        #region Private Fields

        private readonly int _intervallength;
        private readonly int _intervaltype = (int)BarInterval.CustomTicks;
        private int _currBarid = -1;

        #endregion Private Fields

        #region Public Constructors

        public TickIntervalData(int unitsPerInterval)
        {
            _intervallength = unitsPerInterval;
        }

        #endregion Public Constructors

        #region Public Events

        public event SymBarIntervalDelegate NewBar;

        #endregion Public Events

        #region Public Properties

        public int IntSize { get { return _intervallength; } }

        public int IntType { get { return _intervaltype; } }

        public int MaxHistory { get { return Maxhistory; } set { Maxhistory = value; } }

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
            Lows.Add(mybar.Close);
            Vols.Add(mybar.Volume);
            Times.Add(mybar.Bartime);
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
            // if we have no bars or we'll exceed our interval length w/this tick
            if ((_currBarid == -1) || (Ticks[_currBarid] == _intervallength))
            {
                // create a new one
                Newbar();
                // mark it
                _IsRecentNew = true;
                // make it current
                _currBarid++;
                // set time
                Times[Times.Count - 1] = time;
                // set date
                Dates[Dates.Count - 1] = date;
            }
            else _IsRecentNew = false;
            // blend tick into bar store value of Last
            int l = Last();
            // open
            if (Opens[l] == 0) Opens[l] = p;
            // high
            if (p > Highs[l]) Highs[l] = p;
            // low
            if (p < Lows[l]) Lows[l] = p;
            // close
            Closes[l] = p;
            // count ticks
            Ticks[l]++;
            // don't set volume for index
            if (size > 0)
                Vols[l] += size;
            // notify barlist
            if (_IsRecentNew && NewBar != null)
                NewBar(symbol, _intervallength);
        }

        public void NewTick(Tick k)
        {
            // ignore quotes
            if (k.Trade == 0) return;
            // if we have no bars or we'll exceed our interval length w/this tick
            if ((_currBarid == -1) || (Ticks[_currBarid] == _intervallength))
            {
                // create a new one
                Newbar();
                // mark it
                _IsRecentNew = true;
                // make it current
                _currBarid++;
                // set time
                Times[Times.Count - 1] = k.Time;
                // set date
                Dates[Dates.Count - 1] = k.Date;
            }
            else _IsRecentNew = false;
            // blend tick into bar store value of Last
            int l = Last();
            // open
            if (Opens[l] == 0) Opens[l] = k.Trade;
            // high
            if (k.Trade > Highs[l]) Highs[l] = k.Trade;
            // low
            if (k.Trade < Lows[l]) Lows[l] = k.Trade;
            // close
            Closes[l] = k.Trade;
            // count ticks
            Ticks[l]++;
            // don't set volume for index
            if (k.Size > 0)
                Vols[l] += k.Size;
            // notify barlist
            if (_IsRecentNew && NewBar != null)
                NewBar(k.Symbol, _intervallength);
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

        #region Private Methods

        private void Newbar()
        {
            Ticks.Add(0);
            Opens.Add(0);
            Closes.Add(0);
            Highs.Add(0);
            Lows.Add(decimal.MaxValue);
            Vols.Add(0);
            Times.Add(0);
            Dates.Add(0);

            //Check if we need to remove old bars to safe on memory usage
            if (Opens.Count > Maxhistory)
            {
                int deleteto = Opens.Count - Maxhistory;
                Opens.RemoveRange(0, deleteto);
                Closes.RemoveRange(0, deleteto);
                Highs.RemoveRange(0, deleteto);
                Lows.RemoveRange(0, deleteto);
                Vols.RemoveRange(0, deleteto);
                Times.RemoveRange(0, deleteto);
                Dates.RemoveRange(0, deleteto);
                Ticks.RemoveRange(0, deleteto);
            }
        }

        #endregion Private Methods
    }
}