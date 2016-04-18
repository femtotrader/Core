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
using Quantler.Interfaces.Indicators;
using System;

namespace Quantler.Indicators
{
    public class StandardIndicators : IndicatorFactory
    {
        #region Private Fields

        private readonly Interfaces.IndicatorManager _manager;

        #endregion Private Fields

        #region Public Constructors

        public StandardIndicators(Interfaces.IndicatorManager manager)
        {
            _manager = manager;
        }

        #endregion Public Constructors

        #region Public Methods

        public Interfaces.Indicators.Aroon Aroon(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> computeLow, Func<Bar, decimal> computeHigh)
        {
            var ntemplate = new Aroon(period, barSize, stream, computeLow, computeHigh);
            return _manager.Subscribe<Aroon>(ntemplate);
        }

        public Interfaces.Indicators.Aroon Aroon(int period, DataStream stream, TimeSpan barSize)
        {
            var ntemplate = new Aroon(period, barSize, stream);
            return _manager.Subscribe<Aroon>(ntemplate);
        }

        public Interfaces.Indicators.Aroon Aroon(int period, DataStream stream)
        {
            var ntemplate = new Aroon(period, _manager.Agent.TimeFrame, stream);
            return _manager.Subscribe<Aroon>(ntemplate);
        }

        public Interfaces.Indicators.AroonOscillator AroonOscillator(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> computeLow, Func<Bar, decimal> computeHigh)
        {
            var ntemplate = new AroonOscillator(period, barSize, stream, computeLow, computeHigh);
            return _manager.Subscribe<AroonOscillator>(ntemplate);
        }

        public Interfaces.Indicators.AroonOscillator AroonOscillator(int period, DataStream stream, TimeSpan barSize)
        {
            var ntemplate = new AroonOscillator(period, barSize, stream);
            return _manager.Subscribe<AroonOscillator>(ntemplate);
        }

        public Interfaces.Indicators.AroonOscillator AroonOscillator(int period, DataStream stream)
        {
            var ntemplate = new AroonOscillator(period, _manager.Agent.TimeFrame, stream);
            return _manager.Subscribe<AroonOscillator>(ntemplate);
        }

        public Interfaces.Indicators.AverageTrueRange AverageTrueRange(int period, DataStream stream)
        {
            var ntemplate = new AverageTrueRange(period, _manager.Agent.TimeFrame, stream);
            return _manager.Subscribe<AverageTrueRange>(ntemplate);
        }

        public Interfaces.Indicators.AverageTrueRange AverageTrueRange(int period, DataStream stream, TimeSpan barSize)
        {
            var ntemplate = new AverageTrueRange(period, barSize, stream);
            return _manager.Subscribe<AverageTrueRange>(ntemplate);
        }

        //public Interfaces.Indicators.BalanceOfPower BalanceOfPower(DataStream stream)
        //{
        //    var ntemplate = new BalanceOfPower(stream);
        //    return Manager.Subscribe<BalanceOfPower>(ntemplate);
        //}

        //public Interfaces.Indicators.BalanceOfPower BalanceOfPower(DataStream stream, TimeSpan BarSize)
        //{
        //    var ntemplate = new BalanceOfPower(stream, BarSize);
        //    return Manager.Subscribe<BalanceOfPower>(ntemplate);
        //}

        public Interfaces.Indicators.BollingerBands BollingerBands(int period, double sdUp, double sdDown, DataStream stream)
        {
            var ntemplate = new BollingerBands(period, sdUp, sdDown, stream, _manager.Agent.TimeFrame);
            return _manager.Subscribe<BollingerBands>(ntemplate);
        }

        public Interfaces.Indicators.BollingerBands BollingerBands(int period, double sdUp, double sdDown, DataStream stream, TimeSpan barSize)
        {
            var ntemplate = new BollingerBands(period, sdUp, sdDown, stream, barSize);
            return _manager.Subscribe<BollingerBands>(ntemplate);
        }

        public Interfaces.Indicators.BollingerBands BollingerBands(int period, double sdUp, double sdDown, DataStream stream, TimeSpan barSize, MovingAverageType maType)
        {
            var ntemplate = new BollingerBands(period, sdUp, sdDown, stream, barSize, maType);
            return _manager.Subscribe<BollingerBands>(ntemplate);
        }

