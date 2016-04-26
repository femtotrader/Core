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
using Quantler.TALib;
using System;
using System.Collections.Generic;

namespace Quantler.Indicators
{
    public class BollingerBands : IndicatorBase, Interfaces.Indicators.BollingerBands
    {
        #region Private Fields

        private readonly List<double> _input = new List<double>();
        private MovingAverageType _maType;
        private double _sdUp, _sdDown;
        private readonly TaLib _ta = new TaLib();
        private TimeSpan _timeSpan;
        private IndicatorDataSerie _upperBand, _middleBand, _lowerBand;

        #endregion Private Fields

        #region Public Constructors

        public BollingerBands(int period, double sdUp, double sdDown, DataStream stream, TimeSpan barSize)
        {
            Construct(period, stream, barSize, sdUp, sdDown, null, MovingAverageType.Simple);
        }

        public BollingerBands(int period, double sdUp, double sdDown, DataStream stream, TimeSpan barSize, MovingAverageType maType)
        {
            Construct(period, stream, barSize, sdUp, sdDown, null, maType);
        }

        public BollingerBands(int period, double sdUp, double sdDown, DataStream stream, TimeSpan barSize, MovingAverageType maType, Func<Bar, decimal> compute)
        {
            Construct(period, stream, barSize, sdUp, sdDown, compute, maType);
        }

        #endregion Public Constructors

        #region Public Properties

        public new bool IsReady
        {
            get { return (_upperBand.Count > 0 && _upperBand[0] != 0); }
        }

        public DataSerie Lower
        {
            get { return _lowerBand; }
        }

        public DataSerie Middle
        {
            get { return _middleBand; }
        }

        public DataSerie Upper
        {
            get { return _upperBand; }
        }

        #endregion Public Properties

        #region Public Methods

        public override void OnBar(Bar bar)
        {
            //Check for correct interval
            if (bar.CustomInterval != (int)_timeSpan.TotalSeconds)
                return;

            //Add new values
            _input.Add((double)Compute.Invoke(bar));

            //Clean up old values
            Cleanup();

            //Calculate the indicator
            var calced = _ta.Bbands(_input.ToArray(), _sdUp, _sdDown, Period, (int)_maType);

            //Add to current values
            if (calced.IsValid)
            {
                _upperBand[0] = (decimal)calced.UpperBand[calced.Index - 1];
                _lowerBand[0] = (decimal)calced.LowerBand[calced.Index - 1];
                _middleBand[0] = (decimal)calced.MiddleBand[calced.Index - 1];
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void Cleanup()
        {
            if (_input.Count > Period * 3)
            {
                _input.RemoveRange(0, Period);
            }
        }

        private void Construct(int period, DataStream stream, TimeSpan barSize, double sdUp, double sdDown, Func<Bar, decimal> compute, MovingAverageType maType)
        {
            _timeSpan = barSize;
            Period = period;
            DataStreams = new[] { stream };
            _maType = maType;
            Compute = compute ?? (x => (x.Close));
            _sdUp = sdUp;
            _sdDown = sdDown;
            Period = sdUp > sdDown ? (int)sdUp : (int)sdDown;
            _upperBand = new IndicatorDataSerie();
            _middleBand = new IndicatorDataSerie();
            _lowerBand = new IndicatorDataSerie();
        }

        #endregion Private Methods
    }
}