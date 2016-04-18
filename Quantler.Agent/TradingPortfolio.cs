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
using Quantler.Tracker;
using Quantler.Trades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Quantler.Agent
{
    public partial class TradingPortfolio : PortfolioManager
    {
        #region Private Fields

        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly List<PendingOrder> _queuedOrders = new List<PendingOrder>();
        private readonly Dictionary<string, DataStream> _streams = new Dictionary<string, DataStream>();
        private List<TradingAgentManager> _agents = new List<TradingAgentManager>();
        private Results _currentresults = new Results();
        private List<PendingOrder> _pendingOrders;
        private IPositionTracker _positions;

        #endregion Private Fields

        #region Public Constructors

        public TradingPortfolio()
        {
            _pendingOrders = new List<PendingOrder>();
            OrderFactory = new Orders.OrderFactoryImpl(this);
        }

        #endregion Public Constructors

        #region Public Events

        public event OrderSourceDelegate SendOrderEvent;

        #endregion Public Events

        #region Public Properties

        public IAccount Account { get; private set; }

        public ITradingAgent[] Agents
        {
            get { return _agents.ToArray(); }
        }

        public string FullName
        {
            get;
            set;
        }

        public int Id
        {
            get;
            set;
        }

        public string[] Indicators
        {
            get;
            set;
        }

        IPositionTracker IPortfolio.Positions
        {
            get { return _positions; }
        }

        public bool IsValid
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public OrderFactory OrderFactory { get; set; }

        public PendingOrder[] PendingOrders
        {
            get { return _pendingOrders.ToArray(); }
        }

        public Result Results
        {
            get { return _currentresults; }
        }

        public ISecurityTracker Securities
        {
            get
            {
                return Account.Securities;
            }
        }

        public Dictionary<string, DataStream> Streams
        {
            get
            {
                return _streams;
            }
        }

        public TradingAgentManager[] TradingAgents
        {
            get { return _agents.ToArray(); }
        }

        #endregion Public Properties

        #region Public Methods

        public void AddStream(DataStream stream)
        {
            Log(LogLevel.Debug, "Adding stream to portfolio, with symbol {0}", stream.Security.Name);
            DataStream existing;
            if (!_streams.TryGetValue(stream.Security.Name, out existing))
            {
                _streams.Add(stream.Security.Name, stream);
            }
            else
                existing.AddInterval(stream.Intervals);
        }

        public void GotFill(Trade fill, PendingOrder pendingorder)
        {
            //Process fill for this portfolio
            _positions.GotFill(fill);

            foreach (var agent in Agents.Where(x => x.AgentId == pendingorder.Order.AgentId))
                agent.OnFill(fill, pendingorder);
        }

        public void GotOrder(PendingOrder pendingorder)
        {
            foreach (var agent in Agents.Where(x => x.AgentId == pendingorder.Order.AgentId))
                agent.OnOrder(pendingorder);
        }

        public void GotOrderCancel(PendingOrder pendingorder)
        {
            foreach (var agent in Agents.Where(x => x.AgentId == pendingorder.Order.AgentId))
                agent.OnOrderUpdate(pendingorder);
        }

        public void GotOrderUpdate(PendingOrder pendingorder)
        {
            foreach (var agent in Agents.Where(x => x.AgentId == pendingorder.Order.AgentId))
                agent.OnOrderUpdate(pendingorder);
        }

        public void GotTick(Tick tick)
        {
            //Check all agents
            foreach (var agent in Agents)
                agent.OnTick(tick);

            //Go through all streams
            foreach (var stream in _streams.Values.Where(x => x.Security.Name == tick.Symbol))
                stream.GotTick(tick);

            //Process all orders that were created
            ProcessQueuedOrders();
        }

        public void Initialize()
        {
            Log(LogLevel.Debug, "Initializing portfolio");
            if (Agents == null)
            {
                _agents = new List<TradingAgentManager>();
            }
        }

        public void InjectAgent<T>()
            where T : ITradingAgent
        {
            //Get type
            Type ct = typeof(T);

            //Check if expected basetype is present
            if (ct.BaseType == null)
                throw new Exception("Could not get expected Base Type of type " + ct);

            //Log current procedure
            Log(LogLevel.Debug, "Adding new trading agent to portfolio, agent type: {0}", ct.BaseType.Name);

            //Get object and initialize
            var cons = ct.GetConstructors()[0];
            var parms = cons.GetParameters();
            object[] args = new object[parms.Length];

            for (int i = 0; i < parms.Length; i++)
                args[i] = Activator.CreateInstance(parms[i].ParameterType);

            var toreturn = (TradingAgentManager)Activator.CreateInstance(ct, args);

            //Associate templates
            foreach (ITemplate t in args.OfType<ITemplate>())
                toreturn.AddTemplate(t);

            //Asscociate portfolio
            toreturn.SetPortfolio(this);
            _agents.Add(toreturn);
        }

        public void QueueOrder(PendingOrder order)
        {
            _pendingOrders.Add(order);
            _queuedOrders.Add(order);
        }

        public void RemoveAgent(int agentId, bool flatten = false)
        {
            Log(LogLevel.Debug, "Received request to remove agent from portfolio({0}) where agent id = {1} (flatten = {2})", Id, agentId, flatten);
            var found = _agents.FirstOrDefault(x => x.AgentId == agentId);
            if (found != null)
            {
                found.Stop();
                Thread.Sleep(100);  //Give it some time to process
                if (flatten)
                {
                    found.Flatten();
                    DateTime timeout = DateTime.UtcNow.AddSeconds(10);
                    while (!found.Positions[found.Symbol].IsFlat && DateTime.UtcNow < timeout)
                    {
                        Thread.Sleep(100);  //Give it some time to process
                    }

                    //Cancel any remaining orders
                    foreach (var po in found.PendingOrders)
                    {
                        po.Cancel();
                    }
                }
                found.Deinitialize();
                _agents.Remove(found);
            }
            else
                Log(LogLevel.Warn, "Could not find agent with id {0} in portfolio {1}", agentId, Id);
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void SetAccount(IAccount account)
        {
            if (Account == null)
            {
                Account = account;
                _currentresults = new Results(0, account);
                _positions = new PositionTracker(account);
            }
        }

        public void SetPendingOrders(PendingOrder[] orders)
        {
            _pendingOrders = orders.ToList();
        }

        public void SetPositionTracker(IPositionTracker tracker)
        {
            _positions = tracker;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Send a log event
        /// </summary>
        /// <param name="lvl"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        private void Log(LogLevel lvl, string message, params object[] args)
        {
            _logger.Log(lvl, "[Portfolio: {0}] " + string.Format(message, args), Id);
        }

        private void ProcessQueuedOrders()
        {
            //Go through all pending orders and send these to the broker
            foreach (var order in _queuedOrders.Where(order => SendOrderEvent != null))
            {
                SendOrderEvent(order, 0);
            }

            //Clear all known queued orders
            _queuedOrders.Clear();
        }

        #endregion Private Methods
    }
}