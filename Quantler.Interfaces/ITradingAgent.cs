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
    /// Base implementation of a trading agent
    /// </summary>
    public interface ITradingAgent
    {
        #region Public Events

        /// <summary>
        /// Chart updates send by a template within an agent
        /// </summary>
        event ChartUpdate OnChartUpdate;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Agent identifier
        /// </summary>
        int AgentId { get; set; }

        /// <summary>
        /// Get any BarList available
        /// </summary>
        BarIndexer Bars { get; }

        /// <summary>
        /// Get latest closed bars based on symbol name
        /// </summary>
        Dictionary<string, Bar> CurrentBar { get; }

        /// <summary>
        /// Get current agent state per symbol
        /// </summary>
        Dictionary<string, List<AgentState>> CurrentState { get; }

        /// <summary>
        /// Get latest ticks occured based on symbol name
        /// </summary>
        Dictionary<string, Tick> CurrentTick { get; }

        /// <summary>
        /// True if this agent is currently being used for backtesting
        /// </summary>
        bool IsBacktesting { get; }

        /// <summary>
        /// True if this agent is running and processing data
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Name of this agent
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns known pending orders for this agent
        /// </summary>
        PendingOrder[] PendingOrders { get; }

        //Settings
        /// <summary>
        /// Returns the portfolio for which this agent is part of
        /// </summary>
        IPortfolio Portfolio { get; }

        /// <summary>
        /// Current positions hold by this agent
        /// </summary>
        IPositionTracker Positions { get; }

        /// <summary>
        /// Current trading results calculated for this agent (isolated)
        /// </summary>
        Result Results { get; }

        /// <summary>
        /// Default security used by this agent
        /// </summary>
        ISecurity Security { get; }

        /// <summary>
        /// Create standard built in indicators
        /// </summary>
        IndicatorFactory StandardIndicators { get; }

        /// <summary>
        /// Date and time on which this trading agent was set to start
        /// </summary>
        DateTime StartedDTUTC { get; }

        /// <summary>
        /// Returns all statistic templates used by this agent
        /// </summary>
        ITemplate[] Statistics { get; }

        /// <summary>
        /// Default datastream for this agent
        /// </summary>
        DataStream Stream { get; }

        /// <summary>
        /// Default symbol for this agent
        /// </summary>
        string Symbol { get; }

        /// <summary>
        /// Returns all templates used by this agent
        /// </summary>
        ITemplate[] Templates { get; }

        /// <summary>
        /// Default timeframe for this agent
        /// </summary>
        TimeSpan TimeFrame { get; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Add a new datastream
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        void AddDataStream(SecurityType type, string name);

        /// <summary>
        /// Add a new datastream, using timespan as interval
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="interval"></param>
        void AddDataStream(SecurityType type, string name, TimeSpan interval);

        /// <summary>
        /// Add a new datastream, using an integer as interval
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="interval"></param>
        void AddDataStream(SecurityType type, string name, int interval);

        /// <summary>
        /// Add a new datastream, using a barinterval as interval
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="interval"></param>
        void AddDataStream(SecurityType type, string name, BarInterval interval);

        /// <summary>
        /// Sends a chart update to all subscribed events if applicable
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        void ChartUpdate(ITemplate template, string name, ChartType type, decimal value);

        /// <summary>
        /// Create a new order and have it processed by the risk management agent and money management agents
        /// </summary>
        /// <param name="security"></param>
        /// <param name="direction"></param>
        /// <param name="quantity"></param>
        /// <param name="limitPrice"></param>
        /// <param name="stopPrice"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        PendingOrder CreateOrder(ISecurity security, Direction direction, decimal quantity, decimal limitPrice, decimal stopPrice, string comment = "");

        /// <summary>
        /// Create a new order and have it processed by the risk management agent and money management agents
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="direction"></param>
        /// <param name="quantity"></param>
        /// <param name="limitPrice"></param>
        /// <param name="stopPrice"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        PendingOrder CreateOrder(string symbol, Direction direction, decimal quantity, decimal limitPrice, decimal stopPrice, string comment = "");

        void Deinitialize();

        //Quantler Model
        void Entry();

        void Exit();

        /// <summary>
        /// Flatten the current agents positions
        /// </summary>
        void Flatten();

        void Initialize();

        void Log(LogSeverity severity, string message, params object[] args);

        void MoneyManagement(PendingOrder order);

        void OnBar(Bar bar);

        void OnFill(Trade trade, PendingOrder order);

        void OnOrder(PendingOrder order);

        void OnOrderUpdate(PendingOrder order);

        //Events
        void OnTick(Tick tick);

        void RiskManagement(PendingOrder order);

        /// <summary>
        /// Set the name of this agent
        /// </summary>
        /// <param name="name"></param>
        void SetName(string name);

        /// <summary>
        /// Start the current agent and allow it to process data
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the current agent from running
        /// </summary>
        void Stop();

        /// <summary>
        /// Send pending order to its destined broker
        /// </summary>
        /// <param name="pendingorder"></param>
        /// <returns></returns>
        StatusType SubmitOrder(PendingOrder pendingorder);

        #endregion Public Methods
    }
}