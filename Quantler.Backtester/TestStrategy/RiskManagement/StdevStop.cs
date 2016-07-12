using Quantler;
using Quantler.Interfaces;
using Quantler.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;

//Use a fixed stop amount to manage new positions
class Risk_Hedger : RiskManagementTemplate
{
    //Specify arbitrage amount
    [Parameter(10, 100, 10, "Hedged %")]
    public int arbitrage { get; set; }

    private ISecurity lastsecurity;
    private Dictionary<ISecurity, double> corr = new Dictionary<ISecurity, double>();

    public override void Initialize()
    {
        //Add other pairs data
        AddStream(SecurityType.Forex, "AUDJPY", BarInterval.Day);
        AddStream(SecurityType.Forex, "AUDNZD", BarInterval.Day);
        AddStream(SecurityType.Forex, "AUDUSD", BarInterval.Day);
        AddStream(SecurityType.Forex, "CADJPY", BarInterval.Day);
        AddStream(SecurityType.Forex, "CHFJPY", BarInterval.Day);
        AddStream(SecurityType.Forex, "EURGBP", BarInterval.Day);
        AddStream(SecurityType.Forex, "EURJPY", BarInterval.Day);
        AddStream(SecurityType.Forex, "EURUSD", BarInterval.Day);
        AddStream(SecurityType.Forex, "GBPJPY", BarInterval.Day);
        AddStream(SecurityType.Forex, "GBPUSD", BarInterval.Day);
        AddStream(SecurityType.Forex, "NZDUSD", BarInterval.Day);
        AddStream(SecurityType.Forex, "USDCAD", BarInterval.Day);
        AddStream(SecurityType.Forex, "USDCHF", BarInterval.Day);
        AddStream(SecurityType.Forex, "USDJPY", BarInterval.Day);
        AddStream(SecurityType.Forex, "EURCHF", BarInterval.Day);

        //Add correlation pairs
        corr.Add(Portfolio.Securities["AUDJPY"], 0);
        corr.Add(Portfolio.Securities["AUDNZD"], 0);
        corr.Add(Portfolio.Securities["AUDUSD"], 0);
        corr.Add(Portfolio.Securities["CADJPY"], 0);
        corr.Add(Portfolio.Securities["CHFJPY"], 0);
        corr.Add(Portfolio.Securities["EURGBP"], 0);
        corr.Add(Portfolio.Securities["EURJPY"], 0);
        corr.Add(Portfolio.Securities["EURUSD"], 0);
        corr.Add(Portfolio.Securities["GBPJPY"], 0);
        corr.Add(Portfolio.Securities["GBPUSD"], 0);
        corr.Add(Portfolio.Securities["NZDUSD"], 0);
        corr.Add(Portfolio.Securities["USDCAD"], 0);
        corr.Add(Portfolio.Securities["USDCHF"], 0);
        corr.Add(Portfolio.Securities["USDJPY"], 0);
        corr.Add(Portfolio.Securities["EURCHF"], 0);
    }

    // Executed before each trade made
    public override bool IsTradingAllowed()
    {
        //Do not trade if the ROI is less than -20%
        if (Agent.Results.ROI < -.20M)
            return false;
        else
            return Agent.Bars[Agent.Security, TimeSpan.FromDays(1), -15].IsValid;
    }

    // Executed when a new order has been created
    public PendingOrder RiskManagement(PendingOrder pendingOrder, AgentState state)
    {
        //Check if we have enough data
        if (!Agent.Bars[Agent.Security, (int)BarInterval.Day, -15].IsValid)
            return null;
        //Check if we can calculate the correlation
        else if (!corr.ContainsKey(Agent.Security))
            return null;

        //Calculate correlation for each symbol
        var basevalues = new List<double>();
        for (int i = 0; i < 15; i++)
            basevalues.Add((double)Agent.Bars[Agent.Security, (int)BarInterval.Day, i].Close);

        //Get all symbol data
        foreach (var symbol in corr.Keys.ToArray())
        {
            var values = new List<double>();
            for (int i = 0; i < 15; i++)
                values.Add((double)Agent.Bars[symbol, (int)BarInterval.Day, i].Close);

            //calculate correlation coeff
            corr[symbol] = Correlation.Pearson(basevalues.ToArray(), values.ToArray());
        }

        //Get item with the highest corr
        var max = corr
            .Where(x => x.Key != pendingOrder.Order.Security)
            .Max(n => n.Value);
        var security = corr
            .Where(x => x.Value == max).FirstOrDefault().Key;

        //Remove old position
        if (lastsecurity != security && !Agent.Positions[security].IsFlat)
            SubmitOrder(CreateOrder(lastsecurity.Name, Direction.Flat, Agent.Positions[security].FlatQuantity));

        //Return our current order
        lastsecurity = security;
        decimal quantity = pendingOrder.Order.Quantity * (arbitrage / 100M);
        quantity = quantity > 0.01M ? quantity : 0.01M;
        quantity += Math.Abs(Agent.Positions[security].FlatQuantity);

        return CreateOrder(security.Name, pendingOrder.Order.Direction == Direction.Long ? Direction.Short : Direction.Long, quantity, 0, 0);
    }
}