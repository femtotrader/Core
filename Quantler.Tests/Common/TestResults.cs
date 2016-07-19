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
using Quantler.Trades;
using System;
using System.Collections.Generic;
using System.Linq;
using Quantler.Securities;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestResults
    {
        private Result rt = new Results();
        private const string sym = "TST";
        private const decimal p = 100;
        private const int s = 100;
        private const decimal inc = .1m;

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void KeyRatios()
        {
            EquitySecurity ts = new EquitySecurity(sym);
            ts.LotSize = 1;
            ts.PipValue = 1;
            ts.PipSize = 1;
            IAccount account = new SimAccount("TEST");
            account.Securities.AddSecurity(ts);
            rt = new Results(.01m, account);

            // get some trades
            List<Trade> fills = new List<Trade>(new Trade[] {
                // go long
                new TradeImpl(sym,p,s) { Security = ts, Account = account, Commission = 1},             // 100 @ $100
                // increase bet
                new TradeImpl(sym,p+inc,s*2) { Security = ts, Account = account, Commission = 2},       // 300 @ $100.066666
                // take some profits
                new TradeImpl(sym,p+inc*2,s*-1) { Security = ts, Account = account, Commission = 1},    // 200 @ 100.0666 (profit = 100 * (100.20 - 100.0666) = 13.34) / maxMIU(= 300*100.06666) = .04% ret
                // go flat (round turn)
                new TradeImpl(sym,p+inc*2,s*-2) { Security = ts, Account = account, Commission = 2},    // 0 @ 0
                // go short
                new TradeImpl(sym,p,s*-2) { Security = ts, Account = account, Commission = 2},          // -200 @ 100
                // decrease bet
                new TradeImpl(sym,p,s) { Security = ts, Account = account, Commission = 1},             // -100 @100
                // exit (round turn)
                new TradeImpl(sym,p+inc,s) { Security = ts, Account = account, Commission = 1},         // 0 @ 0 (gross profit = -0.10*100 = -$10)
                // do another entry
                new TradeImpl(sym,p,s) { Security = ts, Account = account, Commission = 1}              // 100 @ 100
            });

            //fill all trades
            foreach (var t in fills)
                account.Positions.Adjust(t);

            // check ratios
#if DEBUG
            Console.WriteLine(rt.ToString());
#endif

            Assert.Equal(0.0238M, Math.Round(rt.PortfolioPctReturns.Average() * 100, 4));
            Assert.Equal(0.1028M, Math.Round(Calc.StdDev(rt.PortfolioPctReturns) * 100, 4));
            Assert.Equal(-9.496M, rt.SharpeRatio);
            Assert.Equal(-27.164M, rt.SortinoRatio);
            Assert.Equal(2.060M, rt.ProfitFactor);
            Assert.Equal(1100, rt.SharesTraded);
            Assert.Equal(11, Math.Round(rt.Commissions));
            Assert.Equal(19, Math.Round(rt.NetPL));
            Assert.Equal(10000, rt.InitialCapital);
            Assert.Equal(10019, Math.Round(rt.Balance));
            Assert.Equal(0.0019M, Math.Round(rt.ROI, 4));
            Assert.Equal(-0.0015M, Math.Round(rt.MaxDDPortfolio, 4));
        }

        [Fact(Skip = "Reimplement")]
        [Trait("Quantler.Common", "Quantler")]
        public void RoundTurnStat()
        {
            ForexSecurity ts = new ForexSecurity(sym);
            ts.LotSize = 1;
            ts.PipSize = 1;
            IAccount account = new SimAccount("TEST");
            account.Securities.AddSecurity(ts);
            rt = new Results(.01m, account);

            // get some trades
            List<Trade> fills = new List<Trade>(new Trade[] {
                // go long
                new TradeImpl(sym,p,s) { Security = ts, Account = account},
                // increase bet
                new TradeImpl(sym,p+inc,s*2) { Security = ts, Account = account},
                // take some profits
                new TradeImpl(sym,p+inc*2,s*-1) { Security = ts, Account = account},
                // go flat (round turn)
                new TradeImpl(sym,p+inc*2,s*-2) { Security = ts, Account = account},
                // go short
                new TradeImpl(sym,p,s*-2) { Security = ts, Account = account},
                // decrease bet
                new TradeImpl(sym,p,s) { Security = ts, Account = account},
                // exit (round turn)
                new TradeImpl(sym,p+inc,s) { Security = ts, Account = account},
                // do another entry
                new TradeImpl(sym,p,s) { Security = ts, Account = account}
            });

            //fill all trades
            foreach (var t in fills)
                account.Positions.Adjust(t);

            // check trade count
            Assert.Equal(fills.Count, rt.Trades);

            // check round turn count
            Assert.Equal(2, rt.RoundTurns);

            // verify trade winners
            Assert.Equal(2, rt.Winners);

            // verify round turn winners
            Assert.Equal(1, rt.RoundWinners);

            // verify round turn losers
            Assert.Equal(1, rt.RoundLosers);
        }
    }
}