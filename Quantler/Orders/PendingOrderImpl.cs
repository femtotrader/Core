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
using System;

namespace Quantler.Orders
{
    public class PendingOrderImpl : PendingOrder
    {
        #region Private Fields

        private readonly OrderImpl _order;

        /// <summary>
        /// Initial Order status, data holder
        /// </summary>
        private StatusType _orderStatus = StatusType.OK;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Create a new pending order, based on the requested order object
        /// </summary>
        /// <param name="order"></param>
        public PendingOrderImpl(OrderImpl order)
        {
            _order = order;
        }

        /// <summary>
        /// Send a pendingorder to a sepcific account
        /// </summary>
        /// <param name="order"></param>
        /// <param name="account"></param>
        public PendingOrderImpl(OrderImpl order, IAccount account)
        {
            _order = order;
            Account = account;
        }

        #endregion Public Constructors

        #region Public Events

        /// <summary>
        /// Fired on the event that this order has been cancelled
        /// </summary>
        public event OrderCancelDelegate OnCancel;

        /// <summary>
        /// Fired on the event this order contains an update
        /// </summary>
        public event OrderUpdateDelegate OnUpdate;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Associated account for this order
        /// </summary>
        public IAccount Account
        {
            get;
            private set;
        }

        /// <summary>
        /// Trading Agent that created this pending order
        /// </summary>
        public int AgentId
        {
            get
            {
                return _order.AgentId;
            }
        }

        /// <summary>
        /// True if the order has been cancelled
        /// </summary>
        public bool IsCancelled { get; private set; }

        /// <summary>
        /// Order container
        /// </summary>
        public Order Order
        {
            get { return _order; }
        }

        /// <summary>
        /// Get the orderid as defined by the broker
        /// </summary>
        public long OrderId
        {
            get { return Order.Id; }
        }

        /// <summary>
        /// Get the current order status, changes will fire the update event
        /// </summary>
        public StatusType OrderStatus
        {
            get { return _orderStatus; }
            set
            {
                //Check if this really is an changed update event
                if (value == _orderStatus)
                    return;

                //Set and send our new status
                _orderStatus = value;
                if (OnUpdate != null)
                    OnUpdate(this);
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Cancel this order, if possible
        /// </summary>
        public void Cancel()
        {
            //Check if we are not already cancelled and if we need to fire an event
            if (OnCancel != null && !IsCancelled)
            {
                IsCancelled = true;
                OnCancel(this);
            }
            else
                IsCancelled = true;
        }

        /// <summary>
        /// Update this order
        /// </summary>
        /// <param name="updates"></param>
        public void Update(Action<OrderUpdate> updates)
        {
            //Invoke for new updates
            var changes = new OrderUpdateImpl();
            updates.Invoke(changes);

            if (!string.IsNullOrWhiteSpace(changes.Comment))
                _order.Comment = changes.Comment;
            if (changes.LimitPrice.HasValue)
                _order.LimitPrice = changes.LimitPrice.Value;
            if (changes.Size.HasValue)
                _order.Size = changes.Size.Value;
            if (changes.Quantity.HasValue)
                _order.Quantity = changes.Quantity.Value;
            if (changes.StopPrice.HasValue)
                _order.StopPrice = changes.StopPrice.Value;

            if (OnUpdate != null)
                OnUpdate(this);
        }

        #endregion Public Methods
    }
}