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

namespace Quantler.Interfaces
{
    /// <summary>
    /// Order factory is used to create new orders and specify which implementations to use for creating orders
    /// </summary>
    public interface OrderFactory
    {
        #region Public Methods

        /// <summary>
        /// Create a new order, returned pendingorder is the order ticket and will be valid untill the order is filled and turned into a position
        /// </summary>
        /// <param name="security"></param>
        /// <param name="direction"></param>
        /// <param name="quantity"></param>
        /// <param name="LimitPrice"></param>
        /// <param name="StopPrice"></param>
        /// <param name="Comment"></param>
        /// <param name="agentid"></param>
        /// <returns></returns>
        PendingOrder CreateOrder(ISecurity security, Direction direction, decimal quantity, decimal LimitPrice, decimal StopPrice, string Comment, int agentid);

        /// <summary>
        /// Create a new order, returned pendingorder is the order ticket and will be valid untill the order is filled and turned into a position
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="direction"></param>
        /// <param name="quantity"></param>
        /// <param name="LimitPrice"></param>
        /// <param name="StopPrice"></param>
        /// <param name="Comment"></param>
        /// <param name="agentid"></param>
        /// <returns></returns>
        PendingOrder CreateOrder(string symbol, Direction direction, decimal quantity, decimal LimitPrice, decimal StopPrice, string Comment, int agentid);

        #endregion Public Methods
    }
}