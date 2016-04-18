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
    /// Current account interface, holds all account information
    /// </summary>
    public interface IAccount
    {
        #region Public Properties

        /// <summary>
        /// Current account balance
        /// </summary>
        decimal Balance { get; }

        /// <summary>
        /// Client name shown on the brokers account
        /// </summary>
        string Client { get; }

        /// <summary>
        /// Company name of the broker used by this account
        /// </summary>
        string Company { get; }

        /// <summary>
        /// Base currency used denominating the account values
        /// </summary>
        CurrencyType Currency { get; }

        /// <summary>
        /// Current account equity
        /// </summary>
        decimal Equity { get; }

        /// <summary>
        /// Total PnL based on the current positions
        /// </summary>
        decimal FloatingPnL { get; }

        /// <summary>
        /// Free margin left to trade
        /// </summary>
        decimal FreeMargin { get; }

        /// <summary>
        /// Current account id
        /// </summary>
        string Id { get; }

        /// <summary>
        /// True if this account is used for live trading (real money account)
        /// </summary>
        bool IsLiveTrading { get; }

        /// <summary>
        /// True if this account is allowed to initiate trades
        /// </summary>
        bool IsTradingAllowed { get; }

        /// <summary>
        /// Latency from this server measured in Milliseconds
        /// </summary>
        int Latency { get; }

        /// <summary>
        /// Leverage used 100 == 1:00
        /// </summary>
        int Leverage { get; }

        /// <summary>
        /// Current margin
        /// </summary>
        decimal Margin { get; }

        /// <summary>
        /// Ratio between free margin and account equity
        /// </summary>
        decimal MarginLevel { get; }

        /// <summary>
        /// Currently held positions on this account
        /// </summary>
        IPositionTracker Positions { get; }

        /// <summary>
        /// Securities associated and thus tradeable on this account.
        /// If a security is not available in this collection, it cannot be traded.
        /// </summary>
        ISecurityTracker Securities { get; }

        /// <summary>
        /// Name of the server
        /// </summary>
        string Server { get; }

        /// <summary>
        /// Returns the stop out level for this account
        /// </summary>
        decimal StopOutLevel { get; }

        #endregion Public Properties
    }
}