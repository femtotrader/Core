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

namespace Quantler.Interfaces
{
    /// <summary>
    /// </summary>
    public enum BarInterval
    {
        /// <summary>
        /// custom tick bars
        /// </summary>
        CustomTicks = -2,

        /// <summary>
        /// custom interval length
        /// </summary>
        CustomTime = -1,

        /// <summary>
        /// One-minute intervals
        /// </summary>
        Minute = 60,

        /// <summary>
        /// Five-minute interval
        /// </summary>
        FiveMin = 300,

        /// <summary>
        /// FifteenMinute intervals
        /// </summary>
        FifteenMin = 900,

        /// <summary>
        /// Hour-long intervals
        /// </summary>
        ThirtyMin = 1800,

        /// <summary>
        /// Hour-long intervals
        /// </summary>
        Hour = 3600,

        /// <summary>
        /// Day-long intervals
        /// </summary>
        Day = 86400,
    }
}