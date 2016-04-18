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
    public abstract class ExitTemplate : Template
    {
        #region Public Methods

        /// <summary>
        /// Provide an exit long signal to the agent
        /// </summary>
        public void ExitLong()
        {
            this.Agent.CurrentState[this.Agent.Symbol].Add(AgentState.ExitLong);
        }

        /// <summary>
        /// Provide an exit long signal to the agent, for a specific symbol
        /// </summary>
        public void ExitLong(string Symbol)
        {
            if (this.Agent.CurrentState.ContainsKey(Symbol))
                this.Agent.CurrentState[Symbol].Add(AgentState.ExitLong);
        }

        /// <summary>
        /// Provide an exit short signal to the agent
        /// </summary>
        public void ExitShort()
        {
            this.Agent.CurrentState[this.Agent.Symbol].Add(AgentState.ExitShort);
        }

        /// <summary>
        /// Provide an exit short signal to the agent, for a specific symbol
        /// </summary>
        public void ExitShort(string Symbol)
        {
            if (this.Agent.CurrentState.ContainsKey(Symbol))
                this.Agent.CurrentState[Symbol].Add(AgentState.ExitShort);
        }

        /// <summary>
        /// Flatten the current position of the current symbol which the agent is trading
        /// </summary>
        public void Flatten()
        {
            this.Agent.CurrentState[this.Agent.Symbol].Add(AgentState.Flatten);
        }

        /// <summary>
        /// Flatten the current position of a specific symbol which the agent is trading
        /// </summary>
        public void Flatten(string Symbol)
        {
            if (this.Agent.CurrentState.ContainsKey(Symbol))
                this.Agent.CurrentState[Symbol].Add(AgentState.Flatten);
        }

        public virtual void OnCalculate()
        {
            throw new NotImplementedException("Exit templates need OnCalculate Events!");
        }

        #endregion Public Methods
    }
}