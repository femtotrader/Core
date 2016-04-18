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
    /// General indicator implementation
    /// </summary>
    public interface Indicator
    {
        /// <summary>
        /// Timeframe this indicator is based upon and used for calculating its values
        /// </summary>
        TimeSpan BarSize { get; }

        /// <summary>
        /// Function to compute values
        /// </summary>
        Func<Bar, decimal> Compute { get; }

        /// <summary>
        /// All datastreams that feed this indicator with data
        /// </summary>
        DataStream[] DataStreams { get; }

        /// <summary>
        /// True if this indicator has valid data
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Indicator period on whcih the calculations are based
        /// </summary>
        int Period { get; }

        /// <summary>
        /// Calculate indicator values on bar updates
        /// </summary>
        /// <param name="bar"></param>
        void OnBar(Bar bar);
    }
}