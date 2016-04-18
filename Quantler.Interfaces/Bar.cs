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

using System;

namespace Quantler.Interfaces
{
    public interface Bar
    {
        #region Public Properties

        /// <summary>
        /// Bar Date
        /// </summary>
        int Bardate { get; }

        /// <summary>
        /// Bar date and time
        /// </summary>
        DateTime BarDateTime { get; }

        /// <summary>
        /// Type or bar interval amount
        /// </summary>
        BarInterval BarInterval { get; }

        /// <summary>
        /// Bar time
        /// </summary>
        int Bartime { get; }

        /// <summary>
        /// Bar closing price
        /// </summary>
        decimal Close { get; }

        /// <summary>
        /// Custom interval in seconds (60 == 1 minute)
        /// </summary>
        int CustomInterval { get; }

        /// <summary>
        /// Bar closing high price
        /// </summary>
        decimal High { get; }

        /// <summary>
        /// Internal bar identifier
        /// </summary>
        long Id { get; }

        /// <summary>
        /// Current interval in seconds (60 == 1 minute)
        /// </summary>
        int Interval { get; }

        /// <summary>
        /// Checked if this bar is based on a custom interval
        /// </summary>
        bool IsCustom { get; }

        /// <summary>
        /// Checks if this bar is not closed yet
        /// </summary>
        bool IsNew { get; }

        /// <summary>
        /// Checks if this bar is valid
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Bar closing low price
        /// </summary>
        decimal Low { get; }

        /// <summary>
        /// Bar closing open price
        /// </summary>
        decimal Open { get; }

        /// <summary>
        /// Symbol name for this bar
        /// </summary>
        string Symbol { get; }

        /// <summary>
        /// Time of the bar
        /// </summary>
        int Time { get; }

        /// <summary>
        /// Total volume traded during this bar
        /// </summary>
        long Volume { get; }

        #endregion Public Properties
    }
}