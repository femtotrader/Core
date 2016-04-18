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

//Use a fixed time event to exit your currently held positions
internal class TimedExit : ExitTemplate
{
    #region Public Properties

    //Determine the time to exit based on a 24 hour integer format (HHMM)
    [Parameter(2100, 2300, 100, "Exit Time")]
    public int ExitTime { get; set; }

    #endregion Public Properties

    #region Public Methods

    public override void OnCalculate()
    {
        if (CurrentBar[Agent.Symbol].Time >= ExitTime * 100)
            Flatten();
    }

    #endregion Public Methods
}