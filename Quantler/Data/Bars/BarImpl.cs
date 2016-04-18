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

using Quantler.Data.Ticks;
using Quantler.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Quantler.Data.Bars
{
    /// <summary>
    /// A single bar of price data, which represents OHLC and volume for an interval of time.
    /// </summary>
    public class BarImpl : GotTickIndicator, Bar
    {
        #region Private Fields

        private readonly int _units = 300;

        private int _ci = -1;

        private decimal _h = decimal.MinValue;

        private decimal _l = decimal.MaxValue;

        private string _sym = "";

        #endregion Private Fields

        #region Public Constructors

        public BarImpl()
            : this(BarInterval.FiveMin)
        {
        }

        public BarImpl(decimal open, decimal high, decimal low, decimal close, long vol, int date, int time, string symbol, int interval)
            : this(open, high, low, close, vol, date, time, symbol, interval, interval, 0)
        {
        }

        public BarImpl(decimal open, decimal high, decimal low, decimal close, long vol, int date, int time, string symbol, int interval, int custint)
            : this(open, high, low, close, vol, date, time, symbol, interval, custint, 0)
        {
        }

        public BarImpl(decimal open, decimal high, decimal low, decimal close, long vol, int date, int time, string symbol, int interval, int custint, long id)
        {
            if (open < 0 || high < 0 || low < 0 || close < 0)
            {
            }
            else
            {
                Id = id;
                _units = interval;
                _ci = custint;
                _h = (ulong)(high * Const.IPREC);
                LOpen = (ulong)(open * Const.IPREC);
                _l = (ulong)(low * Const.IPREC);
                LClose = (ulong)(close * Const.IPREC);
                Volume = vol;
                Bardate = date;
                Time = time;
                _sym = symbol;
            }
        }

        public BarImpl(BarImpl b)
        {
            Id = b.Id;
            _units = b._units;
            _ci = b.CustomInterval;
            Volume = b.Volume;
            _h = b.LHigh;
            _l = b.LLow;
            LOpen = b.LOpen;
            LClose = b.LClose;
            DayEnd = b.DayEnd;
            Time = b.Time;
            Bardate = b.Bardate;
        }

        public BarImpl(BarInterval tu)
        {
            _units = (int)tu;
        }

        #endregion Public Constructors

        #region Public Properties

        public int Bardate { get; private set; }

        public DateTime BarDateTime { get { return Util.ToDateTime(Bardate, Bartime); } }
        public BarInterval BarInterval { get { return (BarInterval)_units; } }

        public int Bartime
        {
            get
            {
                var nint = (int)BarInterval.CustomTime;
                // bartime for non time intervals is same as first tick time
                if (Interval < nint)
                    return Time;
                // get num of seconds elaps
                int elap = Util.Ft2Fts(Time);
                // get remainder of dividing by interval
                int rem = nint < 0 ? elap % (CustomInterval * 1000) : elap % (Interval * 1000);
                // get datetime
                DateTime dt = Util.Qld2Dt(Bardate);
                // add rounded down result
                dt = dt.AddMilliseconds(elap - rem);
                // conver back to normal time
                int bt = Util.ToQLTime(dt);
                return bt;
            }
        }

        public decimal Close { get { return LClose * Const.IPRECV; } }

        public int CustomInterval { get { return _ci; } set { _ci = value; } }

        public bool DayEnd { get; private set; }

        public decimal High { get { return _h * Const.IPRECV; } }

        public long Id { get; set; }

        public int Interval { get { return _units; } }

        public bool IsCustom { get { return (_ci > 0) && (_units < 0); } }

        public bool IsNew { get; set; }

        public bool IsValid { get { return !string.IsNullOrWhiteSpace(_sym) && (_h >= _l) && (LOpen != 0) && (LClose != 0); } }

        public decimal Low { get { return _l * Const.IPRECV; } }

        public decimal Open { get { return LOpen * Const.IPRECV; } }

        public string Symbol { get { return _sym; } }

        public int Time { get; private set; }

        public int TradeCount { get; private set; }

        public long Volume { get; private set; }

        #endregion Public Properties

        #region Private Properties

        private decimal LClose { get; set; }

        private decimal LHigh { get { return _h; } }

        private decimal LLow { get { return _l; } }

        private decimal LOpen { get; set; }

        #endregion Private Properties

        #region Public Methods

        public static int BarsBackFromDate(BarInterval interval, int startdate)
        {
            return BarsBackFromDate(interval, startdate, Util.ToQLDate());
        }

        public static int BarsBackFromDate(BarInterval interval, int startdate, int enddate)
        {
            return BarsBackFromDate(interval, Util.ToDateTime(startdate, 0), Util.ToDateTime(enddate, Util.ToQLTime()));
        }

        public static int BarsBackFromDate(BarInterval interval, DateTime startdate, DateTime enddate)
        {
            double start2Endseconds = enddate.Subtract(startdate).TotalSeconds;
            int bars = (int)(start2Endseconds / (int)interval);
            return bars;
        }

        public static DateTime DateFromBarsBack(int barsback, BarInterval intv)
        {
            return DateFromBarsBack(barsback, intv, DateTime.Now);
        }

        public static DateTime DateFromBarsBack(int barsback, BarInterval intv, DateTime enddate)
        {
            return DateFromBarsBack(barsback, (int)intv, enddate);
        }

        public static DateTime DateFromBarsBack(int barsback, int interval)
        {
            return DateFromBarsBack(barsback, interval, DateTime.Now);
        }

        public static DateTime DateFromBarsBack(int barsback, int interval, DateTime enddate)
        {
            return enddate.Subtract(new TimeSpan(0, 0, interval * barsback));
        }

        public static Bar Deserialize(string msg)
        {
            string[] r = msg.Split(',');
            decimal open = Convert.ToDecimal(r[0], CultureInfo.InvariantCulture);
            decimal high = Convert.ToDecimal(r[1], CultureInfo.InvariantCulture);
            decimal low = Convert.ToDecimal(r[2], CultureInfo.InvariantCulture);
            decimal close = Convert.ToDecimal(r[3], CultureInfo.InvariantCulture);
            long vol = Convert.ToInt64(r[4], CultureInfo.InvariantCulture);
            int date = Convert.ToInt32(r[5], CultureInfo.InvariantCulture);
            int time = Convert.ToInt32(r[6], CultureInfo.InvariantCulture);
            string symbol = r[7];
            int interval = Convert.ToInt32(r[8], CultureInfo.InvariantCulture);
            int custint = (int)BarInterval.CustomTime;
            if (r.Length >= 10)
                custint = Convert.ToInt32(r[9], CultureInfo.InvariantCulture);
            long id = 0;
            if (r.Length >= 11)
            {
                try
                {
                    id = Convert.ToInt64(r[10], CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    id = 0;
                }
                catch (OverflowException)
                {
                    id = 0;
                }
            }
            return new BarImpl(open, high, low, close, vol, date, time, symbol, interval, custint, id);
        }

        public static string Serialize(Bar b)
        {
            const char d = ',';
            StringBuilder sb = new StringBuilder();
            sb.Append(b.Open.ToString(CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(b.High.ToString(CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(b.Low.ToString(CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(b.Close.ToString(CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(b.Volume.ToString(CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(b.Bardate.ToString(CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(b.Time.ToString(CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(b.Symbol);
            sb.Append(d);
            sb.Append(b.Interval.ToString(CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(b.CustomInterval.ToString(CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(b.Id.ToString(CultureInfo.InvariantCulture));

            return sb.ToString();
        }

        /// <summary>
        /// convert a bar into an array of ticks
        /// </summary>
        /// <param name="bar"></param>
        /// <returns></returns>
        public static Tick[] ToTick(Bar bar)
        {
            if (!bar.IsValid) return new Tick[0];
            List<Tick> list = new List<Tick>
            {
                TickImpl.NewTrade(bar.Symbol, bar.Bardate, bar.Bartime, bar.Open,
                    (int) ((double) bar.Volume/4), string.Empty),
                TickImpl.NewTrade(bar.Symbol, bar.Bardate, bar.Bartime,
                    bar.High, (int) ((double) bar.Volume/4), string.Empty),
                TickImpl.NewTrade(bar.Symbol, bar.Bardate, bar.Bartime, bar.Low,
                    (int) ((double) bar.Volume/4), string.Empty),
                TickImpl.NewTrade(bar.Symbol, bar.Bardate, bar.Bartime,
                    bar.Close, (int) ((double) bar.Volume/4), string.Empty)
            };
            return list.ToArray();
        }

        public void GotTick(Tick k)
        {
            NewTick(k);
        }

        /// <summary>
        /// Accepts the specified tick.
        /// </summary>
        /// <param name="k">The tick you want to add to the bar.</param>
        /// <returns>true if the tick is accepted, false if it belongs to another bar.</returns>
        public bool NewTick(Tick k)
        {
            TickImpl t = (TickImpl)k;
            if (_sym == "") _sym = t.Symbol;
            if (_sym != t.Symbol) throw new InvalidTick();
            if (Time == 0) { Time = Bt(t.Time); Bardate = t.Date; }
            DayEnd = Bardate != t.Date;
            // check if this bar's tick
            if ((Bt(t.Time) != Time) || (Bardate != t.Date)) return false;
            // if tick doesn't have trade or index, ignore
            if (!t.IsTrade && !t.IsIndex) return true;
            TradeCount++; // count it
            IsNew = TradeCount == 1;
            // only count volume on trades, not indicies
            if (!t.IsIndex) Volume += t.Size; // add trade size to bar volume
            if (LOpen == 0) LOpen = t.Trade;
            if (t.Trade > _h) _h = t.Trade;
            if (t.Trade < _l) _l = t.Trade;
            LClose = t.Trade;
            return true;
        }

        public override string ToString()
        {
            return "OHLC (" + Bardate + "/" + Bartime + ") " + Open.ToString("F2") + "," + High.ToString("F2") + "," + Low.ToString("F2") + "," + Close.ToString("F2");
        }

        #endregion Public Methods

        #region Private Methods

        private int Bt(int time)
        {
            // get time elapsed to this point
            int elap = Util.Ft2Fts(time);
            // get milliseconds per bar
            int secperbar = Interval * 1000;
            // get number of this bar in the day for this interval
            int bcount = (int)((double)elap / secperbar);
            return bcount;
        }

        #endregion Private Methods
    }
}