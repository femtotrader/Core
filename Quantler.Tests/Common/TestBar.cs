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
using Quantler.Data.Ticks;
using Quantler.Interfaces;
using System;
using System.Linq;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestBar
    {
        #region Private Fields

        private const int D = 20070517;
        private const string Sym = "TST";
        private const int T = 93500000;
        private const string X = "NYSE";

        private readonly TickImpl[] _ticklist = {
                TickImpl.NewTrade(Sym,D,T,10,100,X),
                TickImpl.NewTrade(Sym,D,T+100000,10,100,X),
                TickImpl.NewTrade(Sym,D,T+200000,10,100,X),
                TickImpl.NewTrade(Sym,D,T+300000,10,100,X),
                TickImpl.NewTrade(Sym,D,T+400000,15,100,X),
                TickImpl.NewTrade(Sym,D,T+500000,16,100,X),
                TickImpl.NewTrade(Sym,D,T+600000,16,100,X),
                TickImpl.NewTrade(Sym,D,T+700000,10,100,X),
                TickImpl.NewTrade(Sym,D,T+710000,10,100,X),
            };

        #endregion Private Fields

        #region Public Methods

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void BarIntervals()
        {
            BarImpl b = new BarImpl(BarInterval.FiveMin);
            int accepts = _ticklist.Count(k => b.NewTick(k));
            Assert.Equal(5, accepts);

            b = new BarImpl(BarInterval.FifteenMin);
            accepts = _ticklist.Count(k => b.NewTick(k));
            Assert.Equal(9, accepts);

            b = new BarImpl(BarInterval.Minute);
            accepts = 0;
            for (int i = 7; i < _ticklist.Length; i++)
                if (b.NewTick(_ticklist[i])) accepts++;
            Assert.Equal(2, accepts);
        }

        // TestBarImpl
        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void BarsBack()
        {
            DateTime present = DateTime.Parse("2010/1/3 15:00:00");
            DateTime past = BarImpl.DateFromBarsBack(5, BarInterval.FiveMin, present);
            Assert.Equal(20100103, Util.ToQLDate(past));
            Assert.Equal(143500000, Util.ToQLTime(past));
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void BarTime()
        {
            Bar b = new BarImpl(1, 1, 1, 1, 1, 20100302, 93533000, "IBM", (int)BarInterval.FiveMin);
            Assert.Equal(93500000, b.Bartime);
            Assert.Equal(93533000, b.Time);
            Console.WriteLine(b.Bartime + " " + b.Time);

            b = new BarImpl(1, 1, 1, 1, 1, 20100302, 93533000, "IBM", (int)BarInterval.Hour);
            Assert.Equal(90000000, b.Bartime);
            Assert.Equal(93533000, b.Time);
            Console.WriteLine(b.Bartime + " " + b.Time);

            b = new BarImpl(1, 1, 1, 1, 1, 20100302, 95504000, "IBM", (int)BarInterval.FiveMin);
            Assert.Equal(95500000, b.Bartime);
            Assert.Equal(95504000, b.Time);
            Console.WriteLine(b.Bartime + " " + b.Time);

            //DateTime check
            Assert.Equal(int.Parse(b.BarDateTime.ToString("hhmmssfff")), 95500000);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Construction()
        {
            BarImpl b = new BarImpl();
            Assert.True(!b.IsValid);
            Assert.True(!b.IsNew);
            b.NewTick(_ticklist[0]);
            Assert.True(b.IsValid);
            Assert.True(b.IsNew);
            b.NewTick(_ticklist[1]);
            Assert.True(b.IsValid);
            Assert.True(!b.IsNew);
            Assert.True(b.Volume == 200);
            b.NewTick(TickImpl.NewQuote(Sym, D, T, 10m, 11m, 1, 1, X, X));
            Assert.True(b.TradeCount == 2);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void SerializeDeseralize()
        {
            Bar b = new BarImpl(1, 1, 1, 1, 1, 20100302, 93533, "IBM", (int)BarInterval.FiveMin);
            string msg = BarImpl.Serialize(b);
            Bar cb = BarImpl.Deserialize(msg);

            Assert.Equal(b.Symbol, cb.Symbol);
            Assert.Equal(b.Time, cb.Time);
            Assert.Equal(b.Interval, cb.Interval);
            Assert.Equal(b.High, cb.High);
            Assert.Equal(b.Low, cb.Low);
            Assert.Equal(b.Open, cb.Open);
            Assert.Equal(b.Close, cb.Close);
            Assert.Equal(b.Volume, cb.Volume);
            Assert.Equal(b.Bardate, cb.Bardate);
        }

        #endregion Public Methods
    }
}