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
using System.Collections.Generic;

namespace Quantler
{
    /// <summary>
    /// Container class for all linq extensions
    /// </summary>
    public static class LinqExtensions
    {
        #region Public Methods

        /// <summary>
        /// Cancel all orders in the current IEnumerable
        /// </summary>
        /// <param name="source"></param>
        public static IEnumerable<PendingOrder> Cancel(this IEnumerable<PendingOrder> source)
        {
            foreach (var pendingorder in source)
                pendingorder.Cancel();

            return source;
        }

        /// <summary>
        /// Update all orders in the current IEnumerable
        /// </summary>
        /// <param name="source"></param>
        public static IEnumerable<PendingOrder> Update(this IEnumerable<PendingOrder> source, Action<OrderUpdate> updateAction)
        {
            foreach (var pendingorder in source)
                pendingorder.Update(updateAction);

            return source;
        }

        #endregion Public Methods
    }
}