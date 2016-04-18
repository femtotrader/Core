﻿#region License
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
    public enum OrderType
    {
        /// <summary>
        /// Execute order based on the limit price
        /// </summary>
        Limit,

        /// <summary>
        /// Execute order based on the best market price
        /// </summary>
        Market,

        /// <summary>
        /// Order size is based on the current flatsize
        /// </summary>
        MarketFlat,

        /// <summary>
        /// Order is only executed on the stop price
        /// </summary>
        Stop,

        /// <summary>
        /// Order is only executed on the stop price for a specific fill price
        /// </summary>
        StopLimit
    }
}