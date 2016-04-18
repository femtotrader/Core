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
using System.Linq;

/// <summary>
/// FXCM Broker trading costs implementation
/// </summary>
public class FXCMBrokerModel : BrokerModelTemplate
{
    /*
     * FXCM Spreads And Commissions - https://www.fxcm.com/advantages/spreads-commissions/
     * 0.04 PER - 1K EUR/USD - GBP/USD - USD/JPY - USD/CHF - AUD/USD - EUR/JPY - GBP/JPY
     * 0.06 PER - 1K ALL OTHER PAIRS
     * Additional spread - Observed 0.2 pips (compared to interbank rates)
     * Latency is an assumption
     */

    private string[] majors = { "EURUSD", "GBPUSD", "USDJPY", "USDCHF", "AUDUSD", "EURJPY", "GBPJPY" };

    public override decimal GetCommission(Order o)
    {
        decimal lotprice;

        if (majors.Contains(o.Security.Name))
            lotprice = 4M;
        else
            lotprice = 6M;

        return lotprice * o.Quantity;
    }

    public override int GetLatencyInMilliseconds(Order o)
    {
        return 60;
    }

    public override decimal GetSpread(Order o)
    {
        return 0.2M;
    }

    public override decimal GetSlippage(Order o)
    {
        return 0;
    }
}