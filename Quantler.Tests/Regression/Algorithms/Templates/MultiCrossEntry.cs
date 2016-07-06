using Quantler;
using Quantler.Interfaces;
using Quantler.Interfaces.Indicators;
using Quantler.Templates;
using System.Collections.Generic;
using System.Linq;

namespace Quantler.Tests.Regression.Algorithms.Templates
{
    public class MultiCrossEntry : EntryTemplate
    {
        //Private
        Dictionary<ISecurity, ExponentialMovingAverage> _emaslow = new Dictionary<ISecurity, ExponentialMovingAverage>();
        Dictionary<ISecurity, ExponentialMovingAverage> _emafast = new Dictionary<ISecurity, ExponentialMovingAverage>();

        //Fast EMA period
        [Parameter(20, 50, 10, "FastEMA")]
        public int fastperiod { get; set; }

        //Slow EMA period
        [Parameter(100, 200, 20, "SlowEMA")]
        public int slowperiod { get; set; }

        public override void Initialize()
        {
            //initialize this entry template using base symbol
            _emaslow.Add(Agent.Security, Indicators.ExponentialMovingAverage(fastperiod, Agent.Stream));
            _emafast.Add(Agent.Security, Indicators.ExponentialMovingAverage(fastperiod, Agent.Stream));

            //add another item to check
            AddStream(SecurityType.Forex, "AUDJPY", BarInterval.Hour);
            var audjpystream = Portfolio.Streams["AUDJPY"];
            _emaslow.Add(Portfolio.Securities["AUDJPY"], Indicators.ExponentialMovingAverage(fastperiod, audjpystream));
            _emafast.Add(Portfolio.Securities["AUDJPY"], Indicators.ExponentialMovingAverage(fastperiod, audjpystream));
        }

        public override void OnCalculate()
        {
            //Go through all items
            foreach (var security in _emaslow.Keys)
            {
                //Get indicators and values
                var emaslow = _emaslow[security];
                var emafast = _emafast[security];

                //Check if the indicators are ready for usage
                if (!emaslow.IsReady || !emafast.IsReady)
                {
                    NoEntry(security.Name);
                    Agent.Log(LogSeverity.Debug, "No entry (Slow: {0}, Fast: {1})", emaslow.IsReady, emafast.IsReady);
                }
                else if (emafast.Result.CrossedAbove(emaslow.Result) && !IsLong(security))
                {
                    EnterLong(security.Name);
                    Agent.Log(LogSeverity.Info, "Entry spotted for long (Slow: {0}, Fast: {1})", emaslow.Result.CurrentValue, emafast.Result.CurrentValue);
                }
                else if (emafast.Result.CrossedUnder(emaslow.Result) && !IsShort(security))
                {
                    EnterShort(security.Name);
                    Agent.Log(LogSeverity.Info, "Entry spotted for short (Slow: {0}, Fast: {1})", emaslow.Result.CurrentValue, emafast.Result.CurrentValue);
                }
                else
                    NoEntry(security.Name);
            }
        }

        // Check if we are currently long (on our default symbol)
        private bool IsLong(ISecurity sec)
        {
            return Agent.Positions[sec.Name].IsLong;
        }

        // Check if we are currently short (on our default symbol)
        private bool IsShort(ISecurity sec)
        {
            return Agent.Positions[sec.Name].IsShort;
        }
    }
}