        public Interfaces.Indicators.BollingerBands BollingerBands(int period, double sdUp, double sdDown, DataStream stream, TimeSpan barSize, MovingAverageType maType, Func<Bar, decimal> compute)
        {
            var ntemplate = new BollingerBands(period, sdUp, sdDown, stream, barSize, maType, compute);
            return _manager.Subscribe<BollingerBands>(ntemplate);
        }

        public Interfaces.Indicators.ChandeMomentumOscillator ChandeMomentumOscillator(int period, DataStream stream)
        {
            var ntemplate = new ChandeMomentumOscillator(period, _manager.Agent.TimeFrame, stream);
            return _manager.Subscribe<ChandeMomentumOscillator>(ntemplate);
        }

        public Interfaces.Indicators.ChandeMomentumOscillator ChandeMomentumOscillator(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null)
        {
            var ntemplate = new ChandeMomentumOscillator(period, barSize, stream, comp);
            return _manager.Subscribe<ChandeMomentumOscillator>(ntemplate);
        }

        public Interfaces.Indicators.CommodityChannelIndex CommodityChannelIndex(int period, DataStream stream)
        {
            var ntemplate = new CommodityChannelIndex(period, _manager.Agent.TimeFrame, stream);
            return _manager.Subscribe<CommodityChannelIndex>(ntemplate);
        }

        public Interfaces.Indicators.CommodityChannelIndex CommodityChannelIndex(int period, DataStream stream, TimeSpan barSize)
        {
            var ntemplate = new CommodityChannelIndex(period, barSize, stream);
            return _manager.Subscribe<CommodityChannelIndex>(ntemplate);
        }

        public Interfaces.Indicators.ExponentialMovingAverage ExponentialMovingAverage(int period, DataStream stream)
        {
            var ntemplate = new ExponentialMovingAverage(period, _manager.Agent.TimeFrame, stream);
            return _manager.Subscribe<ExponentialMovingAverage>(ntemplate);
        }

        public Interfaces.Indicators.ExponentialMovingAverage ExponentialMovingAverage(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null)
        {
            var ntemplate = new ExponentialMovingAverage(period, barSize, stream, comp);
            return _manager.Subscribe<ExponentialMovingAverage>(ntemplate);
        }

        Interfaces.Indicators.AverageDirectionalIndex IndicatorFactory.AverageDirectionalIndex(int period, DataStream stream)
        {
            var ntemplate = new DirectionalIndex(period, stream, _manager.Agent.TimeFrame);
            return _manager.Subscribe<DirectionalIndex>(ntemplate);
        }

        Interfaces.Indicators.AverageDirectionalIndex IndicatorFactory.AverageDirectionalIndex(int period, DataStream stream, TimeSpan barSize)
        {
            var ntemplate = new DirectionalIndex(period, stream, barSize);
            return _manager.Subscribe<DirectionalIndex>(ntemplate);
        }

        public Interfaces.Indicators.Momentum Momentum(int period, DataStream stream)
        {
            var ntemplate = new Momentum(period, _manager.Agent.TimeFrame, stream);
            return _manager.Subscribe<Momentum>(ntemplate);
        }

        public Interfaces.Indicators.Momentum Momentum(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null)
        {
            var ntemplate = new Momentum(period, barSize, stream, comp);
            return _manager.Subscribe<Momentum>(ntemplate);
        }

        public Interfaces.Indicators.MovingAverage MovingAverage(int period, MovingAverageType maType, DataStream stream)
        {
            var ntemplate = new MovingAverage(period, maType, stream, _manager.Agent.TimeFrame);
            return _manager.Subscribe<MovingAverage>(ntemplate);
        }

        public Interfaces.Indicators.MovingAverage MovingAverage(int period, MovingAverageType maType, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null)
        {
            var ntemplate = new MovingAverage(period, barSize, maType, stream, comp);
            return _manager.Subscribe<MovingAverage>(ntemplate);
        }

        public Interfaces.Indicators.MovingAverageConDiv MovingAverageConDiv(int fastPeriod, int slowPeriod, int signalPeriod, TimeSpan barSize, DataStream stream, Func<Bar, decimal> comp = null)
        {
            var ntemplate = new MovingAverageConDiv(fastPeriod, slowPeriod, signalPeriod, barSize, stream, comp);
            return _manager.Subscribe<MovingAverageConDiv>(ntemplate);
        }

        public Interfaces.Indicators.MovingAverageConDiv MovingAverageConDiv(int fastPeriod, int slowPeriod, int signalPeriod, DataStream stream)
        {
            var ntemplate = new MovingAverageConDiv(fastPeriod, slowPeriod, signalPeriod, stream, _manager.Agent.TimeFrame);
            return _manager.Subscribe<MovingAverageConDiv>(ntemplate);
        }

