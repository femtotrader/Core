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

namespace Quantler.Orders
{
    public class OrderUpdateImpl : OrderUpdate
    {
        #region Public Properties

        /// <summary>
        /// Alter the comment for this order
        /// </summary>
        public string Comment
        {
            get;
            set;
        }

        /// <summary>
        /// Set a new limit price, if this was a market order, the order type will change
        /// </summary>
        public decimal? LimitPrice
        {
            get;
            set;
        }

        /// <summary>
        /// Set a new order size based on its quantity
        /// </summary>
        public decimal? Quantity
        {
            get;
            set;
        }

        /// <summary>
        /// Set a new order size
        /// </summary>
        public int? Size
        {
            get;
            set;
        }

        /// <summary>
        /// Set a new stop price for this order, if this order was a market order, the order type will change
        /// </summary>
        public decimal? StopPrice
        {
            get;
            set;
        }

        #endregion Public Properties
    }
}