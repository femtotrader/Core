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
    public class WilliamsR : IndicatorBase, Interfaces.Indicators.WilliamsR
    {
        #region Private Fields

        private readonly List<double> _close = new List<double>();
        private readonly List<double> _high = new List<double>();
        private readonly List<double> _low = new List<double>();
        private readonly IndicatorDataSerie _result = new IndicatorDataSerie();
        private readonly TaLib _ta = new TaLib();
        private TimeSpan _timeSpan;

        #endregion Private Fields

        #region Public Constructors

        public WilliamsR(int period, DataStream stream, TimeSpan barSize)
        {
            Construct(period, stream, barSize);
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
            _high.Insert(0, (double)bar.High);
            _low.Insert(0, (double)bar.Low);
            _close.Insert(0, (double)bar.Close);

            //Clean up old values
            Cleanup();

            //Calculate the indicator
            var calced = _ta.WilliamsR(_high.ToArray(), _low.ToArray(), _close.ToArray(), Period);

            //Add to current values
            if (calced.IsValid)
                Result[0] = (decimal)calced.CurrentValue;
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
            }
        }

        private void Construct(int period, DataStream stream, TimeSpan barSize)
        {
            _timeSpan = barSize;
            BarSize = barSize;
            Period = period;
            DataStreams = new DataStream[] { stream };
        }

        #endregion Private Methods
    }
}