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
class FixedPositionSizing : MoneyManagementTemplate
{
    // Determine the amount of units to trade with
    [Parameter(25, 100, 25, "Fixed Unit Size")]
    public int FixedSize { get; set; }

    public void PositionSize(PendingOrder pendingorder, AgentState state)
    {
        //Check if this is for entry
        if (state != AgentState.EntryLong && state != AgentState.EntryShort)
            return;

        //Get the size of the position
        var positionsize = (pendingorder.Order.Quantity * FixedSize);

        //Get current stop order
        var currentstop = Agent.PendingOrders.FirstOrDefault(x => x.Order.Symbol == pendingorder.Order.Symbol &&
                    (x.Order.Type == OrderType.Stop || x.Order.Type == OrderType.StopLimit));

        //See if we need to update our stop order
        if (currentstop != null)
            currentstop.Update(x => x.Quantity = positionsize);

        //Set our pending order on our size (check if we need to reverse the current order)
        var currentpos = Agent.Positions[pendingorder.Order.Security];
        if (!currentpos.IsFlat)
            positionsize += Math.Abs(currentpos.Quantity);

        //Update pending order
        pendingorder.Update(x => x.Quantity = positionsize);
    }
}