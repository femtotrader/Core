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
using Quantler.Orders;
using System;
using System.Collections.Generic;
using Quantler.Securities;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestBroker
    {
        #region Private Fields

        private const string S = "TST";
        private readonly SimBroker _broker = new SimBroker();
        private readonly Research.ZeroTransactionCosts _trans = new Research.ZeroTransactionCosts();
        private int _fills;
        private int _orders;

        #endregion Private Fields

        #region Public Methods

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Basics()
        {
            ForexSecurity tsec = new ForexSecurity(S);
            tsec.LotSize = 1;
            tsec.PipValue = 1;
            tsec.PipSize = 1;
            tsec.OrderStepSize = 1;

            SimAccount account = new SimAccount("TEST", "testing", 1000M, 100, "SIM");
            account.Securities.AddSecurity(tsec);

            SimBroker broker = new SimBroker(account, _trans);
            broker.BrokerModel = _trans;
            broker.GotFill += broker_GotFill;
            broker.GotOrder += broker_GotOrder;
            OrderImpl o = new OrderImpl();
            PendingOrderImpl po = new PendingOrderImpl(o);
            int error = broker.SendOrderStatus(po);
            Assert.NotEqual((int)StatusType.OK, error);
            Assert.True(_orders == 0);
            Assert.True(_fills == 0);

            o = new OrderImpl(tsec, Direction.Long, 100);
            po = new PendingOrderImpl(o);

            broker.SendOrderStatus(po);
            Assert.True(_orders == 1);
            Assert.True(_fills == 0);
            Assert.True(broker.Execute(TickImpl.NewTrade(S, 10, 200)) == 1);
            Assert.True(_fills == 1);

            // test that a limit order is not filled outside the market
            o = new OrderImpl(tsec, Direction.Long, 100, 9);
            po = new PendingOrderImpl(o);

            broker.SendOrderStatus(po);
            Assert.Equal(0, broker.Execute(TickImpl.NewTrade(S, 10, 100)));
            Assert.True(_fills == 1); // redundant but for counting

            // test that limit order is filled inside the market
            Assert.Equal(1, broker.Execute(TickImpl.NewTrade(S, 8, 100)));
            Assert.True(_fills == 2);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void BBO()
        {
            SimBroker broker = new SimBroker();
            broker.BrokerModel = _trans;
            ForexSecurity tsec = new ForexSecurity(S);
            tsec.LotSize = 1;
            tsec.OrderStepSize = 1;
            const decimal p1 = 10m;
            const decimal p2 = 11m;
            const int x = 100;
            Order bid, offer;

            // send bid, make sure it's BBO (since it's only order on any book)
            broker.SendOrderStatus(new PendingOrderImpl(new OrderImpl(tsec, Direction.Long, x, p1)));
            bid = broker.BestBid(S);
            offer = broker.BestOffer(S);
            Assert.True(bid.IsValid && (bid.LimitPrice == p1) && (bid.Quantity == x), bid.ToString());
            Assert.True(!offer.IsValid, offer.ToString());

            // add better bid, make sure it's BBO
            OrderImpl o;
            // Order#1... 100 shares buy at $11
            o = new OrderImpl(tsec, Direction.Long, x, p2);
            PendingOrderImpl po = new PendingOrderImpl(o);
            broker.SendOrderStatus(po);
            bid = broker.BestBid(S);
            offer = broker.BestOffer(S);
            Assert.True(bid.IsValid);
            Assert.Equal(p2, bid.LimitPrice);
            Assert.Equal(x, bid.Size);
            Assert.True(!offer.IsValid, offer.ToString());

            // add another bid at same price on another account, make sure it's additive
            //order #2... 100 shares buy at $11
            o = new OrderImpl(tsec, Direction.Long, x, p2);
            po = new PendingOrderImpl(o, new SimAccount("ANOTHER_ACCOUNT"));
            o.AccountName = "ANOTHER_ACCOUNT";
            broker.SendOrderStatus(po);
            bid = broker.BestBid(S);
            offer = broker.BestOffer(S);
            Assert.True(bid.IsValid);
            Assert.Equal(p2, bid.LimitPrice);
            Assert.Equal(x * 2, bid.Size);
            Assert.True(!offer.IsValid, offer.ToString());

            // cancel order and make sure bbo returns
            po.Cancel();
            bid = broker.BestBid(S);
            offer = broker.BestOffer(S);
            Assert.True(bid.IsValid);
            Assert.Equal(p2, bid.LimitPrice);
            Assert.Equal(x, bid.Size);
            Assert.True(!offer.IsValid, offer.ToString());

            // other test ideas
            // replicate above tests for sell-side
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void DayFill()
        {
            SimBroker broker = new SimBroker();
            broker.BrokerModel = _trans;
            const string s = "TST";
            ForexSecurity tsec = new ForexSecurity(s);
            tsec.LotSize = 1;
            tsec.OrderStepSize = 1;

            OrderImpl day = new OrderImpl(tsec, Direction.Long, 200);
            PendingOrderImpl pday = new PendingOrderImpl(day);
            broker.SendOrderStatus(pday);

            TickImpl openingTick = TickImpl.NewTrade(s, Util.ToQLDate(DateTime.Now), Util.QL2FT(9, 30, 00, 000), 9, 10000, "NYS");
            TickImpl endMornTick = TickImpl.NewTrade(s, Util.ToQLDate(DateTime.Now), Util.QL2FT(12, 00, 00, 000), 9, 10000, "NYS");
            TickImpl endLunchTick = TickImpl.NewTrade(s, Util.ToQLDate(DateTime.Now), Util.QL2FT(14, 15, 00, 000), 9, 10000, "NYS");
            TickImpl closingTick = TickImpl.NewTrade(s, Util.ToQLDate(DateTime.Now), Util.QL2FT(16, 00, 00, 000), 9, 10000, "NYS");

            int c;
            c = broker.Execute(openingTick); Assert.Equal(1, c); // should execute on first received tick
            c = broker.Execute(endMornTick); Assert.Equal(0, c);
            c = broker.Execute(endLunchTick); Assert.Equal(0, c);
            c = broker.Execute(closingTick); Assert.Equal(0, c);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Fill_HighLiquidity()
        {
            SimBroker broker = new SimBroker();
            broker.BrokerModel = _trans;
            broker.UseHighLiquidityFillsEod = true;
            const string s = "SPY";
            ForexSecurity tsec = new ForexSecurity(s);
            tsec.LotSize = 1;
            tsec.OrderStepSize = 1;

            // OHLC for 6/21/2012 on SPY
            TickImpl openingTick = TickImpl.NewTrade(s, Util.ToQLDate(DateTime.Now), Util.QL2FT(9, 30, 00, 000), 135.67m, 10670270, "NYS");
            TickImpl endMornTick = TickImpl.NewTrade(s, Util.ToQLDate(DateTime.Now), Util.QL2FT(12, 00, 00, 000), 135.78m, 10670270, "NYS");
            TickImpl endLunchTick = TickImpl.NewTrade(s, Util.ToQLDate(DateTime.Now), Util.QL2FT(14, 15, 00, 000), 132.33m, 10670270, "NYS");
            TickImpl closingTick = TickImpl.NewTrade(s, Util.ToQLDate(DateTime.Now), Util.QL2FT(16, 00, 00, 000), 132.44m, 10670270, "NYS");

            tsec.LastTickEvent = openingTick.TickDateTime;

            OrderImpl limitBuy = new OrderImpl(tsec, Direction.Long, 1, 133m);
            OrderImpl limitSell = new OrderImpl(tsec, Direction.Short, 2, 133.5m);
            OrderImpl stopBuy = new OrderImpl(tsec, Direction.Long, 3, 0, 135.70m);
            OrderImpl stopSell = new OrderImpl(tsec, Direction.Short, 4, 0, 135.75m);

            PendingOrderImpl plimitBuy = new PendingOrderImpl(limitBuy);
            PendingOrderImpl plimitSell = new PendingOrderImpl(limitSell);
            PendingOrderImpl pstopBuy = new PendingOrderImpl(stopBuy);
            PendingOrderImpl pstopSell = new PendingOrderImpl(stopSell);

            broker.SendOrderStatus(plimitBuy);
            broker.SendOrderStatus(plimitSell);
            broker.SendOrderStatus(pstopBuy);
            broker.SendOrderStatus(pstopSell);

            broker.Execute(openingTick);
            broker.Execute(endMornTick);
            broker.Execute(endLunchTick);
            broker.Execute(closingTick);

            List<Trade> trades = broker.GetTradeList();
            Assert.True(trades.Count == 4);

            foreach (Trade trade in trades)
            {
                if (trade.Xsize == 1)
                    Assert.Equal(133m, trade.Xprice);
                else if (trade.Xsize == 2)
                    Assert.Equal(133.5m, trade.Xprice);
                else if (trade.Xsize == 3)
                    Assert.Equal(135.7m, trade.Xprice);
                else if (trade.Xsize == 4)
                    Assert.Equal(135.75m, trade.Xprice);
            }
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Fill_RegularLiquidity()
        {
            SimBroker broker = new SimBroker();
            broker.BrokerModel = _trans;
            const string s = "SPY";
            ForexSecurity tsec = new ForexSecurity(s);
            tsec.LotSize = 1;
            tsec.OrderStepSize = 1;

            OrderImpl limitBuy = new OrderImpl(tsec, Direction.Long, 1, 133m);
            OrderImpl limitSell = new OrderImpl(tsec, Direction.Short, 1, 133.5m);
            OrderImpl stopBuy = new OrderImpl(tsec, Direction.Long, 3, 0, 135.70m);
            OrderImpl stopSell = new OrderImpl(tsec, Direction.Short, 4, 0, 135.75m);

            PendingOrderImpl plimitBuy = new PendingOrderImpl(limitBuy);
            PendingOrderImpl plimitSell = new PendingOrderImpl(limitSell);
            PendingOrderImpl pstopBuy = new PendingOrderImpl(stopBuy);
            PendingOrderImpl pstopSell = new PendingOrderImpl(stopSell);

            broker.SendOrderStatus(plimitBuy);
            broker.SendOrderStatus(plimitSell);
            broker.SendOrderStatus(pstopBuy);
            broker.SendOrderStatus(pstopSell);

            // OHLC for 6/21/2012 on SPY
            TickImpl openingTick = TickImpl.NewTrade(s, Util.ToQLDate(DateTime.Now), Util.QL2FT(9, 30, 00, 000), 135.67m, 10670270, "NYS");
            TickImpl endMornTick = TickImpl.NewTrade(s, Util.ToQLDate(DateTime.Now), Util.QL2FT(12, 00, 00, 000), 135.78m, 10670270, "NYS");
            TickImpl endLunchTick = TickImpl.NewTrade(s, Util.ToQLDate(DateTime.Now), Util.QL2FT(14, 15, 00, 000), 132.33m, 10670270, "NYS");
            TickImpl closingTick = TickImpl.NewTrade(s, Util.ToQLDate(DateTime.Now), Util.QL2FT(16, 00, 00, 000), 132.44m, 10670270, "NYS");

            broker.Execute(openingTick);
            broker.Execute(endMornTick);
            broker.Execute(endLunchTick);
            broker.Execute(closingTick);

            List<Trade> trades = broker.GetTradeList();
            Assert.True(trades.Count == 4);

            foreach (Trade trade in trades)
            {
                if (trade.Xsize == 1)
                    Assert.Equal(132.33m, trade.Xprice);
                else if (trade.Xsize == 2)
                    Assert.Equal(132.33m, trade.Xprice);
                else if (trade.Xsize == 3)
                    Assert.Equal(135.78m, trade.Xprice);
                else if (trade.Xsize == 4)
                    Assert.Equal(135.78m, trade.Xprice);
            }
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void MOCs()
        {
            SimBroker broker = new SimBroker();
            broker.BrokerModel = _trans;
            const string s = "TST";
            ForexSecurity tsec = new ForexSecurity(s);
            tsec.LotSize = 1;
            tsec.OrderStepSize = 1;

            OrderImpl moc = new OrderImpl(tsec, Direction.Long, 200);
            moc.ValidInstruct = OrderInstructionType.MOC;
            PendingOrderImpl pmoc = new PendingOrderImpl(moc);
            Assert.True(moc.ValidInstruct == OrderInstructionType.MOC, "unexpected order instruction: " + moc.ValidInstruct);
            Assert.Equal(0, broker.SendOrderStatus(pmoc));

            TickImpl openingTick = TickImpl.NewTrade(s, Util.ToQLDate(DateTime.Now), Util.QL2FT(9, 30, 00, 000), 9, 10000, "NYS");
            TickImpl endMornTick = TickImpl.NewTrade(s, Util.ToQLDate(DateTime.Now), Util.QL2FT(12, 00, 00, 000), 9, 10000, "NYS");
            TickImpl endLunchTick = TickImpl.NewTrade(s, Util.ToQLDate(DateTime.Now), Util.QL2FT(14, 15, 00, 000), 9, 10000, "NYS");
            TickImpl closingTick = TickImpl.NewTrade(s, Util.ToQLDate(DateTime.Now), Util.QL2FT(16, 00, 00, 000), 9, 10000, "NYS");

            int c = 0;
            c = broker.Execute(openingTick); Assert.Equal(0, c);
            c = broker.Execute(endMornTick); Assert.Equal(0, c);
            c = broker.Execute(endLunchTick); Assert.Equal(0, c);
            c = broker.Execute(closingTick); Assert.Equal(1, c); // should execute on the first tick at/after 16:00:00
        }

        [Fact(Skip = "Todo, fix this")]
        [Trait("Quantler.Common", "Quantler")]
        public void MultiAccount()
        {
            _broker.BrokerModel = _trans;
            const string sym = "TST";
            ForexSecurity tsec = new ForexSecurity(sym);
            tsec.LotSize = 1;
            tsec.OrderStepSize = 1;

            const string me = "tester";
            const string other = "anotherguy";
            SimAccount a = new SimAccount(me);
            SimAccount b = new SimAccount(other);
            SimAccount c = new SimAccount("sleeper");
            OrderImpl oa = new OrderImpl(tsec, Direction.Long, 100);
            OrderImpl ob = new OrderImpl(tsec, Direction.Long, 100);

            oa.AccountName = me;
            ob.AccountName = other;
            // send order to accounts
            PendingOrderImpl poa = new PendingOrderImpl(oa, a);
            PendingOrderImpl pob = new PendingOrderImpl(ob, b);

            _broker.SendOrderStatus(poa);
            _broker.SendOrderStatus(pob);

            TickImpl t = new TickImpl(sym)
            {
                Trade = 100m,
                Size = 200
            };
            Assert.Equal(2, _broker.Execute(t));
            Position apos = _broker.GetOpenPosition(tsec, a);
            Position bpos = _broker.GetOpenPosition(tsec, b);
            Position cpos = _broker.GetOpenPosition(tsec, c);
            Assert.True(apos.IsLong);
            Assert.Equal(100, apos.Size);
            Assert.True(bpos.IsLong);
            Assert.Equal(100, bpos.Size);
            Assert.True(cpos.IsFlat);
            // make sure that default account doesn't register
            // any trades
            Assert.True(_broker.GetOpenPosition(tsec).IsFlat);
        }

        [Fact(Skip = "Reimplement")]
        [Trait("Quantler.Common", "Quantler")]
        public void OpGs()
        {
            SimBroker broker = new SimBroker();
            broker.BrokerModel = _trans;
            const string s = "NYS";
            ForexSecurity tsec = new ForexSecurity(s);
            tsec.LotSize = 1;
            tsec.OrderStepSize = 1;

            // build and send an OPG order
            OrderImpl opg = new OrderImpl(tsec, Direction.Long, 200, 10);
            PendingOrderImpl popg = new PendingOrderImpl(opg);
            Assert.Equal(0, broker.SendOrderStatus(popg));

            // build a tick on another exchange
            TickImpl it = TickImpl.NewTrade(s, 9, 100);
            it.Exchange = "ISLD";

            // fill order (should fail)
            int c = broker.Execute(it);
            Assert.Equal(0, c);

            // build opening price for desired exchange
            TickImpl nt = TickImpl.NewTrade(s, 9, 10000);
            nt.Exchange = "NYS";
            // fill order (should work)

            c = broker.Execute(nt);

            Assert.Equal(1, c);

            // add another OPG, make sure it's not filled with another tick

            TickImpl next = TickImpl.NewTrade(s, 9, 2000);
            next.Exchange = "NYS";

            OrderImpl late = new OrderImpl(tsec, Direction.Long, 200, 10);
            PendingOrderImpl plate = new PendingOrderImpl(late);
            broker.SendOrderStatus(plate);
            c = broker.Execute(next);
            Assert.Equal(0, c);
        }

        #endregion Public Methods

        #region Private Methods

        private void broker_GotFill(Trade t, PendingOrder o)
        {
            _fills++;
        }

        private void broker_GotOrder(PendingOrder o)
        {
            _orders++;
        }

        #endregion Private Methods
    }
}