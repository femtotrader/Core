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
using Quantler.Securities;

namespace Quantler.Tests.Indicators
{
    public class TestIndicatorAverageTrueRange
    {
        private readonly AverageTrueRange _sut;
        private const int Period = 14;

        /// <summary>
        /// Initialize test
        /// </summary>
        public TestIndicatorAverageTrueRange()
        {
            DataStream stream = new OHLCBarStream(new ForexSecurity("EURUSD"), BarInterval.FiveMin);
            _sut = new Quantler.Indicators.AverageTrueRange(Period, stream.DefaultInterval, stream);
        }

        [Fact]
        [Trait("Quantler.Indicator", "Quantler")]
        public void ComparisonTest()
        {
            Util.TestIndicator(_sut, "atr", new[] { "ATR" }, (indicator, results) =>
            {
                Math.Round(indicator.Result.CurrentValue, 5).Should().Be(results[0]);
            });
        }
    }
}