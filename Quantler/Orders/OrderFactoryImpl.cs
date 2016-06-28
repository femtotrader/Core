#region License
/*
Copyright (c) Quantler B.V., All rights reserved.

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
using System.Linq;

namespace Quantler.Orders
{
    /// <summary>
    /// Order factory is used to create new orders and specify which implementations to use for creating orders
    /// </summary>
    public class OrderFactoryImpl : OrderFactory
    {
        #region Private Fields

        private readonly PortfolioManager _portfolio;

        #endregion Private Fields

        #region Public Constructors

        public OrderFactoryImpl(PortfolioManager portfolio)
        {
            _portfolio = portfolio;
        }

        #endregion Public Constructors

        #region Public Methods

        public PendingOrder CreateOrder(ISecurity security, Direction direction, decimal quantity, decimal limitPrice, decimal stopPrice, string comment, int agentid)
        {
            return CreateOrder(security.Name, direction, quantity, limitPrice, stopPrice, comment, agentid);
        }

        public PendingOrder CreateOrder(string symbol, Direction direction, decimal quantity, decimal limitPrice, decimal stopPrice, string comment, int agentid)
        {
            //Order creation logic
            ISecurity security = _portfolio.Securities[symbol];

            //Pre-Check flatten order
            //Create a market flat order
            if (direction == Direction.Flat)
            {
                var position = _portfolio.Positions[security];
                if (!position.IsFlat)
                    quantity = position.FlatSize / security.LotSize;
                if (position.IsLong || _portfolio.Agents.First(x => x.AgentId == agentid).CurrentState[symbol].TrueForAll(x => x == AgentState.EntryLong))
                    direction = Direction.Short;
                else if (position.IsShort || _portfolio.Agents.First(x => x.AgentId == agentid).CurrentState[symbol].TrueForAll(x => x == AgentState.EntryShort))
                    direction = Direction.Long;
            }

            //Set initial objects
            OrderImpl norder = new OrderImpl(security, direction, quantity, limitPrice, stopPrice, comment)
            {
                AgentId = agentid,
                ValidInstruct = OrderInstructionType.GTC
            };

            var toreturn = new PendingOrderImpl(norder, _portfolio.Account)
            {
                OrderStatus = security == null ? StatusType.SYMBOL_NOT_LOADED : StatusType.OK
            };

            //Check for security

            //Check order action and sizing
            norder.Quantity = quantity;
            if (direction != Direction.Short && quantity < 0)
            {
                toreturn.OrderStatus = StatusType.ORDER_INVALID_VOLUME;
                toreturn.Cancel();
            }

            //Check order size is positive
            if(quantity == 0)
            {
                toreturn.OrderStatus = StatusType.ORDER_INVALID_VOLUME;
                toreturn.Cancel();
            }

            //Check order quantity stepsize
            if (norder.Quantity % norder.Security.OrderStepQuantity > 0)
            {
                toreturn.OrderStatus = StatusType.ORDER_INVALID_VOLUME;
                toreturn.Cancel();
            }

            //Check order minimum size
            if(norder.Size < norder.Security.OrderMinSize)
            {
                toreturn.OrderStatus = StatusType.ORDER_INVALID_VOLUME;
                toreturn.Cancel();
            }

            //Check order type
            //Set Order Limit Price
            if (limitPrice >= 0)
                norder.LimitPrice = limitPrice;
            else
            {
                toreturn.OrderStatus = StatusType.ORDER_INVALID_PRICE;
                toreturn.Cancel();
            }

            //Create a stop order
            if (stopPrice >= 0)
                norder.StopPrice = stopPrice;
            else
            {
                toreturn.OrderStatus = StatusType.ORDER_INVALID_STOP;
                toreturn.Cancel();
            }

            //Return the newly created, pending state order
            return toreturn;
        }

        #endregion Public Methods
    }
}