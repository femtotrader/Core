using Quantler;
using Quantler.Interfaces;
using Quantler.Interfaces.Indicators;
using Quantler.Templates;
using System.Collections.Generic;
using System.Linq;

namespace Quantler.Tests.Regression.Algorithms.Templates
{
    public class EMACrossEntry : EntryTemplate
    {
        //Private
        private ExponentialMovingAverage emaslow;
        private ExponentialMovingAverage emafast;

        //Fast EMA period
        [Parameter(20, 50, 10, "FastEMA")]
        public int fastperiod { get; set; }

        //Slow EMA period
        [Parameter(100, 200, 20, "SlowEMA")]
        public int slowperiod { get; set; }

        public override void Initialize()
        {
            //initialize this entry template
            emaslow = Indicators.ExponentialMovingAverage(slowperiod, Agent.Stream);
            emafast = Indicators.ExponentialMovingAverage(fastperiod, Agent.Stream);
        }

        public override void OnCalculate()
        {
            //Charting
            UpdateChart("ROI", ChartType.Step, Agent.Results.ROI);
            UpdateChart("DD", ChartType.Line, Agent.Results.MaxDDPortfolio);

            //Check if the indicators are ready for usage
            if (!emaslow.IsReady || !emafast.IsReady)
            {
                NoEntry();
                Agent.Log(LogSeverity.Debug, "No entry (Slow: {0}, Fast: {1})", emaslow.IsReady, emafast.IsReady);
            }
            else if (emafast.Result.CrossedAbove(emaslow.Result) && !IsLong())
            {
                EnterLong();
                Agent.Log(LogSeverity.Info, "Entry spotted for long (Slow: {0}, Fast: {1})", emaslow.Result.CurrentValue, emafast.Result.CurrentValue);
            }
            else if (emafast.Result.CrossedUnder(emaslow.Result) && !IsShort())
            {
                EnterShort();
                Agent.Log(LogSeverity.Info, "Entry spotted for short (Slow: {0}, Fast: {1})", emaslow.Result.CurrentValue, emafast.Result.CurrentValue);
            }
            else
                NoEntry();
        }

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
    }
}
