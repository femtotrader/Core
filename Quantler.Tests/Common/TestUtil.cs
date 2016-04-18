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

using Quantler.Data.TikFile;
using Quantler.Interfaces;
using Quantler.Trades;
using System;
using System.Collections.Generic;
using Quantler.Securities;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestUtil
    {
        #region Private Fields

        private DateTime d;

        private int tldate = 20070731;

        private int tltime1 = 93100000;

        private int tltime2 = 140045123;

        #endregion Private Fields

        #region Public Methods

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Date()
        {
            d = Util.Qld2Dt(tldate);
            Assert.True(d.Year == 2007);
            Assert.True(d.Month == 7);
            Assert.True(d.Day == 31);
            d = Util.QLT2DT(tltime1);
            Assert.True(d.Hour == 9);
            Assert.True(d.Minute == 31);
            Assert.True(d.Second == 0);
            d = Util.ToDateTime(tldate, tltime2);
            Assert.True(d.Year == 2007);
            Assert.True(d.Month == 7);
            Assert.True(d.Day == 31);
            Assert.True(d.Hour == 14);
            Assert.True(d.Minute == 00);
            Assert.True(d.Second == 45);
            Assert.True(d.Millisecond == 123);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void DateException()
        {
            Assert.Throws<System.Exception>(() => d = Util.Qld2Dt(0));
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void FastTime()
        {
            // dt2ft
            int h = 23;
            int m = 59;
            int s = 48;
            int f = 123;
            DateTime now = new DateTime(1, 1, 1, h, m, s).AddMilliseconds(f);
            int ft = Util.DT2FT(now);
            Assert.Equal(235948123, ft);

            // f2fts

            int span = Util.Ft2Fts(ft);
            Assert.Equal(h * 60 * 60 * 1000 + m * 60 * 1000 + s * 1000 + f, span);

            // ft2dt

            DateTime next = Util.Ft2Dt(ft);
            Assert.Equal(now.Hour, next.Hour);
            Assert.Equal(now.Minute, next.Minute);
            Assert.Equal(now.Second, next.Second);
            Assert.Equal(now.Millisecond, next.Millisecond);

            // diff (subtraction => fast timespan)

            int t1 = 115100111;
            int t2 = 231408000;
            span = Util.Ftdiff(t1, t2);
            Assert.Equal(11 * 60 * 60 * 1000 + 23 * 60 * 1000 + 7 * 1000 + 889, span);

            // addition (addition of fastime and fasttimespan => fasttime)
            t1 = 133709000;
            span = 300 * 60 * 1000; // 300 minutes
            int endtime = Util.Ftadd(t1, span);
            Assert.Equal(183709000, endtime);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void QLDatematch()
        {
            int dd = 20070601;
            int m = 20071201;
            int m2 = 20060131;
            Assert.True(Util.QlDateMatch(dd, m, DateMatchType.Year));
            Assert.True(!Util.QlDateMatch(dd, m, DateMatchType.Month));
            Assert.True(!Util.QlDateMatch(dd, m, DateMatchType.None));
            Assert.True(Util.QlDateMatch(dd, m, DateMatchType.Day));
            Assert.True(Util.QlDateMatch(dd, m, DateMatchType.Day | DateMatchType.Year));
            Assert.True(!Util.QlDateMatch(dd, m, DateMatchType.Day | DateMatchType.Month));
            Assert.True(!Util.QlDateMatch(dd, m2, DateMatchType.Day | DateMatchType.Month | DateMatchType.Year));
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void TickIndex()
        {
            string[,] idx = Util.TickFileIndex(Environment.CurrentDirectory + @"\\Common" + "\\", TikConst.WildcardExt);
            string[] syma = new string[] { "ABN", "$SPX", "FTI", "TPX", "EURUSD" };
            string syms = string.Join(",", syma);
            bool foundsym = true;
            List<string> missing = new List<string>();
            for (int i = 0; i < idx.GetLength(0); i++)
            {
                ISecurity s = SecurityImpl.SecurityFromFileName(idx[i, 0]);
                var found = syms.Contains(s.Name);
                if (!found)
                    missing.Add(s.Name);
                foundsym &= found;
            }
            Assert.True(foundsym, "missing symbols: " + string.Join(" ", missing.ToArray()));
            Assert.Equal(syma.Length, idx.GetLength(0));
            Assert.Equal(2, idx.GetLength(1));
        }

        [Fact(Skip = "Reimplement")]
        [Trait("Quantler.Common", "Quantler")]
        public void TradesToClosedPLString()
        {
            List<Trade> tradelist = new List<Trade>();
            string s = "WAG";
            tradelist.Add(new TradeImpl(s, 47.04m, 300)); // enter
            tradelist.Add(new TradeImpl(s, 47.31m, 500)); // add
            tradelist.Add(new TradeImpl(s, 47.74m, -800)); // exit

            string[] closedpl = Util.TradesToClosedPL(tradelist);
            for (int i = 0; i < closedpl.Length; i++)
            {
                string plline = closedpl[i];
                string[] plrec = plline.Split(',');

                // check record length matches expected
                int numfields = Enum.GetNames(typeof(TradePlField)).Length;
                Assert.True(numfields == plrec.Length);

                // validate the values
                switch (i)
                {
                    case 0:
                        Assert.True(plrec[(int)TradePlField.OpenPl] == "0.00", plrec[(int)TradePlField.OpenPl]);
                        Assert.True(plrec[(int)TradePlField.ClosedPl] == "0.00", plrec[(int)TradePlField.ClosedPl]);
                        Assert.True(plrec[(int)TradePlField.OpenSize] == "300", plrec[(int)TradePlField.OpenSize]);
                        Assert.True(plrec[(int)TradePlField.ClosedSize] == "0", plrec[(int)TradePlField.ClosedSize]);
                        Assert.True(plrec[(int)TradePlField.AvgPrice] == "47.04", plrec[(int)TradePlField.AvgPrice]);
                        break;

                    case 1:
                        Assert.True(plrec[(int)TradePlField.OpenPl] == "81.00", plrec[(int)TradePlField.OpenPl]);
                        Assert.True(plrec[(int)TradePlField.ClosedPl] == "0.00", plrec[(int)TradePlField.ClosedPl]);
                        Assert.True(plrec[(int)TradePlField.OpenSize] == "800", plrec[(int)TradePlField.OpenSize]);
                        Assert.True(plrec[(int)TradePlField.ClosedSize] == "0", plrec[(int)TradePlField.ClosedSize]);
                        Assert.True(plrec[(int)TradePlField.AvgPrice] == "47.20875", plrec[(int)TradePlField.AvgPrice]); //TODO: this test is false if it is based on stocks, the average price of stock should be rounded to 2 decimals (with FX it is 5)
                        break;

                    case 2:
                        Assert.True(plrec[(int)TradePlField.OpenPl] == "0.00", plrec[(int)TradePlField.OpenPl]);
                        Assert.True(plrec[(int)TradePlField.ClosedPl] == "425.00", plrec[(int)TradePlField.ClosedPl]);
                        Assert.True(plrec[(int)TradePlField.OpenSize] == "0", plrec[(int)TradePlField.OpenSize]);
                        Assert.True(plrec[(int)TradePlField.ClosedSize] == "-800", plrec[(int)TradePlField.ClosedSize]);
                        Assert.True(plrec[(int)TradePlField.AvgPrice] == "0.00", plrec[(int)TradePlField.AvgPrice]);
                        break;
                }
            }
        }

        #endregion Public Methods
    }
}