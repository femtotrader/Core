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

using Quantler.Interfaces.Indicators;
using System;

namespace Quantler.Interfaces
{
    /// <summary>
    /// Factory class is used to instantiate new indicators that are ready to use
    /// </summary>
    public interface IndicatorFactory
    {
        #region Public Methods

        Aroon Aroon(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> computeLow, Func<Bar, decimal> computeHigh);

        Aroon Aroon(int period, DataStream stream, TimeSpan barSize);

        Aroon Aroon(int period, DataStream stream);

        AroonOscillator AroonOscillator(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> computeLow, Func<Bar, decimal> computeHigh);

        AroonOscillator AroonOscillator(int period, DataStream stream, TimeSpan barSize);

        AroonOscillator AroonOscillator(int period, DataStream stream);

        AverageDirectionalIndex AverageDirectionalIndex(int period, DataStream stream);

        AverageDirectionalIndex AverageDirectionalIndex(int period, DataStream stream, TimeSpan barSize);

        AverageTrueRange AverageTrueRange(int period, DataStream stream);

        AverageTrueRange AverageTrueRange(int period, DataStream stream, TimeSpan barSize);

        BollingerBands BollingerBands(int period, double sdUp, double sdDown, DataStream stream);

        BollingerBands BollingerBands(int period, double sdUp, double sdDown, DataStream stream, TimeSpan barSize);

        BollingerBands BollingerBands(int period, double sdUp, double sdDown, DataStream stream, TimeSpan barSize, MovingAverageType maType);

        BollingerBands BollingerBands(int period, double sdUp, double sdDown, DataStream stream, TimeSpan barSize, MovingAverageType maType, Func<Bar, decimal> compute);

        ChandeMomentumOscillator ChandeMomentumOscillator(int period, DataStream stream);

        ChandeMomentumOscillator ChandeMomentumOscillator(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null);

        CommodityChannelIndex CommodityChannelIndex(int period, DataStream stream);

        CommodityChannelIndex CommodityChannelIndex(int period, DataStream stream, TimeSpan barSize);

        ExponentialMovingAverage ExponentialMovingAverage(int period, DataStream stream);

        ExponentialMovingAverage ExponentialMovingAverage(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null);

        Momentum Momentum(int period, DataStream stream);

        Momentum Momentum(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null);

        MovingAverage MovingAverage(int period, MovingAverageType maType, DataStream stream);

        MovingAverage MovingAverage(int period, MovingAverageType maType, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null);

        MovingAverageConDiv MovingAverageConDiv(int fastPeriod, int slowPeriod, int signalPeriod, TimeSpan barSize, DataStream stream, Func<Bar, decimal> comp = null);

        MovingAverageConDiv MovingAverageConDiv(int fastPeriod, int slowPeriod, int signalPeriod, DataStream stream);

        ParabolicSAR ParabolicSAR(int period, double accelerator, double maximum, DataStream stream);

        ParabolicSAR ParabolicSAR(int period, double accelerator, double maximum, DataStream stream, TimeSpan barSize);

        ParabolicSAR ParabolicSAR(int period, double accelerator, double maximum, DataStream stream, TimeSpan barSize, Func<Bar, decimal> calcHigh, Func<Bar, decimal> calcLow);

        RateOfChange RateOfChange(int period, DataStream stream);

        RateOfChange RateOfChange(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null);

        RelativeStrengthIndex RelativeStrengthIndex(int period, DataStream stream);

        RelativeStrengthIndex RelativeStrengthIndex(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null);

        SimpleMovingAverage SimpleMovingAverage(int period, DataStream stream);

        SimpleMovingAverage SimpleMovingAverage(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null);

        TrueRange TrueRange(DataStream stream);

        TrueRange TrueRange(DataStream stream, TimeSpan barSize);

        WeightedMovingAverage WeightedMovingAverage(int period, DataStream stream);

        WeightedMovingAverage WeightedMovingAverage(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null);

        WilliamsR WilliamsR(int period, DataStream stream);

        WilliamsR WilliamsR(int period, DataStream stream, TimeSpan barSize);

        #endregion Public Methods
    }
}