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
using Quantler.Securities;
using Quantler.Tracker;
using Quantler.Trades;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestPositionTracker
    {
        #region Public Methods

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Adjust()
        {
            string s = "IBM";
            ForexSecurity ts = new ForexSecurity(s);
            IAccount account = new SimAccount("TEST");
            account.Securities.AddSecurity(ts);

            TradeImpl t1 = new TradeImpl(s, 100, 100);
            t1.Account = account;
            t1.Security = ts;

            PositionTracker pt = new PositionTracker(account);

            // make we have no position yet
            Assert.True(pt[t1.Symbol].IsFlat);

            // send some adjustments
            decimal cpl = 0;
            cpl += pt.Adjust(t1);
            cpl += pt.Adjust(t1);

            // verify that adjustments took hold
            Assert.Equal(0, cpl);
            Assert.Equal(200, pt[t1.Symbol].Size);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void BlankPositionReq()
        {
            string sym = "IBM";
            ForexSecurity ts = new ForexSecurity(sym);
            IAccount account = new SimAccount("TEST");
            account.Securities.AddSecurity(ts);

            PositionTracker pt = new PositionTracker(account);
            bool except = false;
            int s = 100;
            try
            {
                s = pt[sym].Size;
            }
            catch { except = true; }
            Assert.Equal(0, s);
            Assert.False(except);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void InitAndAdjust()
        {
            string sym = "IBM";
            ForexSecurity ts = new ForexSecurity(sym);
            IAccount account = new SimAccount("TEST");
            account.Securities.AddSecurity(ts);

            // startup position tracker
            PositionTracker pt = new PositionTracker(account);
            PositionTracker pt2 = new PositionTracker(account);
            // give pt our initial position
            PositionImpl init = new PositionImpl(ts, 0, 0, 0, account);

            pt.Adjust(init);
            pt2.Adjust(init);
            // fill a trade in both places
            TradeImpl fill = new TradeImpl(ts.Name, 100, 100);
            fill.Account = account;
            fill.Security = ts;

            pt.Adjust(fill);
            pt2.Adjust(fill);
            // make sure it's only 100 in both places
            Assert.Equal(100, pt[sym].Size);
            Assert.Equal(100, pt2[sym].Size);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void MultipleAccount()
        {
            // setup defaults for 1st and 2nd accounts and positions
            string s = "TST";
            ForexSecurity sym = new ForexSecurity(s);
            IAccount a1 = new SimAccount("account1");
            a1.Securities.AddSecurity(sym);

            IAccount a2 = new SimAccount("account2");
            a1.Securities.AddSecurity(sym);

            int s1 = 300;
            int s2 = 500;
            decimal p = 100m;

            // create position tracker
            PositionTracker pt = new PositionTracker(a1);

            // set initial position in 1st account
            pt.Adjust(new PositionImpl(sym, p, s1, 0, a1));

            // set initial position in 2nd account
            pt.Adjust(new PositionImpl(sym, p, s2, 0, a2));

            // verify I can query default account and it's correct
            Assert.Equal(s1, pt[sym].Size);
            // change default to 2nd account
            pt.DefaultAccount = a2;
            // verify I can query default and it's correct
            Assert.Equal(s2, pt[sym].Size);
            // verify I can query 1st account and correct
            Assert.Equal(s1, pt[sym, a1].Size);
            // verify I can query 2nd account and correct
            Assert.Equal(s2, pt[sym, a2].Size);
            // get fill in sym for 1st account
            TradeImpl f = new TradeImpl(sym.Name, p, s1);
            f.Account = a1;
            f.Security = sym;
            pt.Adjust(f);
            // get fill in sym for 2nd account
            TradeImpl f2 = new TradeImpl(sym.Name, p, s2);
            f2.Account = a2;
            f2.Security = sym;
            pt.Adjust(f2);
            // verify that I can querry 1st account and correct
            Assert.Equal(s1 * 2, pt[sym, a1].Size);
            // verify I can query 2nd account and correct
            Assert.Equal(s2 * 2, pt[sym, a2].Size);
            // reset
            pt.Clear();
            // ensure I can query first and second account and get flat symbols
            Assert.Equal(0, pt[sym].Size);
            Assert.Equal(0, pt[sym, a1].Size);
            Assert.Equal(0, pt[sym, a2].Size);
            Assert.True(pt[sym, a1].IsFlat);
            Assert.True(pt[sym, a2].IsFlat);
            Assert.True(pt[sym].IsFlat);
            Assert.Equal(null, pt.DefaultAccount);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void NewPosition()
        {
            string s = "IBM";
            ForexSecurity ts = new ForexSecurity(s);
            IAccount account = new SimAccount("TEST");
            account.Securities.AddSecurity(ts);
            PositionImpl p = new PositionImpl(ts, 80, 500, 0, account);

            PositionTracker pt = new PositionTracker(account);
            Assert.True(pt[s].IsFlat);
            pt.NewPosition(p);
            Assert.Equal(500, pt[s].Size);
        }

        #endregion Public Methods
    }
}