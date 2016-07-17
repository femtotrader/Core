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
    public class MovingAverageConDiv : IndicatorBase, Interfaces.Indicators.MovingAverageConDiv
    {
        #region Private Fields

        private IndicatorDataSerie _histogram, _line, _signal;
        private readonly List<double> _tavalues = new List<double>();
        private readonly TaLib _ta = new TaLib();
        private TimeSpan _timeSpan;

        #endregion Private Fields

        #region Public Constructors

        public MovingAverageConDiv(int fastPeriod, int slowPeriod, int signalPeriod, TimeSpan barSize, DataStream stream, Func<Bar, decimal> comp = null)
        {
            Construct(fastPeriod, slowPeriod, signalPeriod, barSize, stream, comp);
        }

        public MovingAverageConDiv(int fastPeriod, int slowPeriod, int signalPeriod, DataStream stream, TimeSpan barSize)
        {
            Construct(fastPeriod, slowPeriod, signalPeriod, barSize, stream);
        }

        #endregion Public Constructors

        #region Public Properties

        public DataSerie Histogram
        {
            get { return _histogram; }
        }

        public new bool IsReady
        {
            get { return (_line.Count > 0 && _line[0] != 0); }
        }

        public DataSerie Line
        {
            get { return _line; }
        }

        public DataSerie Signal
        {
            get { return _signal; }
        }

        #endregion Public Properties

        #region Private Properties

        private int FastPeriod { get; set; }
        private int SignalPeriod { get; set; }
        private int SlowPeriod { get { return Period; } set { Period = value; } }

        #endregion Private Properties

        #region Public Methods

        public override void OnBar(Bar bar)
        {
            //Check for correct interval
            if (bar.CustomInterval != (int)_timeSpan.TotalSeconds)
                return;

            //Calculate the new value
            decimal current = Compute.Invoke(bar);

            //Add new value
            _tavalues.Add((double)current);

            //Clean up old values
            if (_tavalues.Count > Period * 5)
                _tavalues.RemoveRange(0, Period);

            //Calculate the indicator
            var calced = _ta.Macd(_tavalues.ToArray(), FastPeriod, SlowPeriod, SignalPeriod);

            //Add to current values
            if (calced.IsValid)
            {
                _line[0] = (decimal)calced.MacdLine[calced.Index - 1];
                _histogram[0] = (decimal)calced.MacdHistogram[calced.Index - 1];
                _signal[0] = (decimal)calced.MacdSignal[calced.Index - 1];
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void Construct(int fastperiod, int slowperiod, int signalPeriod, TimeSpan barSize, DataStream stream, Func<Bar, decimal> comp = null)
        {
            _timeSpan = barSize;
            BarSize = barSize;
            Compute = comp ?? (x => x.Close);
            DataStreams = new[] { stream };
            SlowPeriod = slowperiod;
            FastPeriod = fastperiod;
            _histogram = new IndicatorDataSerie();
            _line = new IndicatorDataSerie();
            _signal = new IndicatorDataSerie();
            SignalPeriod = signalPeriod;
        }

        #endregion Private Methods
    }
}