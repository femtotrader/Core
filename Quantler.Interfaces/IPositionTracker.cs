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
using System.Collections.Generic;

namespace Quantler.Interfaces
{
    public interface IPositionTracker : GotPositionIndicator, GotFillIndicator, IEnumerable<Position>
    {
        #region Public Events

        /// <summary>
        /// Executed on the event that a position has been changed
        /// </summary>
        event PositionUpdateDelegate OnPositionUpdate;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Default account on which the positions are hold at
        /// </summary>
        IAccount DefaultAccount { get; set; }

        /// <summary>
        /// Total closed pnl amount of the current trading session
        /// </summary>
        decimal TotalClosedPL { get; }

        /// <summary>
        /// Tracked type of positions
        /// </summary>
        Type TrackedType { get; }

        /// <summary>
        /// All securities used for tracking the positions
        /// </summary>
        ISecurity[] Securities { get; }

        #endregion Public Properties

        #region Public Indexers

        /// <summary>
        /// Get current position information by index
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        Position this[int idx] { get; }

        /// <summary>
        /// Get current position information by symbol and specified account
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        Position this[string symbol, IAccount account] { get; }

        /// <summary>
        /// Get current position information by symbol and default account
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        Position this[string symbol] { get; }

        /// <summary>
        /// Get current position information by security and specified account
        /// </summary>
        /// <param name="security"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        Position this[ISecurity security, IAccount account] { get; }

        /// <summary>
        /// Get current position information by security and default account
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        Position this[ISecurity security] { get; }

        #endregion Public Indexers

        #region Public Methods

        /// <summary>
        /// Adjust current position based on a new position found
        /// </summary>
        /// <param name="newpos"></param>
        /// <returns></returns>
        decimal Adjust(Position newpos);

        /// <summary>
        /// Adjust current position based on a new trade
        /// </summary>
        /// <param name="fill"></param>
        /// <returns></returns>
        decimal Adjust(Trade fill);

        /// <summary>
        /// Adjust current position based on a new trade and the position index
        /// </summary>
        /// <param name="fill"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        decimal Adjust(Trade fill, int idx);

        /// <summary>
        /// Clear all position information (all positions are removed, not closed)
        /// </summary>
        void Clear();

        /// <summary>
        /// Manually add a new position
        /// </summary>
        /// <param name="newpos"></param>
        void NewPosition(Position newpos);

        #endregion Public Methods
    }
}