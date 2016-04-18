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
using Quantler.Data.Ticks;
using Quantler.Interfaces;
using Quantler.Trades;
using System.Collections.Generic;
using Quantler.Securities;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestAccount
    {
        #region Private Fields

        private const decimal inc = .1m;
        private const decimal p = 1;
        private const int s = 1000;
        private int initialbalance = 10000;
        private int leverage = 100;
        private string sym = "EURUSD";

        #endregion Private Fields

        #region Public Methods

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void AccountBalanceUpdate()
        {
            //Arrange
            SimAccount testaccount = new SimAccount("SIM1", "Menno's Account", initialbalance, leverage, "SIM");
            ForexSecurity ts = new ForexSecurity(sym);
            ts.LotSize = 1000;
            ts.PipSize = 0.0001M;
            ts.TickSize = ts.PipSize / 10;
            testaccount.Securities.AddSecurity(ts);
            TickImpl tick = TickImpl.NewQuote(sym, p, p, int.MaxValue, int.MaxValue, ts.DestEx, ts.DestEx);
            TickImpl secondtick = TickImpl.NewQuote(sym, p + inc, p + inc, int.MaxValue, int.MaxValue, ts.DestEx, ts.DestEx);

            List<Trade> fills = new List<Trade>(new Trade[] {
                // go long
                new TradeImpl(sym,p,s) { Security = ts, Account = testaccount, Commission = 1},             // 100 @ $100
                // increase bet
                new TradeImpl(sym,p+inc,s*2) { Security = ts, Account = testaccount, Commission = 2},       // 300 @ $100.066666
                // take some profits
                new TradeImpl(sym,p+inc*2,s*-1) { Security = ts, Account = testaccount, Commission = 1},    // 200 @ 100.0666 (profit = 100 * (100.20 - 100.0666) = 13.34) / maxMIU(= 300*100.06666) = .04% ret
                // go flat (round turn)
                new TradeImpl(sym,p+inc*2,s*-2) { Security = ts, Account = testaccount, Commission = 2},    // 0 @ 0
                // go short
                new TradeImpl(sym,p,s*-2) { Security = ts, Account = testaccount, Commission = 2},          // -200 @ 100
                // decrease bet
                new TradeImpl(sym,p,s) { Security = ts, Account = testaccount, Commission = 1},             // -100 @100
                // exit (round turn)
                new TradeImpl(sym,p+inc,s) { Security = ts, Account = testaccount, Commission = 1},         // 0 @ 0 (gross profit = -0.10*100 = -$10)
                // do another entry
                new TradeImpl(sym,p,s) { Security = ts, Account = testaccount, Commission = 1}              // 100 @ 100
            });

            //Act, fill all trades
            testaccount.OnTick(tick);
            testaccount.OnTick(secondtick);
            foreach (var t in fills)
                testaccount.OnFill(t);

            //Assert
            Assert.True(testaccount.Balance == 10300);
            Assert.True(testaccount.Margin == 11);
            Assert.True(testaccount.MarginLevel == (testaccount.Equity / testaccount.Margin) * 100);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Basics()
        {
            SimAccount a = new SimAccount();
            Assert.True(a.IsValid);
            Assert.True(a.Id == "empty");
            const string myid = "quantler";
            a = new SimAccount(myid);
            Assert.True(a.IsValid);
            Assert.True(a.Id == myid);
            a = new SimAccount("SIM1", "Menno's Account", initialbalance, leverage, "Menno");
            Assert.True(a.Balance == initialbalance);
            Assert.True(a.Leverage == leverage);
            Assert.True(a.Margin == 0);
            Assert.True(a.FreeMargin == (a.Equity - a.Margin));
            Assert.True(a.Equity == a.Balance);
            Assert.True(a.Currency == CurrencyType.USD);
            Assert.True(a.FloatingPnL == 0);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void LowMarginLevel()
        {
            //Arrange
            SimAccount testaccount = new SimAccount("SIM1", "Menno's Account", initialbalance, leverage, "SIM");
            ForexSecurity ts = new ForexSecurity(sym);
            ts.LotSize = 1000;
            ts.PipSize = 0.0001M;
            ts.TickSize = ts.PipSize / 10;
            testaccount.Securities.AddSecurity(ts);
            TickImpl tick = TickImpl.NewQuote(sym, p, p, int.MaxValue, int.MaxValue, ts.DestEx, ts.DestEx);
            TickImpl secondtick = TickImpl.NewQuote(sym, p + inc, p + inc, int.MaxValue, int.MaxValue, ts.DestEx, ts.DestEx);

            List<Trade> fills = new List<Trade>(new Trade[] {
                // go long
                new TradeImpl(sym,p,s) { Security = ts, Account = testaccount, Commission = 1},             // 100 @ $100
                // increase bet
                new TradeImpl(sym,p+inc,s*2) { Security = ts, Account = testaccount, Commission = 2},       // 300 @ $100.066666
                // take some profits
                new TradeImpl(sym,p+inc*2,s*-1) { Security = ts, Account = testaccount, Commission = 1},    // 200 @ 100.0666 (profit = 100 * (100.20 - 100.0666) = 13.34) / maxMIU(= 300*100.06666) = .04% ret
                // go flat (round turn)
                new TradeImpl(sym,p+inc*2,s*-2) { Security = ts, Account = testaccount, Commission = 2},    // 0 @ 0
                // go short
                new TradeImpl(sym,p,s*-2) { Security = ts, Account = testaccount, Commission = 2},          // -200 @ 100
                // decrease bet
                new TradeImpl(sym,p,s) { Security = ts, Account = testaccount, Commission = 1},             // -100 @100
                // exit (round turn)
                new TradeImpl(sym,p+inc,s*5000) { Security = ts, Account = testaccount, Commission = 1},         // 0 @ 0 (gross profit = -0.10*100 = -$10)
                // do another entry
                new TradeImpl(sym,p,s) { Security = ts, Account = testaccount, Commission = 1}              // 100 @ 100
            });

            //Act, fill all trades
            testaccount.OnTick(tick);
            testaccount.OnTick(secondtick);
            foreach (var t in fills)
                testaccount.OnFill(t);

            //Assert
            Assert.True(testaccount.Balance == 10300);
            Assert.True(testaccount.Margin == 55000);
            //Lower than 20% (Margin Call)
            Assert.True(testaccount.MarginLevel < 20M);
        }

        #endregion Public Methods
    }
}