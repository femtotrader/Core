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
using Quantler.Interfaces;
using System;
using System.Collections.Generic;

namespace Quantler.Templates
{
    public abstract class Template : ITemplate
    {
        #region Private Fields

        private ITradingAgent _agent;

        #endregion Private Fields

        #region Public Properties

        public ITradingAgent Agent { get { return _agent; } set { if (_agent == null) _agent = value; } }

        public BarIndexer Bars
        {
            get { return _agent.Bars; }
        }

        public IReadOnlyDictionary<string, Bar> CurrentBar { get { return Agent.CurrentBar; } }
        public IReadOnlyDictionary<string, Tick> CurrentTick { get { return Agent.CurrentTick; } }
        public int Id { get; set; }
        public IndicatorFactory Indicators { get { return Agent.StandardIndicators; } }
        public string Name { get; set; }
        public IPortfolio Portfolio { get { return Agent.Portfolio; } }

        #endregion Public Properties

        #region Public Methods

        public void AddStream(SecurityType type, string name)
        {
            Agent.AddDataStream(type, name);
        }

        public void AddStream(SecurityType type, string name, TimeSpan interval)
        {
            Agent.AddDataStream(type, name, interval);
        }

        public void AddStream(SecurityType type, string name, int interval)
        {
            Agent.AddDataStream(type, name, interval);
        }

        public void AddStream(SecurityType type, string name, BarInterval interval)
        {
            Agent.AddDataStream(type, name, interval);
        }

        public PendingOrder CreateOrder(string symbol, Direction direction, decimal quantity, decimal limitPrice = 0, decimal stopPrice = 0, string comment = "")
        {
            return Agent.CreateOrder(symbol, direction, quantity, limitPrice, stopPrice, comment);
        }

        public virtual void Initialize()
        {
        }

        public PendingOrder LimitOrder(string symbol, Direction direction, double quantity, double limitPrice, string comment = "")
        {
            var po = CreateOrder(symbol, direction, (decimal)quantity, (decimal)limitPrice, 0, comment);
            SubmitOrder(po);
            return po;
        }

        public PendingOrder MarketOrder(string symbol, Direction direction, double quantity, string comment = "")
        {
            var po = CreateOrder(symbol, direction, (decimal)quantity, 0, 0, comment);
            SubmitOrder(po);
            return po;
        }

        public virtual void OnCalculate()
        {
        }

        public PendingOrder StopLimitOrder(string symbol, Direction direction, double quantity, double limitPrice, double stopPrice, string comment = "")
        {
            var po = CreateOrder(symbol, direction, (decimal)quantity, (decimal)limitPrice, (decimal)stopPrice, comment);
            SubmitOrder(po);
            return po;
        }

        public PendingOrder StopOrder(string symbol, Direction direction, double quantity, double stopPrice, string comment = "")
        {
            var po = CreateOrder(symbol, direction, (decimal)quantity, 0, (decimal)stopPrice, comment);
            SubmitOrder(po);
            return po;
        }

        public StatusType SubmitOrder(PendingOrder pendingorder)
        {
            return Agent.SubmitOrder(pendingorder);
        }

        public void UpdateChart(string name, ChartType type, decimal value)
        {
            Agent.ChartUpdate(this, name, type, value);
        }

        public void UpdateChart(string name, ChartType type, double value)
        {
            Agent.ChartUpdate(this, name, type, (decimal)value);
        }

        public void UpdateChart(string name, ChartType type, int value)
        {
            Agent.ChartUpdate(this, name, type, value);
        }

        public void UpdateChart(string name, ChartType type, float value)
        {
            Agent.ChartUpdate(this, name, type, (decimal)value);
        }

        #endregion Public Methods
    }
}