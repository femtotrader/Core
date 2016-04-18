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
    /// Stream of data arriving from a source and can be used to build up bars
    /// </summary>
    public interface DataStream : BarListTracker
    {
        #region Public Properties

        /// <summary>
        /// During Backtesting, this is the last datapoint selected for this datastream
        /// </summary>
        DateTime EndDateTime { get; }

        /// <summary>
        /// Current datastream id
        /// </summary>
        int Id { get; }

        /// <summary>
        /// During Backtesting, this is the first datapoint selected for this datastream
        /// </summary>
        DateTime StartDateTime { get; }

        /// <summary>
        /// Associated security
        /// </summary>
        ISecurity Security { get; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Add interval for creating bars, interval in seconds
        /// </summary>
        /// <param name="interval"></param>
        void AddInterval(int interval);

        /// <summary>
        /// Add multiple intervals for creating bars, interval in seconds
        /// </summary>
        /// <param name="intervals"></param>
        void AddInterval(int[] intervals);

        /// <summary>
        /// Add interval for creating bars, interval in enum form
        /// </summary>
        /// <param name="interval"></param>
        void AddInterval(BarInterval interval);

        /// <summary>
        /// Add interval for creating bars, interval in timespan
        /// </summary>
        /// <param name="interval"></param>
        void AddInterval(TimeSpan interval);

        #endregion Public Methods
    }
}