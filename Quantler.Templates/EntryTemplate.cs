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

namespace Quantler.Templates
{
    public abstract class EntryTemplate : Template
    {
        #region Public Methods

        /// <summary>
        /// Provide an entry long signal to the agent
        /// </summary>
        public void EnterLong()
        {
            Agent.CurrentState[Agent.Symbol].Add(AgentState.EntryLong);
        }

        /// <summary>
        /// Provide an entry long signal to the agent for a specific symbol
        /// </summary>
        /// <param name="Symbol"></param>
        public void EnterLong(string Symbol)
        {
            if (Agent.CurrentState.ContainsKey(Symbol))
                Agent.CurrentState[Symbol].Add(AgentState.EntryLong);
        }

        /// <summary>
        /// Provide an entry long signal to the agent for a specific security
        /// </summary>
        /// <param name="Security"></param>
        public void EnterLong(ISecurity Security)
        {
            EnterLong(Security.Name);
        }

        /// <summary>
        /// Provide an entry Short signal to the agent
        /// </summary>
        public void EnterShort()
        {
            Agent.CurrentState[Agent.Symbol].Add(AgentState.EntryShort);
        }

        /// <summary>
        /// Provide an entry short signal to the agent for a specific symbol
        /// </summary>
        /// <param name="Symbol"></param>
        public void EnterShort(string Symbol)
        {
            if (Agent.CurrentState.ContainsKey(Symbol))
                Agent.CurrentState[Symbol].Add(AgentState.EntryShort);
        }

        /// <summary>
        /// Provide an entry short signal to the agent for a specific security
        /// </summary>
        /// <param name="Security"></param>
        public void EnterShort(ISecurity Security)
        {
            EnterShort(Security.Name);
        }

        /// <summary>
        /// Flatten the current position of the current symbol which the agent is trading
        /// </summary>
        public void Flatten()
        {
            Agent.CurrentState[Agent.Symbol].Add(AgentState.Flatten);
        }

        /// <summary>
        /// Provide a flatten current position signal to the agent for a specific symbol
        /// </summary>
        /// <param name="Symbol"></param>
        public void Flatten(string Symbol)
        {
            if (Agent.CurrentState.ContainsKey(Symbol))
                Agent.CurrentState[Symbol].Add(AgentState.Flatten);
        }

        /// <summary>
        /// Tell the agent to not enter the market
        /// </summary>
        public void NoEntry()
        {
            Agent.CurrentState[Agent.Symbol].Add(AgentState.NoEntry);
        }

        /// <summary>
        /// Tell the agent to not enter the market for a specific symbol
        /// </summary>
        public void NoEntry(string Symbol)
        {
            if (Agent.CurrentState.ContainsKey(Symbol))
                Agent.CurrentState[Symbol].Add(AgentState.NoEntry);
        }

        public virtual void OnCalculate()
        {
            throw new NotImplementedException("Entry templates need OnCalculate Events!");
        }

        #endregion Public Methods
    }
}