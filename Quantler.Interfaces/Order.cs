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

using System;

namespace Quantler.Interfaces
{
    /// <summary>
    /// Quantler Order
    /// </summary>
    public interface Order
    {
        #region Public Properties

        /// <summary>
        /// account to place inventory if order is executed
        /// </summary>
        string AccountName { get; }

        /// <summary>
        /// owner/originator of this order
        /// </summary>
        int AgentId { get; }

        /// <summary>
        /// Symbol name as defined at the broker (can be different from internal naming)
        /// </summary>
        string BrokerSymbol { get; }

        /// <summary>
        /// order comment
        /// </summary>
        string Comment { get; }

        /// <summary>
        /// Get the order created date and time object
        /// </summary>
        DateTime Created { get; }

        /// <summary>
        /// Direction of the order either Buy or Sell
        /// </summary>
        Direction Direction { get; }

        /// <summary>
        /// destination for order
        /// </summary>
        string Exchange { get; }

        /// <summary>
        /// order id
        /// </summary>
        long Id { get; }

        /// <summary>
        /// whether order has been filled
        /// </summary>
        bool IsFilled { get; }

        /// <summary>
        /// order is valid
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// price of order. (0 for market)
        /// </summary>
        decimal LimitPrice { get; }

        /// <summary>
        /// Lotsize that is linked to this order
        /// </summary>
        int LotSize { get; }

        /// <summary>
        /// Order quantity (size / lotsize)
        /// </summary>
        decimal Quantity { get; }

        /// <summary>
        /// security/contract information for order
        /// </summary>
        ISecurity Security { get; }

        /// <summary>
        /// signed size of order
        /// </summary>
        int Size { get; }

        /// <summary>
        /// stop price if applicable
        /// </summary>
        decimal StopPrice { get; }

        /// <summary>
        /// symbol of order
        /// </summary>
        string Symbol { get; }

        /// <summary>
        /// Get Type of order
        /// </summary>
        OrderType Type { get; }

        /// <summary>
        /// unsigned size of order
        /// </summary>
        int UnsignedSize { get; }

        /// <summary>
        /// valid instruction
        /// </summary>
        OrderInstructionType ValidInstruct { get; }

        #endregion Public Properties
    }
}