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

using System.Collections.Generic;

namespace Quantler.Interfaces
{
    public interface IntervalData
    {
        #region Public Events

        /// <summary>
        /// Executed on each new bar created
        /// </summary>
        event SymBarIntervalDelegate NewBar;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Current size before creating a new bar
        /// </summary>
        int IntSize { get; }

        /// <summary>
        /// Interval type as BarInterval, fixed at custom
        /// </summary>
        int IntType { get; }

        /// <summary>
        /// Maximum amount of datapoints to store as history
        /// </summary>
        int MaxHistory { get; set; }

        /// <summary>
        /// When using tick data, specify to use the Ask, Bid or MidPoint price for creating bars
        /// </summary>
        BarPriceType PriceType { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Add a new bar to the price data
        /// </summary>
        /// <param name="b"></param>
        void AddBar(Bar b);

        /// <summary>
        /// Returns all close prices known in this interval
        /// </summary>
        /// <returns></returns>
        List<decimal> Close();

        /// <summary>
        /// Amount of bars created
        /// </summary>
        /// <returns></returns>
        int Count();

        /// <summary>
        /// Returns all dates known in this interval
        /// </summary>
        /// <returns></returns>
        List<int> Date();

        /// <summary>
        /// Get a specific bar by index and symbol
        /// </summary>
        /// <param name="index"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        Bar GetBar(int index, string symbol);

        /// <summary>
        /// Get the current bar by symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        Bar GetBar(string symbol);

        /// <summary>
        /// Returns all high prices known in this interval
        /// </summary>
        /// <returns></returns>
        List<decimal> High();

        /// <summary>
        /// True, when the latest bar created is a new bar currently not yet closed
        /// </summary>
        /// <returns></returns>
        bool IsRecentNew();

        /// <summary>
        /// Returns the index of the last created bar
        /// </summary>
        /// <returns></returns>
        int Last();

        /// <summary>
        /// Returns all low prices known in this interval
        /// </summary>
        /// <returns></returns>
        List<decimal> Low();

        /// <summary>
        /// Add a new data point
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="p"></param>
        /// <param name="time"></param>
        /// <param name="date"></param>
        /// <param name="size"></param>
        void NewPoint(string symbol, decimal p, int time, int date, int size);

        /// <summary>
        /// Add a new tick
        /// </summary>
        /// <param name="k"></param>
        void NewTick(Tick k);

        /// <summary>
        /// Returns all open prices known in this interval
        /// </summary>
        /// <returns></returns>
        List<decimal> Open();

        /// <summary>
        /// Reset this interval and erase all currently cached data
        /// </summary>
        void Reset();

        /// <summary>
        /// Returns all time moments known in this interval
        /// </summary>
        /// <returns></returns>
        List<int> Time();

        /// <summary>
        /// Returns all volumes known in this interval
        /// </summary>
        /// <returns></returns>
        List<long> Vol();

        #endregion Public Methods
    }
}