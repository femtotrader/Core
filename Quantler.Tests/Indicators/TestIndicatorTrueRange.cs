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
    public class TestIndicatorTrueRange
    {
        private readonly TrueRange _sut;

        /// <summary>
        /// Initialize test
        /// </summary>
        public TestIndicatorTrueRange()
        {
            DataStream stream = new OHLCBarStream(new ForexSecurity("EURUSD"), BarInterval.FiveMin);
            _sut = new TrueRange(stream, stream.DefaultInterval);
        }

        [Fact]
        [Trait("Quantler.Indicator", "Quantler")]
        public void ComparisonTest()
        {
            Util.TestIndicator(_sut, "truerange", new[] { "Result" }, (indicator, results) =>
            {
                Math.Round(indicator.Result.CurrentValue, 5).Should().Be(Math.Round(results[0], 5));
            });
        }
    }
}