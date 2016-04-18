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
    public class Momentum : IndicatorBase, Interfaces.Indicators.Momentum
    {
        #region Private Fields

        private readonly List<double> _tavalues = new List<double>();
        private readonly IndicatorDataSerie _result = new IndicatorDataSerie();
        private readonly TaLib _ta = new TaLib();
        private TimeSpan _timeSpan;

        #endregion Private Fields

        #region Public Constructors

        public Momentum(int period, TimeSpan barSize, DataStream stream, Func<Bar, decimal> comp = null)
        {
            Construct(period, barSize, stream, comp);
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

            //Calculate the new value
            decimal current = Compute.Invoke(bar);

            //Add new value
            _tavalues.Insert(0, (double)current);

            //Clean up old values
            if (_tavalues.Count > Period * 3)
                _tavalues.RemoveRange(Period * 3, _tavalues.Count - (Period * 3));

            //Calculate the indicator
            var calced = _ta.Mom(_tavalues.ToArray(), Period);

            //Add to current values
            if (calced.IsValid)
                Result[0] = (decimal)calced.CurrentValue;
        }

        #endregion Public Methods

        #region Private Methods

        private void Construct(int period, TimeSpan barSize, DataStream stream, Func<Bar, decimal> comp = null)
        {
            _timeSpan = barSize;
            Period = period;
            BarSize = barSize;
            Compute = comp ?? (x => x.Close);
            DataStreams = new DataStream[] { stream };
        }

        #endregion Private Methods
    }
}