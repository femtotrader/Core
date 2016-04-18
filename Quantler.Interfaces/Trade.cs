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
    public interface Trade
    {
        #region Public Properties

        /// <summary>
        /// Account reference on which this order fill occured
        /// </summary>
        IAccount Account { get; }

        /// <summary>
        /// account trade occured in
        /// </summary>
        string AccountName { get; }

        /// <summary>
        /// AgentID for the agent on which this trade was executed on
        /// </summary>
        int AgentId { get; }

        /// <summary>
        /// Broker symbol
        /// </summary>
        string BrokerSymbol { get; }

        /// <summary>
        /// Commissions known from execution of this order
        /// </summary>
        decimal Commission { get; }

        /// <summary>
        /// currency trade occured in
        /// </summary>
        CurrencyType Currency { get; }

        /// <summary>
        /// Direction of the trade, either long or short
        /// </summary>
        Direction Direction { get; }

        /// <summary>
        /// exchange/destination where trade occured
        /// </summary>
        string Exchange { get; }

        /// <summary>
        /// Executed date and time
        /// </summary>
        DateTime Executed { get; }

        /// <summary>
        /// id of trade
        /// </summary>
        long Id { get; }

        /// <summary>
        /// whether trade has been filled
        /// </summary>
        bool IsFilled { get; }

        /// <summary>
        /// whether trade is valid
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// full security information for trade
        /// </summary>
        ISecurity Security { get; }

        /// <summary>
        /// Swap paid for this trade
        /// </summary>
        decimal Swap { get; }

        /// <summary>
        /// symbol traded
        /// </summary>
        string Symbol { get; }

        /// <summary>
        /// unsigned size of trade
        /// </summary>
        int UnsignedSize { get; }

        /// <summary>
        /// executed date
        /// </summary>
        int Xdate { get; }

        /// <summary>
        /// executed price
        /// </summary>
        decimal Xprice { get; }

        /// <summary>
        /// executed quantity
        /// </summary>
        decimal Xquantity { get; }

        /// <summary>
        /// executed size
        /// </summary>
        int Xsize { get; }

        /// <summary>
        /// executed time
        /// </summary>
        int Xtime { get; }

        #endregion Public Properties
    }
}