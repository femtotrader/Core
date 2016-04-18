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
    public interface Position
    {
        #region Public Properties

        /// <summary>
        /// Associated account for this position
        /// </summary>
        IAccount Account { get; }

        /// <summary>
        /// Average price
        /// </summary>
        decimal AvgPrice { get; }

        /// <summary>
        /// Current position direction information
        /// </summary>
        Direction Direction { get; }

        /// <summary>
        /// Quantity needed for flattening this position (either negative or positive)
        /// </summary>
        decimal FlatQuantity { get; }

        /// <summary>
        /// Size needed for flattening this position (either negative or positive)
        /// </summary>
        int FlatSize { get; }

        /// <summary>
        /// Gross PnL
        /// </summary>
        decimal GrossPnL { get; }

        /// <summary>
        /// True if the current position is flat
        /// </summary>
        bool IsFlat { get; }

        /// <summary>
        /// True if the current position is long
        /// </summary>
        bool IsLong { get; }

        /// <summary>
        /// True if the current position is short
        /// </summary>
        bool IsShort { get; }

        /// <summary>
        /// True if this position is valid and no inconsistencies have occured
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Last time and date this position has been modified
        /// </summary>
        DateTime LastModified { get; }

        /// <summary>
        /// Net PnL (GrossPnL - Commissions - Swaps)
        /// </summary>
        decimal NetPnL { get; }

        /// <summary>
        /// Current position quantity (negative or positive)
        /// </summary>
        decimal Quantity { get; }

        /// <summary>
        /// Security on which this position is placed
        /// </summary>
        ISecurity Security { get; }

        /// <summary>
        /// Current position size
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Total amount of commissions paid for this position
        /// </summary>
        decimal TotalCommission { get; }

        /// <summary>
        /// Total amount of swap paid for this position
        /// </summary>
        decimal TotalSwap { get; }

        /// <summary>
        /// Trades that make up the current position
        /// </summary>
        Trade[] Trades { get; }

        /// <summary>
        /// Current position size (absolute, always positive)
        /// </summary>
        int UnsignedSize { get; }

        #endregion Public Properties

        #region Public Methods

        Trade ToTrade();

        #endregion Public Methods
    }

    public class InvalidPosition : Exception { }
}