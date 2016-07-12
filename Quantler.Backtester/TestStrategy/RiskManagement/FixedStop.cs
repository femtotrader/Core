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
using System.Linq;

//Use a fixed stop amount to manage new positions
class FixedStop : RiskManagementTemplate
{
    //Specify stop amount in pips
    [Parameter(20, 50, 10, "Stop Pips")]
    public int stoppips { get; set; }

    // Executed before each trade made
    public override bool IsTradingAllowed()
    {
        //We are always allowed to make a new trade
        return true;
    }

    // Executed when a new order has been created
    public PendingOrder RiskManagement(PendingOrder pendingOrder, AgentState state)
    {
        //Remove all current pending stop orders
        Agent.PendingOrders
            .Where(x => x.Order.StopPrice > 0).Cancel();

        //Check our current state
        if (state != AgentState.EntryLong && state != AgentState.EntryShort)
            return null;

        //Get current close price
        decimal CurrentClose = CurrentBar[pendingOrder.Order.Symbol].Close;

        //Stop size measured in pips
        var pips = pendingOrder.Order.Security.PipSize * stoppips;

        //Get stop price based on the pips stop size
        decimal price = pendingOrder.Order.Direction == Direction.Long ?
                                CurrentClose - pips :
                                CurrentClose + pips;

        //Return our current stoporder
        return CreateOrder(pendingOrder.Order.Symbol, pendingOrder.Order.Direction == Direction.Long ? Direction.Short : Direction.Long,
            pendingOrder.Order.Quantity, 0, price);
    }
}