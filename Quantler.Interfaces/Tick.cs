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
    public interface Tick
    {
        #region Public Properties

        /// <summary>
        /// offer price
        /// </summary>
        decimal Ask { get; }

        /// <summary>
        /// offer exchange
        /// </summary>
        string AskExchange { get; }

        /// <summary>
        /// offer size
        /// </summary>
        int AskSize { get; }

        /// <summary>
        /// bid price
        /// </summary>
        decimal Bid { get; }

        /// <summary>
        /// bid exchange
        /// </summary>
        string BidExchange { get; }

        /// <summary>
        /// bid size
        /// </summary>
        int BidSize { get; }

        /// <summary>
        /// tick date (example: 20091231)
        /// </summary>
        int Date { get; }

        /// <summary>
        /// date and time represented as long, eg 8:05pm on 4th of July: 200907042005. this is not
        /// guaranteed to be set.
        /// </summary>
        long Datetime { get; }

        /// <summary>
        /// depth of last bid/ask quote
        /// </summary>
        int Depth { get; }

        /// <summary>
        /// trade exchange
        /// </summary>
        string Exchange { get; }

        /// <summary>
        /// True if this tick contains an ask price
        /// </summary>
        bool HasAsk { get; }

        /// <summary>
        /// True if this tick contains a bid price
        /// </summary>
        bool HasBid { get; }

        /// <summary>
        /// True if this tick is a complete quote (bid/ask)
        /// </summary>
        bool IsFullQuote { get; }

        /// <summary>
        /// True if this tick is an index price
        /// </summary>
        bool IsIndex { get; }

        /// <summary>
        /// True if this tick is of a quote (bid or ask)
        /// </summary>
        bool IsQuote { get; }

        /// <summary>
        /// True if this tick based on a trade occured
        /// </summary>
        bool IsTrade { get; }

        /// <summary>
        /// True if this tick contains valid information (no inconsistencies)
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// size of last trade
        /// </summary>
        int Size { get; }

        /// <summary>
        /// symbol for tick
        /// </summary>
        string Symbol { get; }

        /// <summary>
        /// index of symbol associated with this tick. this is not guaranteed to be set
        /// </summary>
        int Symidx { get; }

        /// <summary>
        /// Converted tik date and time into DateTime format
        /// </summary>
        DateTime TickDateTime { get; }

        /// <summary>
        /// tick time in HHmmssfff format (4:59:12.000pm == 165912000)
        /// </summary>
        int Time { get; }

        /// <summary>
        /// trade price
        /// </summary>
        decimal Trade { get; }

        #endregion Public Properties
    }

    public class InvalidTick : Exception { }
}