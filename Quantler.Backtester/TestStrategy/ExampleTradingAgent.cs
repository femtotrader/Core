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

using Quantler.Agent;
using Quantler.Interfaces;

internal class ExampleTradingAgent : TradingAgent
{
    #region Public Constructors

    /// <summary>
    /// Constructor where templates are injected
    /// </summary>
    /// <param name="e"></param>
    /// <param name="p"></param>
    public ExampleTradingAgent(EMACrossExample e, FixedPositionSizing p)
    {
        e.slowperiod = 100;
        e.fastperiod = 50;

        p.FixedSize = 10;
    }

    #endregion Public Constructors

    #region Public Methods

    /// <summary>
    /// Check entry decisions and initiate order flow if needed
    /// </summary>
    public override void Entry()
    {
        //Base return composition
        if (CurrentState.Count == 0)
            return;

        foreach (var decisions in CurrentState)
        {
            if (decisions.Value.Count == 0)
                continue;

            var sec = Portfolio.Securities[decisions.Key];

            if (decisions.Value.TrueForAll(x => x == AgentState.EntryLong))  //thus long
                ProcessOrder(CreateOrder(sec, Direction.Long, sec.OrderMinQuantity), AgentState.EntryLong);
            else if (decisions.Value.TrueForAll(x => x == AgentState.EntryShort))  //thus short
                ProcessOrder(CreateOrder(sec, Direction.Short, sec.OrderMinQuantity), AgentState.EntryShort);
            else if (decisions.Value.Contains(AgentState.Flatten) && Portfolio.Positions[decisions.Key] != null)  //thus flatten
                ProcessOrder(CreateOrder(sec, Direction.Flat, sec.OrderMinQuantity), AgentState.Flatten);
        }
    }

    /// <summary>
    /// Check exit decisions and initiate order flow if needed
    /// </summary>
    public override void Exit()
    {
        //Base return composition
        if (CurrentState.Count == 0)
            return;

        foreach (var decisions in CurrentState)
        {
            if (decisions.Value.Count == 0)
                continue;

            var currentPosition = Portfolio.Positions[decisions.Key];
            var sec = Portfolio.Securities[decisions.Key];

            if (currentPosition != null)
            {
                if (currentPosition.IsLong && decisions.Value.Contains(AgentState.ExitLong))
                    ProcessOrder(CreateOrder(sec, Direction.Flat, sec.OrderMinQuantity), AgentState.Flatten);
                else if (currentPosition.IsShort && decisions.Value.Contains(AgentState.ExitShort))
                    ProcessOrder(CreateOrder(sec, Direction.Flat, sec.OrderMinQuantity), AgentState.Flatten);
                else if (!currentPosition.IsFlat && decisions.Value.Contains(AgentState.Flatten))
                    ProcessOrder(CreateOrder(sec, Direction.Flat, sec.OrderMinQuantity), AgentState.Flatten);
            }
        }
    }

    /// <summary>
    /// Moneymanagement adjustments
    /// </summary>
    /// <param name="order"></param>
    public override void MoneyManagement(PendingOrder order)
    {
    }

    /// <summary>
    /// Riskmanagement adjustments
    /// </summary>
    /// <param name="order"></param>
    public override void RiskManagement(PendingOrder order)
    {
    }

    #endregion Public Methods
}