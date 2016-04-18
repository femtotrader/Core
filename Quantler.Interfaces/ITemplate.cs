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
    /// <summary>
    /// Base implementation of a template
    /// </summary>
    public interface ITemplate
    {
        #region Public Properties

        /// <summary>
        /// Associated agent for this template
        /// </summary>
        ITradingAgent Agent { get; set; }

        /// <summary>
        /// Get any BarList available
        /// </summary>
        BarIndexer Bars { get; }

        /// <summary>
        /// Get the current bar information
        /// </summary>
        IReadOnlyDictionary<string, Bar> CurrentBar { get; }

        /// <summary>
        /// Get the current tick information
        /// </summary>
        IReadOnlyDictionary<string, Tick> CurrentTick { get; }

        /// <summary>
        /// Template ID
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Used for creating standard built-in indicators
        /// </summary>
        IndicatorFactory Indicators { get; }

        /// <summary>
        /// Name of this template
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns the associated portfolio
        /// </summary>
        IPortfolio Portfolio { get; }

        #endregion Public Properties

        /// <summary>
        /// Add another stream, without creating bars
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        void AddStream(SecurityType type, string name);

        /// <summary>
        /// Add another datastream, with a bar interval
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="interval"></param>
        void AddStream(SecurityType type, string name, TimeSpan interval);

        /// <summary>
        /// Add another datastream, with a bar interval
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="interval"></param>
        void AddStream(SecurityType type, string name, int interval);

        /// <summary>
        /// Add another datastream, with a bar interval
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="interval"></param>
        void AddStream(SecurityType type, string name, BarInterval interval);

        /// <summary>
        /// Create a new pending order
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="direction"></param>
        /// <param name="quantity"></param>
        /// <param name="limitPrice"></param>
        /// <param name="stopPrice"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        PendingOrder CreateOrder(string symbol, Direction direction, decimal quantity, decimal limitPrice, decimal stopPrice, string comment = "");

        /// <summary>
        /// Initialize this template, only called once
        /// </summary>
        void Initialize();

        /// <summary>
        /// Create and send a LimitOrder
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="direction"></param>
        /// <param name="quantity"></param>
        /// <param name="limitPrice"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        PendingOrder LimitOrder(string symbol, Direction direction, double quantity, double limitPrice, string comment = "");

        /// <summary>
        /// Create and send a MarketOrder
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="direction"></param>
        /// <param name="quantity"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        PendingOrder MarketOrder(string symbol, Direction direction, double quantity, string comment = "");

        /// <summary>
        /// Create and send a StopLimitpOrder
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="direction"></param>
        /// <param name="quantity"></param>
        /// <param name="limitPrice"></param>
        /// <param name="stopPrice"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        PendingOrder StopLimitOrder(string symbol, Direction direction, double quantity, double limitPrice, double stopPrice, string comment = "");

        /// <summary>
        /// Create and send a StopOrder
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="direction"></param>
        /// <param name="quantity"></param>
        /// <param name="stopPrice"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        PendingOrder StopOrder(string symbol, Direction direction, double quantity, double stopPrice, string comment = "");

        /// <summary>
        /// Send order to destined broker
        /// </summary>
        /// <param name="pendingorder"></param>
        /// <returns></returns>
        StatusType SubmitOrder(PendingOrder pendingorder);

        /// <summary>
        /// Update chart information
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        void UpdateChart(string name, ChartType type, decimal value);

        /// <summary>
        /// Update chart information
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        void UpdateChart(string name, ChartType type, double value);

        /// <summary>
        /// Update chart information
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        void UpdateChart(string name, ChartType type, int value);

        /// <summary>
        /// Update chart information
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        void UpdateChart(string name, ChartType type, float value);
    }
}