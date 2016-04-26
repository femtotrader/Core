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
using Quantler.TALib;
using System;
using System.Collections.Generic;

namespace Quantler.Indicators
{
    public class ParabolicSar : IndicatorBase, Interfaces.Indicators.ParabolicSAR
    {
        #region Private Fields

        private readonly List<double> _high = new List<double>();
        private readonly List<double> _low = new List<double>();
        private double _accelerator, _maximum = 0;
        private Func<Bar, decimal> _calcHigh;
        private Func<Bar, decimal> _calcLow;
        private readonly IndicatorDataSerie _result = new IndicatorDataSerie();
        private readonly TaLib _ta = new TaLib();
        private TimeSpan _timeSpan;

        #endregion Private Fields

        #region Public Constructors

        public ParabolicSar(int period, double accelerator, double maximum, DataStream stream, TimeSpan barSize)
        {
            Construct(period, accelerator, maximum, stream, barSize, null, null);
        }

        public ParabolicSar(int period, double accelerator, double maximum, DataStream stream, TimeSpan barSize, Func<Bar, decimal> CalcHigh, Func<Bar, decimal> CalcLow)
        {
            Construct(period, accelerator, maximum, stream, barSize, _calcHigh, _calcLow);
        }

        #endregion Public Constructors

        #region Public Properties

        public new bool IsReady
        {
            get { return (Result.Count > 0 && Result[0] != 0); }
        }

        public DataSerie Result
        {
            get { return _result; }
        }

        #endregion Public Properties

        #region Public Methods

        public override void OnBar(Bar bar)
        {
            //Check for correct interval
            if (bar.CustomInterval != (int)_timeSpan.TotalSeconds)
                return;

            //Add new values
            _high.Add((double)_calcHigh.Invoke(bar));
            _low.Add((double)_calcLow.Invoke(bar));

            //Clean up old values
            Cleanup();

            //Calculate the indicator
            var calced = _ta.Sar(_high.ToArray(), _low.ToArray(), _accelerator, _maximum);

            //Add to current values
            if (calced.IsValid)
                Result[0] = (decimal) calced.CurrentValue;
        }

        #endregion Public Methods

        #region Private Methods

        private void Cleanup()
        {
            if (_high.Count > Period * 3)
            {
                _high.RemoveRange(0, Period);
                _low.RemoveRange(0, Period);
            }
        }

        private void Construct(int period, double accelerator, double maximum, DataStream stream, TimeSpan barSize, Func<Bar, decimal> CalcHigh, Func<Bar, decimal> CalcLow)
        {
            _timeSpan = barSize;
            BarSize = barSize;
            DataStreams = new[] { stream };
            _accelerator = accelerator;
            _maximum = maximum;
            _calcHigh = CalcHigh ?? (x => x.High);
            _calcLow = CalcLow ?? (x => x.Low);
            Period = period;
        }

        #endregion Private Methods
    }
}