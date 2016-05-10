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
using System;

//Use a fixed time event to exit your currently held positions
class TimedExit : ExitTemplate
{
    //Determine the time to exit based on a 24 hour integer format (HHMM)
    [Parameter(2100, 2300, 100, "Exit Time")]
    public int ExitTime { get; set; }

    private DateTime exit = DateTime.MinValue;

    public override void Initialize()
    {
        //Set exit time based on hours provided
        exit = exit.AddHours(Math.Round(ExitTime / 100D));
    }

    public override void OnCalculate()
    {
        //If current bar is above or equal to the exit time, exit any position we current have for this agent
        if (CurrentBar[Agent.Symbol].BarDateTime >= exit)
            Flatten();
    }
}