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
using Quantler.Orders;
using Quantler.Securities;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestOrder
    {
        #region Private Fields

        private ForexSecurity security = new ForexSecurity("SYM") { LotSize = 1, PipSize = 0.0001M, Type = SecurityType.Forex };

        private Research.ZeroTransactionCosts trans = new Research.ZeroTransactionCosts();

        #endregion Private Fields

        #region Public Constructors

        public TestOrder()
        {
        }

        #endregion Public Constructors

        #region Public Methods

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Defaults()
        {
            // assert that a default order is:
            // not valid, not filled
            OrderImpl o = new OrderImpl();
            Assert.True(!o.IsValid);
            Assert.True(!o.IsFilled);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Fill()
        {
            // market should fill on trade but not on quote
            OrderImpl o = new OrderImpl(security, Direction.Long, 100);
            Assert.True(o.Fill(TickImpl.NewTrade(security.Name, 9, 100), trans) == StatusType.ORDER_FILLED);
            Assert.True(o.Fill(TickImpl.NewBid(security.Name, 8, 100), trans) == StatusType.OK);

            // buy limit

            // limit should fill if order price is inside market
            o = new OrderImpl(security, Direction.Long, 100, 10m);
            Assert.True(o.Fill(TickImpl.NewTrade(security.Name, 9, 100), trans) == StatusType.ORDER_FILLED);
            // shouldn't fill outside market
            o = new OrderImpl(security, Direction.Long, 100, 10m);
            Assert.True(o.Fill(TickImpl.NewTrade(security.Name, 11, 100), trans) == StatusType.OK);

            // sell limit

            // limit should fill if order price is inside market
            o = new OrderImpl(security, Direction.Short, 100, 10m);
            Assert.True(o.Fill(TickImpl.NewTrade(security.Name, 11, 100), trans) == StatusType.ORDER_FILLED);
            // shouldn't fill outside market
            o = new OrderImpl(security, Direction.Short, 100, 10m);
            Assert.True(o.Fill(TickImpl.NewTrade(security.Name, 9, 100), trans) == StatusType.OK);

            // buy stop

            o = new OrderImpl(security, Direction.Long, 100, 0, 10m);
            Assert.True(o.Fill(TickImpl.NewTrade(security.Name, 11, 100), trans) == StatusType.ORDER_FILLED);
            // shouldn't fill outside market
            o = new OrderImpl(security, Direction.Long, 100, 0, 10m);
            Assert.True(o.Fill(TickImpl.NewTrade(security.Name, 9, 100), trans) == StatusType.OK);

            // sell stop

            o = new OrderImpl(security, Direction.Short, 100, 0, 10m);
            Assert.True(o.Fill(TickImpl.NewTrade(security.Name, 9, 100), trans) == StatusType.ORDER_FILLED);
            // shouldn't fill outside market
            o = new OrderImpl(security, Direction.Short, 100, 0, 10m);
            Assert.True(o.Fill(TickImpl.NewTrade(security.Name, 11, 100), trans) == StatusType.OK);

            // always fail filling an invalid tick
            o = new OrderImpl(security, Direction.Long, 100);
            Assert.False(o.Fill(TickImpl.NewTrade(security.Name, 0, 0), trans) == StatusType.ORDER_FILLED);

            // always fail filling invalid order
            o = new OrderImpl(security, Direction.Long, 100, 10);
            OrderImpl x = new OrderImpl();
            Assert.False(o.Fill(x) == StatusType.ORDER_FILLED);

            // always fail filling an order that doesn't cross market
            x = new OrderImpl(security, Direction.Long, 100);
            Assert.False(o.Fill(x) == StatusType.ORDER_FILLED);

            const string t2 = "agent2";
            // succeed on crossing market
            x = new OrderImpl(security, Direction.Short, 100);
            x.AccountName = t2;
            Assert.True(o.Fill(x) == StatusType.ORDER_FILLED);

            // fail when accounts are the same
            x = new OrderImpl(security, Direction.Short, 100);
            x.AccountName = o.AccountName;
            Assert.False(o.Fill(x) == StatusType.OK);

            // fail on match outside of market
            x = new OrderImpl(security, Direction.Long, 100, 11);
            x.AccountName = t2;
            Assert.False(o.Fill(x) == StatusType.OK);

            // succeed on limit cross
            o = new OrderImpl(security, Direction.Long, 100, 10);
            x = new OrderImpl(security, Direction.Short, 100, 10);
            x.AccountName = t2;
            Assert.True(o.Fill(x) == StatusType.ORDER_FILLED);

            // make sure we can stop cross
            o = new OrderImpl(security, Direction.Short, 100, 0, 10);
            x = new OrderImpl(security, Direction.Long, 100);
            x.AccountName = t2;
            Assert.True(o.Fill(x) == StatusType.ORDER_FILLED);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void FillBidAsk()
        {
            string s = security.Name;
            // market should fill on trade but not on quote
            OrderImpl o = new OrderImpl(security, Direction.Long, 100);
            Assert.True(o.FillBidAsk(TickImpl.NewAsk(s, 9, 100), trans) == StatusType.ORDER_FILLED);
            Assert.True(o.FillBidAsk(TickImpl.NewTrade(s, 9, 100), trans) == StatusType.OFF_QUOTES);
            Assert.True(o.FillBidAsk(TickImpl.NewBid(s, 8, 100), trans) == StatusType.OFF_QUOTES);

            // buy limit

            // limit should fill if order price is inside market
            o = new OrderImpl(security, Direction.Long, 100, 10m);
            Assert.True(o.FillBidAsk(TickImpl.NewAsk(s, 9, 100), trans) == StatusType.ORDER_FILLED);
            // shouldn't fill outside market
            o = new OrderImpl(security, Direction.Long, 100, 10m);
            Assert.True(o.FillBidAsk(TickImpl.NewTrade(s, 11, 100), trans) == StatusType.OFF_QUOTES);
            Assert.True(o.FillBidAsk(TickImpl.NewAsk(s, 11, 100), trans) == StatusType.OK);
            Assert.True(o.FillBidAsk(TickImpl.NewBid(s, 10, 100), trans) == StatusType.OFF_QUOTES);

            // sell limit

            // limit should fill if order price is inside market
            o = new OrderImpl(security, Direction.Short, 100, 10m);
            Assert.True(o.FillBidAsk(TickImpl.NewBid(s, 11, 100), trans) == StatusType.ORDER_FILLED);
            // shouldn't fill outside market
            o = new OrderImpl(security, Direction.Short, 100, 10m);
            Assert.True(o.FillBidAsk(TickImpl.NewTrade(s, 9, 100), trans) == StatusType.OFF_QUOTES);

            // buy stop

            o = new OrderImpl(security, Direction.Long, 100, 0, 10m);
            Assert.True(o.Type == OrderType.Stop);
            Assert.True(o.FillBidAsk(TickImpl.NewAsk(s, 11, 100), trans) == StatusType.ORDER_FILLED);
            // shouldn't fill outside market
            o = new OrderImpl(security, Direction.Long, 100, 0, 10m);
            Assert.True(o.FillBidAsk(TickImpl.NewTrade(s, 9, 100), trans) == StatusType.OFF_QUOTES);

            // sell stop

            o = new OrderImpl(security, Direction.Short, 100, 0, 10m);
            Assert.True(o.FillBidAsk(TickImpl.NewBid(s, 9, 100), trans) == StatusType.ORDER_FILLED);
            // shouldn't fill outside market
            o = new OrderImpl(security, Direction.Short, 100, 0, 10m);
            Assert.True(o.FillBidAsk(TickImpl.NewTrade(s, 11, 100), trans) == StatusType.OFF_QUOTES);

            // always fail filling an invalid tick
            o = new OrderImpl(security, Direction.Long, 100);
            Assert.True(o.FillBidAsk(TickImpl.NewTrade(s, 0, 0), trans) == StatusType.OFF_QUOTES);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void IdentityLimits()
        {
            Direction direction = Direction.Long;
            int quantity = 51;
            decimal limitprice = 120.13m;
            string comment = "Hello, World!";

            Order orig = new OrderImpl(security, direction, quantity, limitprice, 0, comment);
            Order comp;

            //Test Market Stop Order Sell Initialization
            comp = new OrderImpl(security, direction, quantity, limitprice, 0, comment);
            Assert.Equal(orig.Direction, comp.Direction);
            Assert.Equal(orig.Type, comp.Type);
            Assert.Equal(orig.Symbol, comp.Symbol);
            Assert.Equal(orig.Size, comp.Size);
            Assert.Equal(orig.StopPrice, comp.StopPrice);
            Assert.Equal(orig.LimitPrice, comp.LimitPrice);
            Assert.Equal(orig.Type, OrderType.Limit);
            Assert.Equal(orig.Comment, comp.Comment);

            //Test Market Stop Order Buy Initialization
            direction = Direction.Long;
            orig = new OrderImpl(security, direction, quantity, limitprice, 0);
            comp = new OrderImpl(security, direction, quantity, limitprice, 0);
            Assert.Equal(orig.Direction, comp.Direction);
            Assert.Equal(orig.Type, comp.Type);
            Assert.Equal(orig.Symbol, comp.Symbol);
            Assert.Equal(orig.Size, comp.Size);
            Assert.Equal(orig.StopPrice, comp.StopPrice);
            Assert.Equal(orig.LimitPrice, comp.LimitPrice);
            Assert.Equal(orig.Type, OrderType.Limit);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void IdentityStops()
        {
            Direction direction = Direction.Long;
            int quantity = 51;
            decimal stop = 134.40m;
            string comment = "Hello, World!";

            Order orig = new OrderImpl(security, direction, quantity, 0, stop, comment);
            Order comp;

            //Test Market Stop Order Sell Initialization
            comp = new OrderImpl(security, direction, quantity, 0, stop, comment);
            Assert.Equal(orig.Direction, comp.Direction);
            Assert.Equal(orig.Type, comp.Type);
            Assert.Equal(orig.Symbol, comp.Symbol);
            Assert.Equal(orig.Size, comp.Size);
            Assert.Equal(orig.StopPrice, comp.StopPrice);
            Assert.Equal(orig.LimitPrice, comp.LimitPrice);
            Assert.Equal(orig.Type, OrderType.Stop);
            Assert.Equal(orig.Comment, comp.Comment);

            //Test Market Limit Stop Order Sell Initialization
            orig = new OrderImpl(security, direction, quantity, 10, stop);
            comp = new OrderImpl(security, direction, quantity, 10, stop);
            Assert.Equal(orig.Direction, comp.Direction);
            Assert.Equal(orig.Type, comp.Type);
            Assert.Equal(orig.Symbol, comp.Symbol);
            Assert.Equal(orig.Size, comp.Size);
            Assert.Equal(orig.StopPrice, comp.StopPrice);
            Assert.Equal(orig.LimitPrice, comp.LimitPrice);
            Assert.Equal(orig.Type, OrderType.StopLimit);

            //Test Market Stop Order Buy Initialization
            direction = Direction.Long;
            orig = new OrderImpl(security, direction, quantity, 0, stop);
            comp = new OrderImpl(security, direction, quantity, 0, stop);
            Assert.Equal(orig.Direction, comp.Direction);
            Assert.Equal(orig.Type, comp.Type);
            Assert.Equal(orig.Symbol, comp.Symbol);
            Assert.Equal(orig.Size, comp.Size);
            Assert.Equal(orig.StopPrice, comp.StopPrice);
            Assert.Equal(orig.LimitPrice, comp.LimitPrice);
            Assert.Equal(orig.Type, OrderType.Stop);

            //Test Market Limit Stop Order Buy Initialization
            comp = new OrderImpl(security, direction, quantity, 10, stop);
            orig = new OrderImpl(security, direction, quantity, 10, stop);
            Assert.Equal(orig.Direction, comp.Direction);
            Assert.Equal(orig.Type, comp.Type);
            Assert.Equal(orig.Symbol, comp.Symbol);
            Assert.Equal(orig.Direction, comp.Direction);
            Assert.Equal(orig.Size, comp.Size);
            Assert.Equal(orig.StopPrice, comp.StopPrice);
            Assert.Equal(orig.LimitPrice, comp.LimitPrice);
            Assert.Equal(orig.Type, OrderType.StopLimit);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void MarketOrder()
        {
            const string s = "SYM";
            OrderImpl o = new OrderImpl(security, Direction.Long, 1);
            Assert.True(o.IsValid);
            Assert.True(o.Direction == Direction.Long);
            Assert.True(o.Type == OrderType.Market);
            Assert.True(!o.IsFilled);
            Assert.True(o.Symbol == s);
        }

        #endregion Public Methods
    }
}