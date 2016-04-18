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
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestTick
    {
        #region Public Methods

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Basics()
        {
            TickImpl t = new TickImpl();
            Assert.True(!t.IsValid);
            t.Symbol = "IBM";
            t.Size = 100;
            t.Trade = 1;
            Assert.True(t.IsValid);
            Assert.True(t.IsTrade);
            Assert.True(!t.IsQuote);

            t = new TickImpl("TST") { TradeSize = 100 };
            Assert.True(t.TradeSize == t.Ts, t.TradeSize.ToString());

            t = new TickImpl("TST") { BidSize = 200 };
            Assert.True(t.BidSize == t.BidSize, t.BidSize.ToString());

            t = new TickImpl("TST") { AskSize = 300 };
            Assert.True(t.AskSize == t.AskSize, t.AskSize.ToString());
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Serialization()
        {
            const string t = "TST";
            const decimal p = 10m;
            const int s = 300;
            const int date = 20080702;
            const int time = 93503;
            TickImpl pre = TickImpl.NewTrade(t, p, s);
            pre.Time = time;
            pre.Date = date;
            pre.Bid = p;
            pre.Ask = p;
            pre.OfferSize = s;
            pre.BidSize = s;
            pre.Exchange = t;
            pre.BidExchange = t;
            pre.AskExchange = t;
            pre.Depth = 5;
            string serialize = TickImpl.Serialize(pre);
            Tick post = TickImpl.Deserialize(serialize);
            Assert.True(post.Time == pre.Time, post.Time.ToString());
            Assert.True(post.Date == pre.Date);
            Assert.True(post.BidSize == pre.BidSize);
            Assert.True(post.Bid == pre.Bid);
            Assert.True(post.Ask == pre.Ask);
            Assert.True(post.AskSize == pre.OfferSize);
            Assert.True(post.Exchange == pre.Exchange);
            Assert.True(post.BidExchange == pre.BidExchange);
            Assert.True(post.AskExchange == pre.AskExchange);
            Assert.True(post.Depth == pre.Depth);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void SerializationPerformance()
        {
            const int OPS = 10000;
            const string SYM = "TST";
            Tick[] attempts = Research.RandomTicks.GenerateSymbol(SYM, OPS);
            bool v = true;
            DateTime start = DateTime.Now;
            for (int i = 0; i < attempts.Length; i++)
            {
                Tick k = attempts[i];
                Tick m = TickImpl.Deserialize(TickImpl.Serialize(k));
                v &= m.Trade == k.Trade;
            }
            double time = DateTime.Now.Subtract(start).TotalSeconds;
            Assert.True(v);
            Assert.True(time <= .20);
            Console.WriteLine("Tick serialization: {0:n2} {1:n0}t/s", time, OPS / time);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void StaticFactories()
        {
            // factory inputs
            const string s = "TST";
            const decimal p = 13m;
            const int z = 400;

            // produce a new ask tick
            TickImpl t = TickImpl.NewAsk(s, p, z);
            Assert.True(t.HasAsk && !t.HasBid, t.ToString());
            Assert.True(t.Ask == p, t.Ask.ToString());
            Assert.True(t.AskSize == z, t.AskSize.ToString());
            Assert.True(t.OfferSize == z, t.OfferSize.ToString());
            Assert.True(t.Symbol == s);

            // produce bid tick
            t = TickImpl.NewBid(s, p, z);
            Assert.True(t.HasBid && !t.HasAsk, t.ToString());
            Assert.True(t.Bid == p, t.Bid.ToString());
            Assert.True(t.BidSize == z, t.BidSize.ToString());
            Assert.True(t.BidSize == z, t.BidSize.ToString());
            Assert.True(t.Symbol == s);

            // produce a trade tick
            t = TickImpl.NewTrade(s, p, z);
            Assert.True(t.IsTrade && !t.IsQuote, t.ToString());
            Assert.True(t.Trade == p, t.Trade.ToString());
            Assert.True(t.TradeSize == z, t.TradeSize.ToString());
            Assert.True(t.Ts == z, t.Ts.ToString());
            Assert.True(t.Symbol == s);
        }

        #endregion Public Methods
    }
}