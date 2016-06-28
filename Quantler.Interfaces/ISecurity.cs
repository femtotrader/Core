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
    /// <summary>
    /// security definition
    /// </summary>
    public interface ISecurity
    {
        #region Public Properties

        /// <summary>
        /// Get the latest ask price
        /// </summary>
        decimal Ask { get; }

        /// <summary>
        /// Get the latest bid price
        /// </summary>
        decimal Bid { get; }

        /// <summary>
        /// Name of this security at the broker
        /// </summary>
        string BrokerName { get; }

        /// <summary>
        /// exchange associated with security
        /// </summary>
        string DestEx { get; }

        /// <summary>
        /// details associated with security
        /// </summary>
        string Details { get; }

        /// <summary>
        /// Current security amount of digits
        /// </summary>
        int Digits { get; }

        /// <summary>
        /// whether security has an exchange
        /// </summary>
        bool HasDest { get; }

        /// <summary>
        /// whether security has an explicit type
        /// </summary>
        bool HasType { get; }

        /// <summary>
        /// Floating or fixed spreads
        /// </summary>
        bool IsFloatingSpread { get; }

        /// <summary>
        /// whether security is valid
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Last time this security received data
        /// </summary>
        DateTime LastTickEvent { get; }

        /// <summary>
        /// Returns the size of a full lot
        /// </summary>
        int LotSize { get; }

        /// <summary>
        /// symbol name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns the minimum quantity for an order (0.01 for microlots and 0.10 for mini lots)
        /// </summary>
        decimal OrderMinQuantity { get; }

        /// <summary>
        /// Returns the maximum quantity for an order (0.01 for microlots and 0.10 for mini lots)
        /// </summary>
        decimal OrderMaxQuantity { get; }

        /// <summary>
        /// Returns the minimum size for an order
        /// </summary>
        int OrderMinSize { get; }

        /// <summary>
        /// Returns the maximum size for an order
        /// </summary>
        int OrderMaxSize { get; }

        /// <summary>
        /// Returns the step quantity (0.01 for microlots and 0.10 for mini lots)
        /// </summary>
        decimal OrderStepQuantity { get; }

        /// <summary>
        /// Returns the step size (1.000 for microlots and 10.000 for mini lots)
        /// </summary>
        int OrderStepSize { get; }

        /// <summary>
        /// Returns the size of the price for one pip
        /// </summary>
        decimal PipSize { get; }

        /// <summary>
        /// Current Pip Value using the current account currency
        /// </summary>
        decimal PipValue { get; }

        /// <summary>
        /// Current spread in pips
        /// </summary>
        int Spread { get; }

        /// <summary>
        /// Returns the mimimum change in price for this security
        /// </summary>
        decimal TickSize { get; }

        /// <summary>
        /// type associated with security
        /// </summary>
        SecurityType Type { get; }

        #endregion Public Properties
    }
}