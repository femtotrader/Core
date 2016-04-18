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
    public interface BarListTracker : GotTickIndicator
    {
        #region Public Events

        /// <summary>
        /// Trigger on each new bar created
        /// </summary>
        event SymBarIntervalDelegate GotNewBar;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Default interval used for this barlist
        /// </summary>
        TimeSpan DefaultInterval { get; }

        /// <summary>
        /// Current intervals to use for creating new bars
        /// </summary>
        int[] Intervals { get; }

        /// <summary>
        /// Number of symbols used for creating bars
        /// </summary>
        int SymbolCount { get; }

        #endregion Public Properties

        /// <summary>
        /// Get historical bars for a given interval
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        BarList this[int interval] { get; }

        /// <summary>
        /// Get historical bars for a given timespan interval
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        BarList this[TimeSpan interval] { get; }
    }
}