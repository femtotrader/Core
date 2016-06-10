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

//Manage new positions based on a volatility stop
class StdevStop : RiskManagementTemplate
{
    //Multiplier to use for the StDev calculation
    [Parameter(1, 4, 1, "multiplier")]
    public int multiplier { get; set; }

    // We are always allowed to trade
    public override bool IsTradingAllowed()
    {
        return true;
    }

    //Pass on the Risk Management order
    public PendingOrder RiskManagement(PendingOrder pendingorder, AgentState state)
    {
        //Check if this is an exit order, than do nothing
        if (pendingorder.Order.Direction == Direction.Flat)
            return null;

        //Cancel any pending stop orders
        Agent.PendingOrders.Where(x => x.Order.Symbol == pendingorder.Order.Symbol &&
            (x.Order.Type == OrderType.Stop || x.Order.Type == OrderType.StopLimit)).Cancel();

        //Create a flattening stop limit order
        return CreateOrder(pendingorder.Order.Symbol, Direction.Flat, pendingorder.Order.Quantity, 0, StopLevel(pendingorder));
    }

    //Calculate the StDev
    private double STDEV(decimal[] values)
    {
        decimal average = values.Average();
        decimal sumOfSquaresOfDifferences = values.Select(val => (val - average) * (val - average)).Sum();
        double sd = Math.Sqrt((double)sumOfSquaresOfDifferences / (double)values.Length);
        return sd;
    }

    //Set the current stop level based on the entry order
    private decimal StopLevel(PendingOrder pendingorder)
    {
        //Set a standard stop as the initial run might not have enough bars
        decimal value = 500 * CurrentTick[Agent.Symbol].AskSize;
        int period = 25;

        //Get Bars
        decimal[] bars = Portfolio.Streams[pendingorder.Order.Symbol][Agent.TimeFrame].Close();

        if (bars.Length >= period)
        {
            value = Convert.ToDecimal(STDEV(bars.Take(period).ToArray()));  //Set StDev
            value *= multiplier;                                            //Add multiplier
        }

        // Return stop level
        return pendingorder.Order.Direction == Direction.Long ?
            CurrentBar[Agent.Symbol].Close - value :
            CurrentBar[Agent.Symbol].Close + value;
    }
}