        public ParabolicSAR ParabolicSAR(int period, double accelerator, double maximum, DataStream stream)
        {
            var ntemplate = new ParabolicSar(period, accelerator, maximum, stream, _manager.Agent.TimeFrame);
            return _manager.Subscribe<ParabolicSar>(ntemplate);
        }

        public ParabolicSAR ParabolicSAR(int period, double accelerator, double maximum, DataStream stream, TimeSpan barSize)
        {
            var ntemplate = new ParabolicSar(period, accelerator, maximum, stream, barSize);
            return _manager.Subscribe<ParabolicSar>(ntemplate);
        }

        public ParabolicSAR ParabolicSAR(int period, double accelerator, double maximum, DataStream stream, TimeSpan barSize, Func<Bar, decimal> calcHigh, Func<Bar, decimal> calcLow)
        {
            var ntemplate = new ParabolicSar(period, accelerator, maximum, stream, barSize, calcHigh, calcLow);
            return _manager.Subscribe<ParabolicSar>(ntemplate);
        }

        public Interfaces.Indicators.RateOfChange RateOfChange(int period, DataStream stream)
        {
            var ntemplate = new RateOfChange(period, _manager.Agent.TimeFrame, stream);
            return _manager.Subscribe<RateOfChange>(ntemplate);
        }

        public Interfaces.Indicators.RateOfChange RateOfChange(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null)
        {
            var ntemplate = new RateOfChange(period, barSize, stream, comp);
            return _manager.Subscribe<RateOfChange>(ntemplate);
        }

        public Interfaces.Indicators.RelativeStrengthIndex RelativeStrengthIndex(int period, DataStream stream)
        {
            var ntemplate = new RelativeStrengthIndex(period, stream, _manager.Agent.TimeFrame);
            return _manager.Subscribe<RelativeStrengthIndex>(ntemplate);
        }

        public Interfaces.Indicators.RelativeStrengthIndex RelativeStrengthIndex(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null)
        {
            var ntemplate = new RelativeStrengthIndex(period, stream, barSize, comp);
            return _manager.Subscribe<RelativeStrengthIndex>(ntemplate);
        }

        public Interfaces.Indicators.SimpleMovingAverage SimpleMovingAverage(int period, DataStream stream)
        {
            var ntemplate = new SimpleMovingAverage(period, _manager.Agent.TimeFrame, stream);
            return _manager.Subscribe<SimpleMovingAverage>(ntemplate);
        }

        public Interfaces.Indicators.SimpleMovingAverage SimpleMovingAverage(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null)
        {
            var ntemplate = new SimpleMovingAverage(period, barSize, stream, comp);
            return _manager.Subscribe<SimpleMovingAverage>(ntemplate);
        }

        public Interfaces.Indicators.TrueRange TrueRange(DataStream stream)
        {
            var ntemplate = new TrueRange(stream, _manager.Agent.TimeFrame);
            return _manager.Subscribe<TrueRange>(ntemplate);
        }

        public Interfaces.Indicators.TrueRange TrueRange(DataStream stream, TimeSpan barSize)
        {
            var ntemplate = new TrueRange(stream, barSize);
            return _manager.Subscribe<TrueRange>(ntemplate);
        }

        public Interfaces.Indicators.WeightedMovingAverage WeightedMovingAverage(int period, DataStream stream)
        {
            var ntemplate = new WeightedMovingAverage(period, _manager.Agent.TimeFrame, stream);
            return _manager.Subscribe<WeightedMovingAverage>(ntemplate);
        }

        public Interfaces.Indicators.WeightedMovingAverage WeightedMovingAverage(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null)
        {
            var ntemplate = new WeightedMovingAverage(period, barSize, stream, comp);
            return _manager.Subscribe<WeightedMovingAverage>(ntemplate);
        }

        public Interfaces.Indicators.WilliamsR WilliamsR(int period, DataStream stream)
        {
            var ntemplate = new WilliamsR(period, stream, _manager.Agent.TimeFrame);
            return _manager.Subscribe<WilliamsR>(ntemplate);
        }

        public Interfaces.Indicators.WilliamsR WilliamsR(int period, DataStream stream, TimeSpan barSize)
        {
            var ntemplate = new WilliamsR(period, stream, barSize);
            return _manager.Subscribe<WilliamsR>(ntemplate);
        }

        #endregion Public Methods
    }
}