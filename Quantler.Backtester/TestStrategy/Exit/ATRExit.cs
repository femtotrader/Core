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
using Quantler.Interfaces.Indicators;
using Quantler.Templates;

//Use the ATR or Average True Range to determine our exit
class ATRExit : ExitTemplate
{
    private AverageTrueRange atr;
    private decimal currentprice;

    //Define the period at which the ATR is calculated on
    [Parameter(15, 25, 5, "atrperiod")]
    public int atrperiod { get; set; }

    //Define a multiplier to increase the range of the ATR
    [Parameter(1, 2, 1, "multiplier")]
    public int multiplier { get; set; }

    //Initialize this exit template (runs only once, at our start)
    public override void Initialize()
    {
        atr = Indicators.AverageTrueRange(atrperiod, Agent.Stream);
    }

    //Calculate on each bar close (default symbol and timeframe)
    public override void OnCalculate()
    {
        //Check if ready
        if (!atr.IsReady)
            return;

        //Check before using the ATR
        if (currentprice != 0 && !HasPosition())
            currentprice = 0;
        else if (IsLong() && currentprice == 0)
            currentprice = Portfolio.Positions[Agent.Symbol].AvgPrice + (atr.Result[0] * multiplier);
        else if (IsShort() && currentprice == 0)
            currentprice = Portfolio.Positions[Agent.Symbol].AvgPrice - (atr.Result[0] * multiplier);

        //Check for exit strategy
        if (IsLong() && CurrentBar[Agent.Symbol].High > currentprice)
            ExitLong();
        else if (IsShort() && CurrentBar[Agent.Symbol].Low < currentprice)
            ExitShort();
    }

    //Check if we currently have a position (True = Yes)
    private bool HasPosition()
    {
        return !Agent.Positions[Agent.Symbol].IsFlat;
    }

    //Check if we are currently long
    private bool IsLong()
    {
        return Agent.Positions[Agent.Symbol].IsLong;
    }

    //Check if we are currently short
    private bool IsShort()
    {
        return Agent.Positions[Agent.Symbol].IsShort;
    }
}