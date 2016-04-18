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
using Quantler.Templates;

//Use a fixed pip sized amount to exit a currently held position
internal class FixedTakeProfit : ExitTemplate
{
    #region Public Properties

    //Determine the size in pips before exit
    [Parameter(10, 100, 10, "PipSize")]
    public int PipSize { get; set; }

    #endregion Public Properties

    #region Public Methods

    //Calculate on each bar close (default symbol and timeframe)
    public override void OnCalculate()
    {
        //Get current position
        var current = Agent.Positions[Agent.Symbol];

        //Check if we have a position
        if (current.IsFlat)
            return;

        //Calculate pips
        decimal entryprice = current.AvgPrice;
        decimal close = Agent.CurrentBar[Agent.Symbol].Close;
        var pips = (current.IsLong ? close - entryprice : entryprice - close) / Agent.Stream.Security.PipSize;

        //Check our exit
        if (pips > PipSize)
            Flatten();
    }

    #endregion Public Methods
}