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

using System;
using FluentAssertions;
using Quantler.Indicators;
using Quantler.Interfaces;
using Xunit;
using Quantler.Securities;

namespace Quantler.Tests.Indicators
{
    public class TestIndicatorMovingAverageConDiv
    {
        private readonly MovingAverageConDiv _sut;
        private const int Period = 14;

        /// <summary>
        /// Initialize test
        /// </summary>
        public TestIndicatorMovingAverageConDiv()
        {
            DataStream stream = new OHLCBarStream(new ForexSecurity("EURUSD"), BarInterval.FiveMin);
            _sut = new MovingAverageConDiv(Period / 2, Period * 2, Period, stream, stream.DefaultInterval);
        }

        [Fact]
        [Trait("Quantler.Indicator", "Quantler")]
        public void ComparisonTest()
        {
            Util.TestIndicator(_sut, "condiv", new[] { "Line", "Histogram", "Signal" }, (indicator, results) =>
            {
                var LineExpected = Math.Abs(Math.Round(results[0], 5));
                var HistogramExpected = Math.Abs(Math.Round(results[1], 5));
                var SignalExpected = Math.Abs(Math.Round(results[2], 5));

                Math.Abs(Math.Round(indicator.Histogram.CurrentValue, 5)).Should().BeInRange(HistogramExpected * .99M, HistogramExpected * 1.01M);
                Math.Abs(Math.Round(indicator.Line.CurrentValue, 5)).Should().BeInRange(LineExpected * .99M, LineExpected * 1.01M);
                Math.Abs(Math.Round(indicator.Signal.CurrentValue, 5)).Should().BeInRange(SignalExpected * .99M, SignalExpected * 1.01M);
            });
        }
    }
}