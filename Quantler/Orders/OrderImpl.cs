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

using Quantler.Interfaces;
using Quantler.Trades;
using System;

namespace Quantler.Orders
{
    /// <summary>
    /// Specify an order to buy or sell a quantity of a security.
    /// </summary>
    [Serializable]
    public class OrderImpl : TradeImpl, Order
    {
        #region Private Fields

        private int _size;
        private OrderInstructionType _instruct = OrderInstructionType.DAY;
        private decimal _price, _stopp;

        #endregion Private Fields

        #region Public Constructors

        public OrderImpl()
        {
        }

        public OrderImpl(Direction direction)
        {
            Direction = direction;
        }

        public OrderImpl(Order copythis)
        {
            Symbol = copythis.Symbol;
            StopPrice = copythis.StopPrice;

            AccountName = copythis.AccountName;
            Exchange = copythis.Exchange;
            LimitPrice = copythis.LimitPrice;
            Security = copythis.Security;
            Direction = copythis.Direction;
            Size = copythis.Size;
            BrokerSymbol = copythis.BrokerSymbol;
            Id = copythis.Id;
            ValidInstruct = copythis.ValidInstruct;
            AgentId = copythis.AgentId;
        }

        public OrderImpl(ISecurity sec, Direction direction, decimal quantity, decimal limitPrice = 0, decimal stopPrice = 0, string comment = "", int agentid = -1)
        {
            Security = sec;
            Quantity = quantity;
            LimitPrice = limitPrice;
            StopPrice = stopPrice;
            Comment = comment;
            Security = sec;
            Direction = direction;
            Symbol = sec.Name;
            Exchange = sec.DestEx;
            Created = sec.LastTickEvent;
            AgentId = agentid;
        }

        #endregion Public Constructors

        #region Public Properties

        public static long Unique { get { return DateTime.Now.Ticks; } }

        public DateTime Created { get; private set; }

        public bool IsValid
        {
            get
            {
                return (Symbol != null) && (Size != 0);
            }
        }

        public decimal LimitPrice { get { return _price; } set { _price = value; } }

        public int LotSize { get { return Security != null ? Security.LotSize : 1; } }

        public override decimal Price
        {
            get
            {
                return Type == OrderType.Stop ? StopPrice : LimitPrice;
            }
        }

        public decimal Quantity { get { return Size / (decimal)LotSize; } set { Size = Convert.ToInt32(value * LotSize); } }

        public int SignedSize { get { return Math.Abs(Size) * (Direction == Direction.Long ? 1 : -1); } }

        public int Size { get { return _size; } set { _size = value; } }

        public decimal StopPrice { get { return _stopp; } set { _stopp = value; } }

        public OrderType Type
        {
            get
            {
                return LimitPrice == 0 && StopPrice == 0 && Direction != Direction.Flat ? OrderType.Market :
                        LimitPrice == 0 && StopPrice != 0 ? OrderType.Stop :
                        LimitPrice != 0 && StopPrice != 0 ? OrderType.StopLimit :
                        LimitPrice != 0 && StopPrice == 0 ? OrderType.Limit :       //Could be that the order was changed from a MarketFlat to Limit order flat
                        OrderType.MarketFlat;
            }
        }

        public new int UnsignedSize { get { return Math.Abs(_size); } }

        public OrderInstructionType ValidInstruct { get { return _instruct; } set { _instruct = value; } }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Fills this order with a tick
        /// </summary>
        /// <param name="t"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public StatusType Fill(Tick t, BrokerModel c)
        {
            return Fill(t, c, false);
        }

        public StatusType Fill(Tick t, BrokerModel c, bool fillOpg)
        {
            if (!t.IsTrade)
                return StatusType.OK;
            if (t.Symbol != Symbol)
                return StatusType.OK;
            if (!fillOpg && (ValidInstruct == OrderInstructionType.OPG))
                return StatusType.INVALID_TRADE_PARAMETERS;

            //Set price P and add negatively impacting spread
            decimal p = ImpactPriceSpread(t.Trade, c.GetSpread(this));

            if ((Type == OrderType.Limit && Direction == Direction.Long && (p <= LimitPrice))                                     // buy limit
                || (Type == OrderType.Limit && Direction == Direction.Short && (p >= LimitPrice))                                 // sell limit
                || (Type == OrderType.Stop && Direction == Direction.Long && (p >= StopPrice))                                    // buy stop
                || (Type == OrderType.Stop && Direction == Direction.Short && (p <= StopPrice))                                   // sell stop
                || (Type == OrderType.StopLimit && Direction == Direction.Long && (p >= StopPrice) && (p <= LimitPrice))          // buy stop limit
                || (Type == OrderType.StopLimit && Direction == Direction.Short && (p <= StopPrice) && (p >= LimitPrice))         // sell stop limit
                || Type == OrderType.Market)
            {
                //Get trade price (factoring in slippage)
                Xprice = ImpactPriceSlippage(p, c.GetSlippage(this));

                //Set commissions
                Commission = c.GetCommission(this);

                //Add execution details
                Xsize = t.Size >= UnsignedSize ? UnsignedSize : t.Size;
                Xsize = Math.Abs(Xsize);
                Xsize *= Direction == Direction.Long ? 1 : -1;
                Xtime = t.Time;
                Xdate = t.Date;
                return StatusType.ORDER_FILLED;
            }
            return StatusType.OK;
        }

