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

using Quantler.Interfaces;
using System;

namespace Quantler.Templates
{
    public abstract class IndicatorTemplate : Template, Indicator
    {
        #region Private Fields

        private Indicators.IndicatorDataSerie _result = new Indicators.IndicatorDataSerie();

        #endregion Private Fields

        #region Public Properties

        public TimeSpan BarSize { get; protected set; }

        public Func<Bar, decimal> Compute { get; protected set; }

        public DataStream[] DataStreams
        {
            get;
            protected set;
        }

        public abstract bool IsReady { get; }

        public int Period { get; protected set; }

        public Indicators.IndicatorDataSerie Result { get { return _result; } set { _result = value; } }

        public string Symbol { get; protected set; }

        #endregion Public Properties

        #region Public Methods

        public abstract void OnBar(Bar bar);

        #endregion Public Methods
    }
}