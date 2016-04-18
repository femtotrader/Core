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
    /// Indexer for retrieving specific bar information
    /// </summary>
    public interface BarIndexer
    {
        #region Public Indexers

        /// <summary>
        /// Get Bar based on the default timeframe
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        Bar this[string symbol] { get; }

        /// <summary>
        /// Get Bar based on a specific timeframe
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        Bar this[string symbol, int interval] { get; }

        /// <summary>
        /// Get Bar based on a specific timeframe
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        Bar this[string symbol, TimeSpan interval] { get; }

        /// <summary>
        /// Get Bar based on a specific timeframe and x amount of bars back
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="barsBack"></param>
        /// <returns></returns>
        Bar this[string symbol, int interval, int barsBack] { get; }

        /// <summary>
        /// Get Bar based on a specific timeframe and x amount of bars back
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="barsBack"></param>
        /// <returns></returns>
        Bar this[string symbol, TimeSpan interval, int barsBack] { get; }

        /// <summary>
        /// Get Bar based on a security and using its default timeframe
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        Bar this[ISecurity symbol] { get; }

        /// <summary>
        /// Get Bar based on security and a specific timeframe
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        Bar this[ISecurity symbol, int interval] { get; }

        /// <summary>
        /// Get Bar based on security and a specific timeframe
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        Bar this[ISecurity symbol, TimeSpan interval] { get; }

        /// <summary>
        /// Get Bar based on a specific timeframe and x amount of bars back
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="barsBack"></param>
        /// <returns></returns>
        Bar this[ISecurity symbol, int interval, int barsBack] { get; }

        /// <summary>
        /// Get Bar based on a specific timeframe and x amount of bars back
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="barsBack"></param>
        /// <returns></returns>
        Bar this[ISecurity symbol, TimeSpan interval, int barsBack] { get; }

        #endregion Public Indexers
    }
}