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
    /// Price types used for creating a bar from ticks
    /// </summary>
    public enum BarPriceType
    {
        /// <summary>
        /// Mid price between Bid and Ask (Bid+Ask)/2
        /// </summary>
        MidPoint,

        /// <summary>
        /// Bid price from ticks
        /// </summary>
        Bid,

        /// <summary>
        /// Ask price from ticks
        /// </summary>
        Ask
    }
}