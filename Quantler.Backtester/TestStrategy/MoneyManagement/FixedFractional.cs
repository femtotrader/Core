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

using Quantler;
using Quantler.Interfaces;
using Quantler.Templates;
using System;
using System.Linq;

//Fixed positionsizing based on a fixed amount of units
internal class FixedFractional : MoneyManagementTemplate
{
    #region Public Properties

    // Determine the fixed fractional percentage 100 = 1% and 200 = 2%
    [Parameter(100, 200, 10, "Fixed Percentage")]
    public int FixedPercentage { get; set; }

    #endregion Public Properties

    #region Public Methods

    public void PositionSize(PendingOrder pendingorder, AgentState state)
    {
        //Check if this is for entry
        if (state != AgentState.EntryLong && state != AgentState.EntryShort)
            return;

        //Get current stop order
        var currentstop = Portfolio.PendingOrders.FirstOrDefault(x => x.Order.Symbol == pendingorder.Order.Symbol &&
                    (x.Order.Type == OrderType.Stop || x.Order.Type == OrderType.StopLimit));

        //Check if we have a stop order, cannot determine the risk without it
        if (currentstop == null)
            return;

        //Get the current close
        var currentprice = CurrentBar[pendingorder.Order.Symbol].Close;

        //Get the current risk (delta price)
        var risk = (Math.Abs(currentprice - currentstop.Order.StopPrice) / pendingorder.Order.Security.PipSize) * (pendingorder.Order.Security.PipValue / 100);

        //Calculate the amount of lots (f * Equity/| Trade Risk|)
        int microlots = (int)(((FixedPercentage * .0001M) * Portfolio.Account.Equity) / risk);
        decimal quantity = microlots * .01M;

        //Set the stop order on our size
        currentstop.Update(x => x.Quantity = quantity);

        //Set our pending order on our size (check if we need to reverse the current order)
        var currentpos = Agent.Positions[pendingorder.Order.Security];
        if (!currentpos.IsFlat)
            quantity += Math.Abs(currentpos.Quantity);

        pendingorder.Update(x => x.Quantity = quantity);
    }

    #endregion Public Methods
}