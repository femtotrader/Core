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

namespace Quantler.Indicators
{
    public class TrueRange : IndicatorBase, Interfaces.Indicators.TrueRange
    {
        #region Private Fields

        private readonly List<decimal> _close = new List<decimal>();
        private readonly List<decimal> _high = new List<decimal>();
        private readonly List<decimal> _low = new List<decimal>();
        private readonly IndicatorDataSerie _result = new IndicatorDataSerie();
        private TimeSpan _timeSpan;

        #endregion Private Fields

        #region Public Constructors

        public TrueRange(DataStream stream, TimeSpan barSize)
        {
            Construct(stream, barSize);
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
                Result[0] = bar.High - bar.Low;
                return;
            }

            //Calculate the indicator
            //Get true range
            decimal hl = bar.High - bar.Low;
            decimal Hcp = Math.Abs(bar.High - _close[1]);
            decimal Lcp = Math.Abs(bar.Low - _close[1]);
            decimal tr = Math.Max(Math.Max(hl, Hcp), Lcp);

            //Add to current values
            Result[0] = tr;
        }

        #endregion Public Methods

        #region Private Methods

        private void Cleanup()
        {
            if (_close.Count > Period * 3)
            {
                _close.RemoveRange(0, Period);
                _high.RemoveRange(0, Period);
                _low.RemoveRange(0, Period);
            }
        }

        private void Construct(DataStream stream, TimeSpan barSize)
        {
            _timeSpan = barSize;
            BarSize = barSize;
            DataStreams = new[] { stream };
            Period = 1;
        }

        #endregion Private Methods
    }
}