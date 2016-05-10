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

using System.Collections.Generic;

namespace Quantler.Interfaces
{
    /// <summary>
    /// Portfolio information holder
    /// </summary>
    public interface IPortfolio
    {
        #region Public Properties

        /// <summary>
        /// Associated account
        /// </summary>
        IAccount Account { get; }

        /// <summary>
        /// Array of all associated agents
        /// </summary>
        ITradingAgent[] Agents { get; }

        /// <summary>
        /// Unique identifier for this portfolio of agents
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Check if this portfolio object is valid for use, or not
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// List of all currently pending orders from all trading agents, present at the broker
        /// </summary>
        PendingOrder[] PendingOrders { get; }

        /// <summary>
        /// Current opened positions and its status for this portfolio
        /// </summary>
        IPositionTracker Positions { get; }

        /// <summary>
        /// Updated results of this trading session, for this portfolio
        /// </summary>
        Result Results { get; }

        /// <summary>
        /// All subscribed securities
        /// </summary>
        ISecurityTracker Securities { get; }

        /// <summary>
        /// Currently associated datastreams (live or backtest based)
        /// </summary>
        Dictionary<string, DataStream> Streams { get; }

        #endregion Public Properties
    }
}