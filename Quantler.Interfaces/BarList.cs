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

using System.Collections;

namespace Quantler.Interfaces
{
    public interface BarList
    {
        #region Public Events

        /// <summary>
        /// called when new bar is created. this happens after first tick in the new bar. last full
        /// bar will have index -1
        /// </summary>
        event SymBarIntervalDelegate GotNewBar;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// count of all bars
        /// </summary>
        int Count { get; }

        /// <summary>
        /// gets list of all standard and custom intervals available on the bar
        /// </summary>
        int[] CustomIntervals { get; }

        /// <summary>
        /// default interval for bar when getting bar data (in interval units)
        /// </summary>
        int DefaultCustomInterval { get; set; }

        /// <summary>
        /// default interval for bar when getting bar data (in BarIntervals)
        /// </summary>
        BarInterval DefaultInterval { get; set; }

        // default interval
        /// <summary>
        /// default interval pair's (type/size) index
        /// </summary>
        int DefaultIntervalIndex { get; set; }

        bool DoubleIncludesRecent { get; set; }

        /// <summary>
        /// index of first bar
        /// </summary>
        int First { get; }

        /// <summary>
        /// gets a list of intervals available on the bar.
        /// </summary>
        BarInterval[] Intervals { get; }

        /// <summary>
        /// true if bar has symbol and some data
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// index of last bar
        /// </summary>
        int Last { get; }

        /// <summary>
        /// most recently occuring bar
        /// </summary>
        Bar RecentBar { get; }

        /// <summary>
        /// symbol bar represents
        /// </summary>
        string Symbol { get; }

        #endregion Public Properties

        #region Public Indexers

        /// <summary>
        /// get a bar from list using it's index. index = 0 is oldest bar. index = Last is newest
        /// bar. index = -1 is one bar back. index = -5 is 5 bars back
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Bar this[int index] { get; }

        /// <summary>
        /// get a bar from list using it's index and interval index = 0 is oldest bar. index = Last
        /// is newest bar. index = -1 is one bar back. index = -5 is 5 bars back
        /// </summary>
        /// <param name="index"></param>
        /// <param name="interval"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Bar this[int index, BarInterval interval, int size] { get; }

        /// <summary>
        /// gets a specific bar in specified seconds interval
        /// </summary>
        /// <param name="barnumber"></param>
        /// <param name="intervalidx"></param>
        /// <returns></returns>
        Bar this[int barnumber, int intervalidx] { get; }

        #endregion Public Indexers

        #region Public Methods

        /// <summary>
        /// gets array of closing prices for all bars in list, ordered by their index.
        /// </summary>
        /// <returns></returns>
        decimal[] Close();

        /// <summary>
        /// gets array of dates for all bars in list, ordered by their index.
        /// </summary>
        /// <returns></returns>
        int[] Date();

        IEnumerator GetEnumerator();

        /// <summary>
        /// check for minimum amount of bars, if they are present returns true
        /// </summary>
        /// <param name="minBars"></param>
        /// <param name="interval"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        bool Has(int minBars, BarInterval interval, int size);

        /// <summary>
        /// returns true if minimum # of bars present in interval
        /// </summary>
        /// <param name="minBars"></param>
        /// <returns></returns>
        bool Has(int minBars);

        /// <summary>
        /// gets array of high prices for all bars in list, ordered by their index.
        /// </summary>
        /// <returns></returns>
        decimal[] High();

        /// <summary>
        /// gets count of bars for a specific interval
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        int IntervalCount(BarInterval interval, int size);

        /// <summary>
        /// gets count of bars for a specific interval
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        int IntervalCount(BarInterval type);

        /// <summary>
        /// gets array of low prices for all bars in list, ordered by their index.
        /// </summary>
        /// <returns></returns>
        decimal[] Low();

        /// <summary>
        /// gets array of opening prices for all bars in list, ordered by their index.
        /// </summary>
        /// <returns></returns>
        decimal[] Open();

        /// <summary>
        /// set default interval as a pair
        /// </summary>
        /// <param name="type"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        bool SetDefaultInterval(BarInterval type, int size);

        /// <summary>
        /// gets array of times for all bars in list, ordered by their index.
        /// </summary>
        /// <returns></returns>
        int[] Time();

        /// <summary>
        /// gets array of volumes for all bars in list, ordered by their index.
        /// </summary>
        /// <returns></returns>
        long[] Vol();

        #endregion Public Methods
    }
}