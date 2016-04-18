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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantler.Indicators
{
    /// <summary>
    /// Calculates the average true range for a period of time
    /// http://stockcharts.com/school/doku.php?st=average+true+range&id=chart_school:technical_indicators:average_true_range_atr
    /// </summary>
    public class AverageTrueRange : IndicatorBase, Interfaces.Indicators.AverageTrueRange
    {
        #region Private Fields

        private readonly List<decimal> _close = new List<decimal>();
        private readonly List<decimal> _high = new List<decimal>();
        private readonly List<decimal> _low = new List<decimal>();
        private readonly List<decimal> _truerange = new List<decimal>();
        private readonly IndicatorDataSerie _result = new IndicatorDataSerie();
        private TimeSpan _timeSpan;

        #endregion Private Fields

        #region Public Constructors

        public AverageTrueRange(int period, TimeSpan barSize, DataStream stream)
        {
            Construct(period, barSize, stream);
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
            _high.Insert(0, bar.High);
            _low.Insert(0, bar.Low);
            _close.Insert(0, bar.Close);

            //Clean up old values
            Cleanup();

            //Check if we have enough values
            if (_close.Count == 1)
            {
                _truerange.Insert(0, bar.High - bar.Low);
                return;
            }

            //Calculate the indicator
            //Get true range
            decimal hl = bar.High - bar.Low;
            decimal Hcp = Math.Abs(bar.High - _close[1]);
            decimal Lcp = Math.Abs(bar.Low - _close[1]);
            decimal tr = Math.Max(Math.Max(hl, Hcp), Lcp);

            _truerange.Insert(0, tr);

            //Can we start calculations?
            if (_truerange.Count < Period)
                return;

            //Add to results
            var calced = _truerange.Count == Period ? _truerange.Average() : (Result[0] * (Period - 1) + tr) / Period;

            //Add to current values
            if (calced != 0)
                Result[0] = calced;
        }

        #endregion Public Methods

        #region Private Methods

        private void Cleanup()
        {
            if (_close.Count > Period * 3)
            {
                _close.RemoveRange(Period * 3, _close.Count - (Period * 3));
                _high.RemoveRange(Period * 3, _high.Count - (Period * 3));
                _low.RemoveRange(Period * 3, _low.Count - (Period * 3));
                _truerange.RemoveRange(Period * 3, _low.Count - (Period * 3));
            }
        }

        private void Construct(int period, TimeSpan barSize, DataStream stream)
        {
            _timeSpan = barSize;
            Period = period;
            BarSize = barSize;
            DataStreams = new[] { stream };
        }

        #endregion Private Methods
    }
}