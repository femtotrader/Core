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
    public class AroonOscillator : IndicatorBase, Interfaces.Indicators.AroonOscillator
    {
        #region Private Fields

        private readonly List<double> _high = new List<double>();
        private readonly List<double> _low = new List<double>();
        private readonly IndicatorDataSerie _result = new IndicatorDataSerie();
        private readonly TaLib _ta = new TaLib();
        private TimeSpan _timeSpan;

        #endregion Private Fields

        #region Public Constructors

        public AroonOscillator(int period, TimeSpan barSize, DataStream stream, Func<Bar, decimal> computeLow, Func<Bar, decimal> computeHigh)
        {
            Construct(period, barSize, stream, computeLow, computeHigh);
        }

        public AroonOscillator(int period, TimeSpan barSize, DataStream stream)
        {
            Construct(period, barSize, stream, null, null);
        }

        #endregion Public Constructors

        #region Public Properties

        public Func<Bar, decimal> ComputeHigh { get; set; }
        public Func<Bar, decimal> ComputeLow { get { return Compute; } set { Compute = value; } }

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

            //Calculate the new value
            decimal currentHigh = ComputeHigh.Invoke(bar);
            decimal currentLow = ComputeLow.Invoke(bar);

            //Add new values
            _high.Insert(0, (double)currentHigh);
            _low.Insert(0, (double)currentLow);

            //Clean up old values
            Cleanup();

            //Calculate the indicator
            var calced = _ta.AroonOsc(_high.ToArray(), _low.ToArray(), Period);

            //Add to current values
            if (calced.IsValid)
                _result[0] = (decimal)calced.Result[0];
        }

        #endregion Public Methods

        #region Private Methods

        private void Cleanup()
        {
            if (_high.Count > Period * 3)
            {
                _high.RemoveRange(Period * 3, _high.Count - (Period * 3));
                _low.RemoveRange(Period * 3, _low.Count - (Period * 3));
            }
        }

        private void Construct(int period, TimeSpan barSize, DataStream stream, Func<Bar, decimal> computeLow, Func<Bar, decimal> computeHigh)
        {
            _timeSpan = barSize;
            Period = period;
            BarSize = barSize;
            DataStreams = new DataStream[] { stream };
            Compute = computeLow ?? (x => x.Low);
            ComputeHigh = ComputeHigh ?? (x => x.High);
        }

        #endregion Private Methods
    }
}