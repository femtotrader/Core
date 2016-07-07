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

using NLog;
using Quantler.Interfaces;
using Quantler.Templates;
using Quantler.Tracker;
using Quantler.Trades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Quantler.Agent
{
    public abstract partial class TradingAgent : TradingAgentManager
    {
        #region Protected Fields

        protected Reflection.InvokeFactory Exec = new Reflection.InvokeFactory();
        protected List<Reflection.InvokeLinkVoid<PendingOrder, AgentState>> InvokeMm = new List<Reflection.InvokeLinkVoid<PendingOrder, AgentState>>();
        protected List<Reflection.InvokeLinkFunc<PendingOrder, AgentState, PendingOrder>> InvokeRm = new List<Reflection.InvokeLinkFunc<PendingOrder, AgentState, PendingOrder>>();
        protected List<Reflection.InvokeLinkVoid> OnCalcEvents = new List<Reflection.InvokeLinkVoid>();

        #endregion Protected Fields

        #region Private Fields

        private readonly List<Reflection.InvokeLinkVoid<PendingOrder>> _invokeOnOrderUpdate = new List<Reflection.InvokeLinkVoid<PendingOrder>>();
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly ILogger _loggerUser = LogManager.GetLogger("User");
        private int _agentid = -1;
        private bool _initialized;
        private List<Reflection.InvokeLinkVoid<Bar>> _invokeOnBar = new List<Reflection.InvokeLinkVoid<Bar>>();
        private List<Reflection.InvokeLinkVoid<Trade, PendingOrder>> _invokeOnFill = new List<Reflection.InvokeLinkVoid<Trade, PendingOrder>>();
        private List<Reflection.InvokeLinkVoid<PendingOrder>> _invokeOnOrder = new List<Reflection.InvokeLinkVoid<PendingOrder>>();
        private List<Reflection.InvokeLinkVoid<Position>> _invokeOnPosition = new List<Reflection.InvokeLinkVoid<Position>>();
        private List<Reflection.InvokeLinkVoid<Tick>> _invokeOnTick = new List<Reflection.InvokeLinkVoid<Tick>>();
        private string _name = "TestAgent";
        private PortfolioManager _portfolio;
        private Results _results;
        private DateTime _started = DateTime.UtcNow;

        //Storage
        private List<ITemplate> _templates = new List<ITemplate>();

        #endregion Private Fields

        #region Public Events

        public event ChartUpdate OnChartUpdate;

        #endregion Public Events

        #region Public Properties

        public int AgentId
        {
            get { return _agentid; }
            set { if (_agentid < 0) _agentid = value; }
        }

        public BarIndexer Bars { get; private set; }

        /// <summary>
        /// Returns the current bar for the specific symbol
        /// </summary>
        public Dictionary<string, Bar> CurrentBar
        {
            get;
            private set;
        }

        /// <summary>
        /// Storage for the decisions made by the templates
        /// </summary>
        public Dictionary<string, List<AgentState>> CurrentState
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the current tick for the specific symbol
        /// </summary>
        public Dictionary<string, Tick> CurrentTick
        {
            get;
            private set;
        }

        public Interfaces.IndicatorManager IndicatorManagement { get; private set; }

        /// <summary>
        /// Checks if this agent is in a backtesting state or trading mode
        /// </summary>
        public bool IsBacktesting
        {
            get;
            set;
        }

        /// <summary>
        /// Is true if this agent is either running a backtest or is running a live trading strategy
        /// </summary>
        public bool IsRunning
        {
            get;
            set;
        }

        public string Name
        {
            get { return _name; }
        }

        public PendingOrder[] PendingOrders
        {
            get { return Portfolio.PendingOrders.Where(x => x.AgentId == AgentId).ToArray(); }
        }

        /// <summary>
        /// Associated portfolio
        /// </summary>
        public IPortfolio Portfolio
        {
            get
            {
                return _portfolio;
            }
        }

        public IPositionTracker Positions { get; private set; }

        public Result Results
        {
            get { return _results; }
        }

        /// <summary>
        /// Default security object associated to this agent
        /// </summary>
        public ISecurity Security
        {
            get { return Stream.Security; }
        }

        public IndicatorFactory StandardIndicators { get; private set; }

        public DateTime StartedDTUTC
        {
            get { return _started; }
        }

        /// <summary>
        /// Return the currently associated statistic templates
        /// </summary>
        public ITemplate[] Statistics
        {
            get { return Templates.Where(x => x is StatisticTemplate).ToArray(); }
        }

        /// <summary>
        /// Default DataStream associated to this agent
        /// </summary>
        public DataStream Stream
        {
            get;
            set;
        }

        /// <summary>
        /// Default symbol associated to this agent
        /// </summary>
        public string Symbol
        {
            get { return Stream.Security.Name; }
        }

        /// <summary>
        /// Collection of all templates associated to this agent
        /// </summary>
        public ITemplate[] Templates
        {
            get
            {
                return _templates.ToArray();
            }
            set
            {
                if (!IsRunning)
                    _templates = value.ToList();
            }
        }

        /// <summary>
        /// Default timeframe of this agent in timespan notation
        /// </summary>
        public TimeSpan TimeFrame
        {
            get;
            set;
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Add a new sample to this agent for processing (can only be done if the agent is not already running)
        /// </summary>
        /// <param name="stream"></param>
        public void AddDataStream(DataStream stream)
        {
            _logger.Debug("AddDataStream: Processing request for symbol {0} and timeframe {1}", stream.Security.Name, stream.DefaultInterval);
            if (!IsRunning)
            {
                _portfolio.AddStream(stream);
            }
            else
                _logger.Debug("AddDataStream: Could not add datastream, agent is already running.");
        }

        public void AddDataStream(SecurityType type, string name)
        {
            AddDataStream(new OHLCBarStream(Portfolio.Securities[name, type]));
        }

        public void AddDataStream(SecurityType type, string name, TimeSpan interval)
        {
            AddDataStream(new OHLCBarStream(Portfolio.Securities[name, type], (int)interval.TotalSeconds));
        }

        public void AddDataStream(SecurityType type, string name, int interval)
        {
            AddDataStream(new OHLCBarStream(Portfolio.Securities[name, type], interval));
        }

        public void AddDataStream(SecurityType type, string name, BarInterval interval)
        {
            AddDataStream(new OHLCBarStream(Portfolio.Securities[name, type], (int)interval));
        }

        public void AddEvent(object template)
        {
            Type baseType = template.GetType().BaseType;
            DataStream[] streams = new DataStream[0];
            if (baseType == typeof(IndicatorTemplate) || baseType == typeof(Indicators.IndicatorBase))
                streams = ((Indicator)template).DataStreams;

            //Search for OnBars
            var found = SearchTemplateMethod(template.GetType(), "OnBar", typeof(Bar));
            if (found != null)
            {
                var referencedTemplate = Expression.Constant(template);
                var parameter = Expression.Parameter(typeof(Bar), found.GetParameters()[0].Name);
                var call = Expression.Call(referencedTemplate, found, parameter);

                _invokeOnBar.Add(new Reflection.InvokeLinkVoid<Bar>()
                    {
                        Action = Expression.Lambda<Action<Bar>>(call, parameter).Compile(),
                        BaseType = baseType,
                        ParmType = typeof(Bar),
                        DataStreams = streams
                    });
            }

            //Search for OnOrders
            found = SearchTemplateMethod(template.GetType(), "OnOrder", typeof(PendingOrder));
            if (found != null)
            {
                var referencedTemplate = Expression.Constant(template);
                var parameter = Expression.Parameter(typeof(PendingOrder), found.GetParameters()[0].Name);
                var call = Expression.Call(referencedTemplate, found, parameter);
                _invokeOnOrder.Add(new Reflection.InvokeLinkVoid<PendingOrder>()
                {
                    Action = Expression.Lambda<Action<PendingOrder>>(call, parameter).Compile(),
                    BaseType = baseType,
                    ParmType = typeof(PendingOrder),
                    DataStreams = streams
                });
            }

            //Search for OnOrderUpdate
            found = SearchTemplateMethod(template.GetType(), "OnOrderUpdate", typeof(PendingOrder));
            if (found != null)
            {
                var referencedTemplate = Expression.Constant(template);
                var parameter = Expression.Parameter(typeof(PendingOrder), found.GetParameters()[0].Name);
                var call = Expression.Call(referencedTemplate, found, parameter);
                _invokeOnOrderUpdate.Add(new Reflection.InvokeLinkVoid<PendingOrder>()
                {
                    Action = Expression.Lambda<Action<PendingOrder>>(call, parameter).Compile(),
                    BaseType = baseType,
                    ParmType = typeof(PendingOrder),
                    DataStreams = streams
                });
            }

            //Search for OnTicks
            found = SearchTemplateMethod(template.GetType(), "OnTick", typeof(Tick));
            if (found != null)
            {
                var referencedTemplate = Expression.Constant(template);
                var parameter = Expression.Parameter(typeof(Tick), found.GetParameters()[0].Name);
                var call = Expression.Call(referencedTemplate, found, parameter);
                _invokeOnTick.Add(new Reflection.InvokeLinkVoid<Tick>()
                {
                    Action = Expression.Lambda<Action<Tick>>(call, parameter).Compile(),
                    BaseType = baseType,
                    ParmType = typeof(Tick),
                    DataStreams = streams
                });
            }

            //Search for OnFills
            found = SearchTemplateMethod(template.GetType(), "OnFill", typeof(Trade), typeof(PendingOrder));
            if (found != null)
            {
                var referencedTemplate = Expression.Constant(template);
                var parameter = Expression.Parameter(typeof(Trade), found.GetParameters()[0].Name);
                var secondparameter = Expression.Parameter(typeof(PendingOrder), found.GetParameters()[1].Name);
                var call = Expression.Call(referencedTemplate, found, parameter, secondparameter);
                _invokeOnFill.Add(new Reflection.InvokeLinkVoid<Trade, PendingOrder>()
                {
                    Action = Expression.Lambda<Action<Trade, PendingOrder>>(call, parameter, secondparameter).Compile(),
                    BaseType = baseType,
                    ParmType = typeof(Trade),
                    DataStreams = streams
                });
            }

            //Search for OnCalculates
            found = template.GetType().GetMethod("OnCalculate");
            if (found != null)
            {
                var referencedTemplate = Expression.Constant(template);
                var call = Expression.Call(referencedTemplate, found);
                OnCalcEvents.Add(new Reflection.InvokeLinkVoid()
                {
                    Action = Expression.Lambda<Action>(call).Compile(),
                    BaseType = template.GetType().BaseType,
                    DataStreams = streams
                });
            }

            //Unique events
            if (template is RiskManagementTemplate)
            {
                found = template.GetType().GetMethod("RiskManagement");
                if (found != null)
                {
                    var referencedTemplate = Expression.Constant(template);
                    var parameter = Expression.Parameter(typeof(PendingOrder), found.GetParameters()[0].Name);
                    var secondparameter = Expression.Parameter(typeof(AgentState), found.GetParameters()[0].Name);
                    var call = Expression.Call(referencedTemplate, found, parameter, secondparameter);
                    InvokeRm.Add(new Reflection.InvokeLinkFunc<PendingOrder, AgentState, PendingOrder>()
                    {
                        Action = Expression.Lambda<Func<PendingOrder, AgentState, PendingOrder>>(call, parameter, secondparameter).Compile(),
                        BaseType = template.GetType().BaseType,
                        ParmType = typeof(PendingOrder),
                        ReturnType = typeof(PendingOrder)
                    });
                }
            }
            else if (template is MoneyManagementTemplate)
            {
                found = template.GetType().GetMethod("PositionSize");
                if (found != null)
                {
                    var referencedTemplate = Expression.Constant(template);
                    var parameter = Expression.Parameter(typeof(PendingOrder), found.GetParameters()[0].Name);
                    var secondparameter = Expression.Parameter(typeof(AgentState), found.GetParameters()[0].Name);
                    var call = Expression.Call(referencedTemplate, found, parameter, secondparameter);
                    InvokeMm.Add(new Reflection.InvokeLinkVoid<PendingOrder, AgentState>()
                    {
                        Action = Expression.Lambda<Action<PendingOrder, AgentState>>(call, parameter, secondparameter).Compile(),
                        BaseType = template.GetType().BaseType,
                        ParmType = typeof(PendingOrder)
                    });
                }
            }

            //Order priorities
            _invokeOnBar = _invokeOnBar.OrderByDescending(x => x.BaseType.GetInterfaces().Contains(typeof(Indicator))).ToList();
            _invokeOnFill = _invokeOnFill.OrderByDescending(x => x.BaseType.GetInterfaces().Contains(typeof(Indicator))).ToList();
            _invokeOnOrder = _invokeOnOrder.OrderByDescending(x => x.BaseType.GetInterfaces().Contains(typeof(Indicator))).ToList();
            _invokeOnPosition = _invokeOnPosition.OrderByDescending(x => x.BaseType.GetInterfaces().Contains(typeof(Indicator))).ToList();
            _invokeOnTick = _invokeOnTick.OrderByDescending(x => x.BaseType.GetInterfaces().Contains(typeof(Indicator))).ToList();
        }

        /// <summary>
        /// Add a new template to the existing templates (not needed when using the dep inj method)
        /// </summary>
        /// <param name="template"></param>
        public void AddTemplate(ITemplate template)
        {
            if (!IsRunning)
                _templates.Add(template);
        }

        public void ChartUpdate(ITemplate template, string name, ChartType type, decimal value)
        {
            if (OnChartUpdate != null)
                OnChartUpdate(this, template, name, value, type);
        }

        /// <summary>
        /// Close an existing current position
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public StatusType ClosePosition(Position pos)
        {
            return SubmitOrder(CreateOrder(pos.Security.Name, Direction.Flat, pos.Quantity));
        }

        public PendingOrder CreateOrder(ISecurity security, Direction direction, decimal quantity, decimal limitPrice = 0, decimal stopPrice = 0, string comment = "")
        {
            return CreateOrder(security.Name, direction, quantity, limitPrice, stopPrice, comment);
        }

        public PendingOrder CreateOrder(string symbol, Direction direction, decimal quantity, decimal limitPrice = 0, decimal stopPrice = 0, string comment = "")
        {
            return _portfolio.OrderFactory.CreateOrder(symbol, direction, quantity, limitPrice, stopPrice, comment, AgentId);
        }

        public void Deinitialize()
        {
            if (IsRunning)
                throw new Exception("Could not deinitialize agent if it is currently running, stop it from running first.");

            Stream.GotNewBar -= Stream_GotNewBar;
        }

        //Model functions
        public abstract void Entry();

        public abstract void Exit();

        public void Flatten()
        {
            LocalLog(LogLevel.Info, "Agent flatten signal received, flattening all positions.");
            foreach (var pos in Positions.Where(x => x.UnsignedSize > 0))
            {
                LocalLog(LogLevel.Debug, "Flattening position {0} with quantity {1}", pos.Security.Name, pos.Quantity);
                ClosePosition(pos);
            }
        }

        /// <summary>
        /// Initialize this agent, should be runned only once when booting up the agent
        /// </summary>
        public virtual void Initialize()
        {
            LocalLog(LogLevel.Debug, "Initializing agent with id {0}, symbol {1} and timeframe {2}", AgentId, Symbol, TimeFrame);
            //Check for previous initialization
            if (_initialized)
            {
                LocalLog(LogLevel.Warn, "Failed to initialize agent with id {0}, already initialized", AgentId);
                return;
            }

            //Set standard indicators
            IndicatorManagement = new IndicatorManager(this);
            StandardIndicators = new Indicators.StandardIndicators(IndicatorManagement);

            //Subscribe for new data entries
            foreach (var stream in Portfolio.Streams.Values)
                stream.GotNewBar += Stream_GotNewBar;

            //Associate agent
            _templates.ForEach(x => x.Agent = this);

            //init all templates
            _templates.ForEach(x => x.Initialize());

            //Set all events
            _templates.ForEach(AddEvent);

            //Set current decisions
            CurrentState = new Dictionary<string, List<AgentState>>();
            foreach (var stream in Portfolio.Streams)
                CurrentState.Add(stream.Key, new List<AgentState>());

            //Set empty objects
            CurrentBar = new Dictionary<string, Bar>();
            CurrentTick = new Dictionary<string, Tick>();
            Bars = new Data.Bars.BarIndexerImpl(_portfolio);
            _results = new Results(0, Portfolio.Account, AgentId);
            Positions = new PositionTracker(_portfolio.Account);

            //Set current initialization point
            _initialized = true;
            LocalLog(LogLevel.Debug, "Initializing agent with id {0}, symbol {1} and timeframe {2}. Succeeded!", AgentId, Symbol, TimeFrame);
        }

        public void Log(LogSeverity severity, string message, params object[] args)
        {
            LogEventInfo logEvent = new LogEventInfo(LogLevel.FromString(severity.ToString()), "User", string.Format(message, args));
            logEvent.Properties["AgentID"] = AgentId;
            logEvent.Properties["Occured"] = Security.LastTickEvent;
            _loggerUser.Log(logEvent);
        }

        public abstract void MoneyManagement(PendingOrder order);

        /// <summary>
        /// Execute the on bar event for each new bar to be processed by the associated templates
        /// </summary>
        /// <param name="bar"></param>
        public void OnBar(Bar bar)
        {
            LocalLog(LogLevel.Trace, "OnBar: Processing bar Symbol: {0}, Interval: {1}", bar.Symbol, bar.CustomInterval);

            //Execute all OnBar events (Indicators first)
            if (string.IsNullOrWhiteSpace(bar.Symbol)) return;
            DataStream stream = Portfolio.Streams[bar.Symbol];

            if (stream == null || stream.Security == null || stream.Security.Name != bar.Symbol)
                LocalLog(LogLevel.Error, "Could not find stream for symbol {0}", bar.Symbol);

            //Execute all OnBar events (Indicators)
            Exec.InvokeAll(_invokeOnBar, bar, stream, typeof(IndicatorTemplate), typeof(Indicators.IndicatorBase));

            //Execute all OnBar events (nonIndicators)
            Exec.InvokeAllExclude(_invokeOnBar, bar, typeof(IndicatorTemplate), typeof(Indicators.IndicatorBase));

            //check for main timeframe
            if (bar.CustomInterval != (int)TimeFrame.TotalSeconds)
            {
                LocalLog(LogLevel.Trace, "OnBar: Bar is discarded for agent calc events, expected interval: {0} but found interval: {1}", TimeFrame.TotalSeconds, bar.CustomInterval);
                return;
            }
            else
                LocalLog(LogLevel.Trace, "OnBar: Bar is processed for agent calc event, found interval: {0}", bar.CustomInterval);

            //Check all entry template logic
            ClearAgentSate();
            Exec.InvokeAll(OnCalcEvents, typeof(EntryTemplate));
            Entry();

            //Check all exit template logic
            ClearAgentSate();
            Exec.InvokeAll(OnCalcEvents, typeof(ExitTemplate));
            Exit();
        }

        public void OnFill(Trade fill, PendingOrder order)
        {
            //Debug logging
            LocalLog(LogLevel.Debug, "OnFill: filling order with fill {0} and order {1}", fill.Id, order.OrderId);

            //Process fill for current trading agent
            Positions.GotFill(fill);

            //Execute all OnFill events
            if (_invokeOnFill.Count > 0 && IsRunning)
                Exec.InvokeAll(_invokeOnFill, fill, order);
        }

        public void OnOrder(PendingOrder order)
        {
            //Debug logging
            LocalLog(LogLevel.Debug, "OnOrder: receiving order event for order with id {0}", order.OrderId);

            //Execute all OnOrder events
            if (_invokeOnOrder.Count > 0 && IsRunning)
                Exec.InvokeAll(_invokeOnOrder, order);
        }

        public void OnOrderUpdate(PendingOrder order)
        {
            //Debug logging
            LocalLog(LogLevel.Debug, "OnOrder: receiving order update event for order with id {0}", order.OrderId);

            if (_invokeOnOrderUpdate.Count > 0 && IsRunning)
                Exec.InvokeAll(_invokeOnOrderUpdate, order);
        }

        public void OnPosition(Position pos)
        {
            //Debug logging
            LocalLog(LogLevel.Debug, "OnOrder: receiving position update event for position with symbol {0}", pos.Security.Name);

            //Execute all OnPosition events
            if (_invokeOnPosition.Count > 0 && IsRunning)
                Exec.InvokeAll(_invokeOnPosition, pos);
        }

        /// <summary>
        /// Execute the on tick event for each new tick to be processed by the associated templates
        /// </summary>
        /// <param name="tick"></param>
        public void OnTick(Tick tick)
        {
            //Check tick
            if (string.IsNullOrWhiteSpace(tick.Symbol) || CurrentTick == null)
                return;

            //Set the current Tick
            CurrentTick[tick.Symbol] = tick;

            //Execute all OnBar events (Indicators first)
            if (_invokeOnTick.Count > 0 && IsRunning)
                Exec.InvokeAll(_invokeOnTick, tick, Portfolio.Streams[tick.Symbol]);
        }

        public abstract void RiskManagement(PendingOrder order);

        public void SetDefaultStream(DataStream stream)
        {
            LocalLog(LogLevel.Debug, "Setting new default stream Symbol = {0} current timeframe = {1}", stream.Security.Name, TimeFrame);
            Stream = stream;
            AddDataStream(stream);
        }

        public void SetName(string name)
        {
            LocalLog(LogLevel.Debug, "Setting agent name (was {0}) to {1}", _name, name);
            _name = name;
        }

        public void SetPortfolio(PortfolioManager portfolio)
        {
            LocalLog(LogLevel.Debug, "Setting agent portfolio to portfolio with id {0}", portfolio.Id);
            if (_portfolio == null)
                _portfolio = portfolio;
            else
                LocalLog(LogLevel.Warn, "Could not set new portfolio (with id {0}) to agent, portfolio was already set to {1}", portfolio.Id, Portfolio.Id);
        }

        public void SetPositionsTracker(IPositionTracker postracker)
        {
            LocalLog(LogLevel.Debug, "Setting position tracker to type of {0}", postracker.GetType().Name);
            Positions = postracker;
        }

        public void Start()
        {
            LocalLog(LogLevel.Info, "Agent start signal received, starting agent and allowing it to process data.");
            IsRunning = true;
            _started = DateTime.UtcNow;
        }

        public void Stop()
        {
            LocalLog(LogLevel.Info, "Agent stop signal received, stopping agent from processing data.");
            IsRunning = false;
        }

        /// <summary>
        /// Submit a new order to the portfolio
        /// </summary>
        /// <param name="pendingorder"></param>
        /// <returns></returns>
        public StatusType SubmitOrder(PendingOrder pendingorder)
        {
            //Debug logging
            LocalLog(LogLevel.Debug, "SubmitOrder: submitting pending order for symbol {0}, direction {1} and type {2}", pendingorder.Order.Security.Name, pendingorder.Order.Direction, pendingorder.Order.Type);

            //Check if order is cancelled, then do not send this order
            if (pendingorder.IsCancelled)
            {
                LocalLog(LogLevel.Warn, "SubmitOrder: pending order for symbol {0} and type {1} was cancelled before submitting: {2}", pendingorder.Order.Security.Name, pendingorder.Order.Type, pendingorder.OrderStatus.ToString());
                return pendingorder.OrderStatus;
            }

            _portfolio.QueueOrder(pendingorder);
            return pendingorder.OrderStatus;
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Submit an order that is going through the regular template cycle of orders
        /// </summary>
        /// <param name="pendingorder"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected bool ProcessOrder(PendingOrder pendingorder, AgentState state)
        {
            //Track Orders
            PendingOrder entryOrder = pendingorder;
            PendingOrder rmOrder = null;

            //Check if we are allowed to trade
            var types = Templates.Select(x => x.GetType());
            RiskManagementTemplate rm = (RiskManagementTemplate)Templates.FirstOrDefault(x => x.GetType().BaseType == typeof(RiskManagementTemplate));

            if (rm != null 
                && !rm.IsTradingAllowed() 
                && (state != AgentState.EntryLong || state != AgentState.EntryShort))
                return false;

            //Check risk management
            if (InvokeRm.Count > 0)
            {
                //Check all Risk Management template logics
                Exec.InvokeAll(InvokeRm, pendingorder, state);

                //On Order Event
                if (InvokeRm[0].Result != null)
                {
                    rmOrder = InvokeRm[0].Result;
                    RiskManagement(rmOrder);
                }
            }

            //Submit our new stop order
            if (rmOrder != null && rmOrder.Order.IsValid)
                SubmitOrder(rmOrder);
            else if (rmOrder != null && !rmOrder.Order.IsValid)
                _logger.Warn("RM: INVALID ORDER {0} - {1}", rmOrder.OrderStatus, rmOrder.Order.Security.Name);

            //Check money management
            if (InvokeMm.Count > 0)
            {
                Exec.InvokeAll(InvokeMm, pendingorder, state);

                MoneyManagement(pendingorder);
            }

            //Submit our new entry order
            if (entryOrder.Order.IsValid)
                SubmitOrder(entryOrder);
            else
                _logger.Warn("MM: INVALID ORDER {0} - {1}", entryOrder.OrderStatus, entryOrder.Order.Security.Name);

            return true;
        }

        #endregion Protected Methods

        #region Private Methods

        private void ClearAgentSate()
        {
            foreach (var dec in CurrentState.Values)
                dec.Clear();
        }

        /// <summary>
        /// Send a log event that is not from the user
        /// </summary>
        /// <param name="lvl"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        private void LocalLog(LogLevel lvl, string message, params object[] args)
        {
            _logger.Log(lvl, "[Agent: {0}] " + string.Format(message, args), AgentId);
        }

        private MethodInfo SearchTemplateMethod(Type instance, string name, params Type[] parmType)
        {
            return instance.GetMethods()
                                 .Where(x => x.Name == name)
                                 .Where(x => x.GetParameters().Length == parmType.Length)
                                 .FirstOrDefault(x => x.GetParameters()[0].ParameterType == parmType[0]);
        }

        private void Stream_GotNewBar(string symbol, int interval)
        {
            //Set the current bar
            try
            {
                CurrentBar[symbol] = Portfolio.Streams[symbol][interval][-1, interval];
            }
            catch
            { //TODO: fix this error
            }

            //Execute the bar event if this agent is running
            if (IsRunning)
                OnBar(CurrentBar[symbol]);
        }

        #endregion Private Methods
    }
}