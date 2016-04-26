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
using Quantler.Interfaces.Indicators;
using Quantler.Templates;

// EMA CrossOver Entry Example
internal class EMACrossExample : EntryTemplate
{
    #region Private Fields

    private ExponentialMovingAverage emafast;

    //Private
    private ExponentialMovingAverage emaslow;

    #endregion Private Fields

    #region Public Properties

    //Fast EMA period
    [Parameter(5, 15, 5, "FastEMA")]
    public int fastperiod { get; set; }

    //Slow EMA period
    [Parameter(20, 40, 10, "SlowEMA")]
    public int slowperiod { get; set; }

    #endregion Public Properties

    #region Public Methods

    public override void Initialize()
    {
        //initialize this entry template
        emaslow = Indicators.ExponentialMovingAverage(slowperiod, Agent.Stream);
        emafast = Indicators.ExponentialMovingAverage(fastperiod, Agent.Stream);
    }

    public override void OnCalculate()
    {
        //Check if the indicators are ready for usage
        if (!emaslow.IsReady || !emafast.IsReady)
            NoEntry();
        else if (emafast.Result.CrossedAbove(emaslow.Result) && !IsLong())
            EnterLong();
        else if (emafast.Result.CrossedUnder(emaslow.Result) && !IsShort())
            EnterShort();
        else
            NoEntry();
    }

    #endregion Public Methods

    #region Private Methods

    // Check if we are currently long (on our default symbol)
    private bool IsLong()
    {
        return Agent.Positions[Agent.Symbol].IsLong;
    }

    // Check if we are currently short (on our default symbol)
    private bool IsShort()
    {
        return Agent.Positions[Agent.Symbol].IsShort;
    }

    #endregion Private Methods
}