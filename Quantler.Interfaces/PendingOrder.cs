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
    public interface PendingOrder
    {
        #region Public Events

        /// <summary>
        /// Event is fired when the order should be cancelled at the broker
        /// </summary>
        event OrderCancelDelegate OnCancel;

        /// <summary>
        /// Event is fired each time this order has been changed
        /// </summary>
        event OrderUpdateDelegate OnUpdate;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Account at which this order will be executed at
        /// </summary>
        IAccount Account { get; }

        /// <summary>
        /// Order ownerid
        /// </summary>
        int AgentId { get; }

        /// <summary>
        /// If true, this order has been cancelled or marked as cancelled
        /// </summary>
        bool IsCancelled { get; }

        /// <summary>
        /// The physical order
        /// </summary>
        Order Order { get; }

        /// <summary>
        /// Current order id
        /// </summary>
        long OrderId { get; }

        /// <summary>
        /// Current order status
        /// </summary>
        StatusType OrderStatus { get; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Cancel this order (will be send to broker right away)
        /// </summary>
        void Cancel();

        #endregion Public Methods

        /// <summary>
        /// Update properties on this order
        /// </summary>
        /// <param name="updates"></param>
        void Update(Action<OrderUpdate> updates);
    }
}