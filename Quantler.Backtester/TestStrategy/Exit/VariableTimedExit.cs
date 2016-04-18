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
internal class VariableTimedExit : ExitTemplate
{
    #region Private Fields

    private DateTime ExitDT = DateTime.UtcNow.AddYears(1);

    #endregion Private Fields

    #region Public Properties

    //Determine the time to exit based on hours
    [Parameter(1, 5, 1, "Exit Time In Hours")]
    public int ExitTime { get; set; }

    #endregion Public Properties

    #region Public Methods

    public override void OnCalculate()
    {
        var positions = Agent.Positions[Agent.Symbol];
        if (!positions.IsFlat && DateTime.UtcNow >= ExitDT)
        {
            ExitDT = DateTime.UtcNow.AddHours(ExitTime);
            Flatten();
        }
    }

    #endregion Public Methods
}