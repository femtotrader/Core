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
    /// Collection of result values to be used for further analysis
    /// </summary>
    public interface DataSerie
    {
        #region Public Properties

        /// <summary>
        /// Number of currently available datapoints
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Returns the current value for this dataserie
        /// </summary>
        decimal CurrentValue { get; }

        /// <summary>
        /// True if the current value is lower than its previous value
        /// </summary>
        bool IsFalling { get; }

        /// <summary>
        /// True if the current value is higher than its previous value
        /// </summary>
        bool IsRising { get; }

        /// <summary>
        /// Maximum amount of values that are allowed to be stored in this collection of results.
        /// When this maximum is reached, previous values will be removed
        /// </summary>
        int MaxHistory { get; set; }

        #endregion Public Properties

        #region Public Indexers

        /// <summary>
        /// Return the given value on a specific point (-5 equals 5)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        decimal this[int index] { get; set; }

        #endregion Public Indexers

        #region Public Methods

        /// <summary>
        /// Returns the average value between the current value and (lookback) values ago
        /// </summary>
        /// <param name="lookback"></param>
        /// <returns></returns>
        decimal Avg(int lookback);

        /// <summary>
        /// Check if another dataserie result is crossing above the current dataserie
        /// </summary>
        /// <param name="b"></param>
        /// <param name="lookback"></param>
        /// <returns></returns>
        bool CrossedAbove(DataSerie b, int lookback = 3);

        /// <summary>
        /// Check if an array of values is crossing above the current dataserie
        /// </summary>
        /// <param name="seriesB"></param>
        /// <param name="lookback"></param>
        /// <returns></returns>
        bool CrossedAbove(decimal[] seriesB, int lookback = 3);

        /// <summary>
        /// Check if a specific value is crossing above the current dataserie
        /// </summary>
        /// <param name="seriesB"></param>
        /// <param name="lookback"></param>
        /// <returns></returns>
        bool CrossedAbove(decimal seriesB, int lookback = 3);

        /// <summary>
        /// Check if another dataserie result is crossing below the current dataserie
        /// </summary>
        /// <param name="b"></param>
        /// <param name="lookback"></param>
        /// <returns></returns>
        bool CrossedUnder(DataSerie b, int lookback = 3);

        /// <summary>
        /// Check if an array of values is crossing under the current dataserie
        /// </summary>
        /// <param name="seriesB"></param>
        /// <param name="lookback"></param>
        /// <returns></returns>
        bool CrossedUnder(decimal[] seriesB, int lookback = 3);

        /// <summary>
        /// Check if a specific value is crossing under the current dataserie
        /// </summary>
        /// <param name="seriesB"></param>
        /// <param name="lookback"></param>
        /// <returns></returns>
        bool CrossedUnder(decimal seriesB, int lookback = 3);

        /// <summary>
        /// Returns the maximum value between the current value and (lookback) values ago
        /// </summary>
        /// <param name="lookback"></param>
        /// <returns></returns>
        decimal Max(int lookback);

        /// <summary>
        /// Returns the minimum value between the current value and (lookback) values ago
        /// </summary>
        /// <param name="lookback"></param>
        /// <returns></returns>
        decimal Min(int lookback);

        /// <summary>
        /// Returns the sum of all values between the current value and (lookback) values ago
        /// </summary>
        /// <param name="lookback"></param>
        /// <returns></returns>
        decimal Sum(int lookback);

        #endregion Public Methods
    }
}