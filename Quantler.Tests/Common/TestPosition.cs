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

using Quantler.Broker;
using Quantler.Interfaces;
using Quantler.Tracker;
using Quantler.Trades;
using System;
using FluentAssertions;
using Quantler.Securities;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestPosition
    {
        #region Private Fields

        private const string s = "TST";

        private DateTime dt = DateTime.Now;

        private ForexSecurity ls = new ForexSecurity("TST");

        #endregion Private Fields

        #region Public Constructors

        public TestPosition()
        {
        }

        #endregion Public Constructors

        #region Public Methods

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Basics()
        {
            PositionImpl p = new PositionImpl(ls, 0, 0, 0, new SimAccount("TST"));

            //Set security options
            ls.LotSize = 100;
            ls.PipValue = 2;
            ls.PipSize = 1;

            Assert.Equal(0, p.Size);
            Assert.True(p.Security.Name != "", "hassymbol");
            Assert.Equal(0, p.AvgPrice);
            Assert.True(p.IsFlat, "isflat");
            Assert.True(p.IsValid, "isvalid");
            PositionImpl p2 = new PositionImpl(ls, 10, 100, 0);
            PositionImpl p2copy = new PositionImpl(p2);
            Assert.Equal(p2.AvgPrice, p2copy.AvgPrice);
            Assert.Equal(p2.Size, p2copy.Size);
            Assert.Equal(p2.GrossPnL, p2copy.GrossPnL);
            Assert.Equal(p2.Security, p2copy.Security);
            p.Adjust(p2);
            Assert.True(p.Size == 100);
            Assert.True(p.Security.Name != "", "hassymbol");
            Assert.True(p.AvgPrice == 10);
            Assert.False(p.IsFlat);
            Assert.True(p.IsLong);
            Assert.True(p.IsValid);
            bool invalidexcept = false;
            PositionImpl p3 = null;
            try
            {
                p3 = new PositionImpl(ls, 0, 100, 0);
            }
            catch
            {
                invalidexcept = true;
            }
            Assert.True(invalidexcept);
            p3 = new PositionImpl(ls, 12, 100, 0);
            p.Adjust(p3);
            Assert.Equal(11, p.AvgPrice);
            Assert.True(p.IsLong);
            Assert.True(p.IsValid);
            Assert.True(!p.IsFlat);
            Assert.True(p.Size == 200);
            p.Adjust(new TradeImpl(s, 13, -100, dt) { Security = ls, Account = p.Account });
            Assert.True(p.AvgPrice == 11);
            Assert.True(p.IsLong);
            Assert.True(p.IsValid);
            Assert.True(!p.IsFlat);
            Assert.True(p.Size == 100);
            TradeImpl lasttrade = new TradeImpl(s, 12, -100, dt);
            decimal profitFromP2toLASTTRADE = Calc.ClosePL(p2, lasttrade);
            Assert.True(profitFromP2toLASTTRADE == (lasttrade.Xprice - p2.AvgPrice) * Math.Abs(lasttrade.Xsize / ls.LotSize) * ls.PipValue);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void ClosedPL()
        {
            const string sym = "RYN";
            ForexSecurity ts = new ForexSecurity(sym);
            IAccount acc = new SimAccount("TST");
            acc.Securities.AddSecurity(ts);

            PositionTracker pt = new PositionTracker(acc);
            Position p = new PositionImpl(ts, 44.39m, 800, 0, acc);
            pt.Adjust(p);
            Position p2 = new PositionImpl(ts, 44.39m, -800, 0, acc);
            pt.Adjust(p2);
            Assert.Equal(0, pt[sym].GrossPnL);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void CreateInvalidFromSymbol()
        {
            const string sym = "TST";
            ForexSecurity ts = new ForexSecurity(sym);

            bool except = false;
            Position p = null;
            try
            {
                p = new PositionImpl(ts);
            }
            catch { except = true; }

            Assert.NotNull(p);
            Assert.True(p.IsValid);
            Assert.False(except);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void FlipSideInOneTrade()
        {
            // this is illegal on the exchanges, but supported by certain
            // retail brokers so we're going to allow Quantler to support it
            // BE CAREFUL WITH THIS FEATURE.  make sure you won't be fined for doing this, before you do it.
            string s = "IBM";
            ForexSecurity ts = new ForexSecurity(s);
            ts.LotSize = 1;
            ts.PipValue = 1;
            ts.PipSize = 1;

            IAccount account = new SimAccount("TEST");
            account.Securities.AddSecurity(ts);

            // long position
            var t = new TradeImpl(s, 100m, 200);
            t.Account = account;
            t.Security = ts;
            decimal cpl = account.Positions.Adjust(t);
            Assert.Equal(cpl, 0);
            // sell more than we've got to change sides
            TradeImpl flip = new TradeImpl(s, 99, -400);
            flip.Account = account;
            flip.Security = ts;
            cpl = account.Positions.Adjust(flip);
            // make sure we captured close of trade
            Assert.Equal(-200, cpl);
            // make sure we captured new side and price
            Assert.Equal(-200, account.Positions[s].Size);
            Assert.Equal(99, account.Positions[s].AvgPrice);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void PositionAccountTest()
        {
            ForexSecurity ts = new ForexSecurity(s);
            IAccount account = new SimAccount("ME");
            account.Securities.AddSecurity(ts);

            TradeImpl t = new TradeImpl("TST", 100, 100);
            t.Account = account;
            t.Security = ts;
            TradeImpl t2 = new TradeImpl("TST", 200, 200);
            Assert.True(t.IsValid);
            Assert.True(t2.IsValid);
            t2.AccountName = "HIM";
            PositionImpl p = new PositionImpl(t);
            p.Adjust(t);
            bool failed = false;
            try
            {
                p.Adjust(t2);
            }
            catch (Exception) { failed = true; }
            Assert.True(failed);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void UsingTrades()
        {
            // long
            string s = "IBM";
            ForexSecurity ts = new ForexSecurity(s);
            ts.LotSize = 1;
            ts.PipValue = 1;
            ts.PipSize = 1;

            IAccount account = new SimAccount("TEST");
            account.Securities.AddSecurity(ts);

            TradeImpl t1 = new TradeImpl(s, 80, 100, dt);
            t1.Account = account;
            t1.Security = ts;
            PositionImpl p = new PositionImpl(t1);
            Assert.True(p.IsLong);
            Assert.True(p.Size == 100);

            TradeImpl t2 = new TradeImpl(s, 84, -100, dt);
            t2.Account = account;
            t2.Security = ts;

            decimal pl = p.Adjust(t2);
            Assert.True(p.IsFlat);
            Assert.Equal((84 - 80) * 100, pl);

            // short
            TradeImpl t3 = new TradeImpl(s, 84, -100, dt);
            t3.Account = account;
            t3.Security = ts;

            p = new PositionImpl(t3);
            Assert.True(!p.IsLong);
            Assert.True(p.Size == -100);

            TradeImpl t4 = new TradeImpl(s, 80, 100, dt);
            t4.Account = account;
            t4.Security = ts;

            pl = p.Adjust(new TradeImpl(t4));
            Assert.True(pl == (84 - 80) * 100);
            Assert.True(p.IsFlat);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void AdjustedDateTime()
        {
            // long
            string s = "IBM";
            ForexSecurity ts = new ForexSecurity(s);
            ts.LotSize = 1;
            ts.PipValue = 1;
            ts.PipSize = 1;

            IAccount account = new SimAccount("TEST");
            account.Securities.AddSecurity(ts);

            TradeImpl t1 = new TradeImpl(s, 80, 100, dt);
            t1.Account = account;
            t1.Security = ts;
            PositionImpl p = new PositionImpl(t1);
            p.LastModified.Should().BeAfter(DateTime.MinValue);
        }

        #endregion Public Methods
    }
}