        /// <summary>
        /// fill against bid and ask rather than trade
        /// </summary>
        /// <param name="k"></param>
        /// <param name="c"></param>
        /// <param name="bidask"></param>
        /// <param name="fillOpg"></param>
        /// <returns></returns>
        public StatusType Fill(Tick k, BrokerModel c, bool bidask, bool fillOpg)
        {
            if (!bidask)
                return Fill(k, c, fillOpg);
            // buyer has to match with seller and vice verca
            bool ok = Direction == Direction.Long ? k.HasAsk : k.HasBid;
            if (!ok)
                return StatusType.OFF_QUOTES;

            //Get price and spread from tick and transaction costs
            decimal p = Direction == Direction.Long ? k.Ask : k.Bid;
            p = ImpactPriceSpread(p, c.GetSpread(this));

            int s = Direction == Direction.Long ? k.AskSize : k.BidSize;
            if (k.Symbol != Symbol)
                return StatusType.OK;
            if (!fillOpg && (ValidInstruct == OrderInstructionType.OPG))
                return StatusType.INVALID_TRADE_PARAMETERS;
            if (Created.AddMilliseconds(c.GetLatencyInMilliseconds(this)) > k.TickDateTime)
                return StatusType.OK;

            if ((Type == OrderType.Limit && Direction == Direction.Long && (p <= LimitPrice))                               // buy limit
                || (Type == OrderType.Limit && Direction == Direction.Short && (p >= LimitPrice))                           // sell limit
                || (Type == OrderType.Stop && Direction == Direction.Long && (p >= StopPrice))                              // buy stop
                || (Type == OrderType.Stop && Direction == Direction.Short && (p <= StopPrice))                             // sell stop
                || (Type == OrderType.StopLimit && Direction == Direction.Long && (p >= StopPrice) && (p <= LimitPrice))    // buy stop limit
                || (Type == OrderType.StopLimit && Direction == Direction.Short && (p <= StopPrice) && (p >= LimitPrice))   // sell stop limit
                || Type == OrderType.Market)
            {
                //Get trade price (factoring in slippage)
                Xprice = ImpactPriceSlippage(p, c.GetSlippage(this));

                //Set commissions
                Commission = c.GetCommission(this);

                Xsize = Math.Abs(Xsize);
                Xsize = (s >= UnsignedSize ? UnsignedSize : s) * (Direction == Direction.Long ? 1 : -1);
                Xtime = k.Time;
                Xdate = k.Date;
                return StatusType.ORDER_FILLED;
            }
            return StatusType.OK;
        }

        /// <summary>
        /// fill assuming high liquidity - fill stops and limits at their stop price rather than at
        /// bid, ask, or trade. primarily for use when only daily data is available.
        /// </summary>
        /// <param name="k"></param>
        /// <param name="c"></param>
        /// <param name="bidask"></param>
        /// <param name="fillOpg"></param>
        /// <param name="fillHighLiquidityEod"></param>
        /// <returns></returns>
        public StatusType Fill(Tick k, BrokerModel c, bool bidask, bool fillOpg, bool fillHighLiquidityEod)
        {
            if (!fillHighLiquidityEod || (Type != OrderType.Stop && Type != OrderType.Limit))
                return Fill(k, c, bidask, fillOpg);

            if (k.Symbol != Symbol)
                return StatusType.UNKNOWN_SYMBOL;
            if (!fillOpg && (ValidInstruct == OrderInstructionType.OPG))
                return StatusType.INVALID_TRADE_PARAMETERS;

            // determine size and activation price using bid-ask or trade method
            int s;
            decimal p;
            if (bidask)
            {
                bool ok = Direction == Direction.Long ? k.HasAsk : k.HasBid;
                if (!ok)
                    return StatusType.OFF_QUOTES;
                s = Direction == Direction.Long ? k.AskSize : k.BidSize;
                p = Direction == Direction.Long ? k.Ask : k.Bid;
            }
            else
            {
                if (!k.IsTrade)
                    return StatusType.INVALID_TRADE_PARAMETERS;
                s = k.Size;
                p = k.Trade;
            }

            // record the fill
            Xsize = (s >= UnsignedSize ? UnsignedSize : s) * (Direction == Direction.Long ? 1 : -1);
            Xtime = k.Time;
            Xdate = k.Date;

            if ((Type == OrderType.Limit && Direction == Direction.Long && (p <= LimitPrice))         // buy limit
             || (Type == OrderType.Limit && Direction == Direction.Short && (p >= LimitPrice)))       // sell limit
            {
                Xprice = LimitPrice;
                return StatusType.ORDER_FILLED;
            }
            if ((Type == OrderType.Stop && Direction == Direction.Long && (p >= StopPrice))         // buy stop
                || (Type == OrderType.Stop && Direction == Direction.Short && (p <= StopPrice)))    // sell stop
            {
                Xprice = StopPrice;
                return StatusType.ORDER_FILLED;
            }
            return StatusType.OK;
        }

