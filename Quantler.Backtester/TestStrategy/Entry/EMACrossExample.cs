using Quantler;
using Quantler.Interfaces;
using Quantler.Interfaces.Indicators;
using Quantler.Templates;

// EMA CrossOver Entry Example
class EMACrossExample : EntryTemplate
{
    private ExponentialMovingAverage emafast;

    //Private
    private ExponentialMovingAverage emaslow;

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