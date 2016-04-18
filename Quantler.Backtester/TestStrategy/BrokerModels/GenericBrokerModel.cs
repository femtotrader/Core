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

using Quantler.Interfaces;
using Quantler.Templates;

/// <summary>
/// Generic trading costs model
/// </summary>
public class GenericBrokerModel : BrokerModelTemplate
{
    #region Public Properties

    public decimal CommPerLot { get; set; }
    public int LatencyInMS { get; set; }
    public decimal SlippageInPips { get; set; }
    public decimal SpreadInPips { get; set; }

    #endregion Public Properties

    #region Public Methods

    public override decimal GetCommission(Order o)
    {
        return CommPerLot * o.Quantity + .06M;
    }

    public override int GetLatencyInMilliseconds(Order o)
    {
        return LatencyInMS;
    }

    public override decimal GetSlippage(Order o)
    {
        return SlippageInPips;
    }

    public override decimal GetSpread(Order o)
    {
        return SpreadInPips;
    }

    #endregion Public Methods
}