        /// <summary>
        /// Try to fill incoming order against this order. If orders match.
        /// </summary>
        /// <param name="o"></param>
        /// <returns>
        /// order can be cast to valid Trade and function returns true. Otherwise, false
        /// </returns>
        public StatusType Fill(Order o)
        {
            // sides must match
            if (Direction == o.Direction) return StatusType.ORDER_INVALID_PRICE;
            // orders must be valid
            if (!o.IsValid || !IsValid) return StatusType.ORDER_INVALID_VOLUME;
            // accounts must not be different
            if (o.AccountName == AccountName) return StatusType.INVALID_ACCOUNT;
            if ((Type == OrderType.Limit && Direction == Direction.Long && (o.LimitPrice <= LimitPrice))                                          // buy limit
                || (Type == OrderType.Limit && Direction == Direction.Short && (o.LimitPrice >= LimitPrice))                                      // sell limit
                || (Type == OrderType.Stop && Direction == Direction.Long && (o.StopPrice >= StopPrice))                                          // buy stop
                || (Type == OrderType.Stop && Direction == Direction.Short && (o.StopPrice <= StopPrice))                                         // sell stop
                || (Type == OrderType.StopLimit && Direction == Direction.Long && (o.StopPrice >= StopPrice) && (o.LimitPrice <= LimitPrice))     // buy stop limit
                || (Type == OrderType.StopLimit && Direction == Direction.Short && (o.StopPrice <= StopPrice) && (o.LimitPrice >= LimitPrice)))   // sell stop limit
            {
                Xprice = o.Type == OrderType.Limit ? o.LimitPrice : o.StopPrice;
                if (Xprice == 0) Xprice = Type == OrderType.Limit ? Price : StopPrice;
                Xsize = Math.Abs(Xsize);
                Xsize = o.UnsignedSize >= UnsignedSize ? UnsignedSize : o.UnsignedSize;
                return IsFilled ? StatusType.ORDER_FILLED : StatusType.REQUOTE;
            }
            return StatusType.OK;
        }

        /// <summary>
        /// fill against bid and ask rather than trade
        /// </summary>
        /// <param name="k"></param>
        /// <param name="c"></param>
        /// <param name="opg"></param>
        /// <returns></returns>
        public StatusType FillBidAsk(Tick k, BrokerModel c, bool opg)
        {
            return Fill(k, c, true, opg);
        }

        /// <summary>
        /// fill against bid and ask rather than trade
        /// </summary>
        /// <param name="k"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public StatusType FillBidAsk(Tick k, BrokerModel c)
        {
            return Fill(k, c, true, false);
        }

        public override string ToString()
        {
            return ToString(2);
        }

        public string ToString(int decimals)
        {
            if (IsFilled) return base.ToString();
            return (Direction == Direction.Long ? "Long" : "Short") + UnsignedSize + " " + Symbol + "@" + (Type == OrderType.Market ? "Mkt" : (Type == OrderType.Limit || Type == OrderType.StopLimit ? LimitPrice.ToString("N" + decimals) : StopPrice.ToString("N" + decimals) + "stp")) + " [" + AccountName + "] " + Id + (Type == OrderType.Stop || Type == OrderType.StopLimit ? " stop: " + StopPrice.ToString("N" + decimals) : string.Empty);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Add impact of additional slippage on the price
        /// </summary>
        /// <param name="price"></param>
        /// <param name="slippage"></param>
        /// <returns></returns>
        private decimal ImpactPriceSlippage(decimal price, decimal slippage)
        {
            slippage /= 2;
            return Direction == Direction.Long ? price + slippage * Security.PipSize : price - slippage * Security.PipSize;
        }

        /// <summary>
        /// Add impact of additional spread on the price
        /// </summary>
        /// <param name="price"></param>
        /// <param name="spread"></param>
        /// <returns></returns>
        private decimal ImpactPriceSpread(decimal price, decimal spread)
        {
            spread /= 2;
            return Direction == Direction.Long ? price + spread * Security.PipSize : price - spread * Security.PipSize;
        }

        #endregion Private Methods
    }
}