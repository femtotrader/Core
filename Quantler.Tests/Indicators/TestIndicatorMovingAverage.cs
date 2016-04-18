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
using Quantler.Interfaces;
using Quantler.Interfaces.Indicators;
using Xunit;
using MovingAverage = Quantler.Indicators.MovingAverage;
using Quantler.Securities;

namespace Quantler.Tests.Indicators
{
    public class TestIndicatorMovingAverage
    {
        private readonly MovingAverage _sut;
        private const int Period = 14;

        /// <summary>
        /// Initialize test
        /// </summary>
        public TestIndicatorMovingAverage()
        {
            DataStream stream = new OHLCBarStream(new ForexSecurity("EURUSD"), BarInterval.FiveMin);
            _sut = new MovingAverage(Period, MovingAverageType.Weighted, stream, stream.DefaultInterval);
        }

        [Fact]
        [Trait("Quantler.Indicator", "Quantler")]
        public void ComparisonTest()
        {
            Util.TestIndicator(_sut, "wma", new[] { "Result" }, (indicator, results) =>
            {
                Math.Round(indicator.Result.CurrentValue, 5).Should().Be(Math.Round(results[0], 5));
            });
        }
    }
}