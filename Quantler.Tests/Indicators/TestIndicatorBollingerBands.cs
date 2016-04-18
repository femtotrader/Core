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
    public class TestIndicatorBollingerBands
    {
        private readonly BollingerBands _sut;
        private const int Period = 14;

        /// <summary>
        /// Initialize test
        /// </summary>
        public TestIndicatorBollingerBands()
        {
            DataStream stream = new OHLCBarStream(new ForexSecurity("EURUSD"), BarInterval.FiveMin);
            _sut = new BollingerBands(Period, 2, 2, stream, stream.DefaultInterval);
        }

        [Fact]
        [Trait("Quantler.Indicator", "Quantler")]
        public void ComparisonTest()
        {
            Util.TestIndicator(_sut, "bob", new[] { "Lower", "Middle", "Upper" }, (indicator, results) =>
            {
                var lowerexpected = Math.Round(results[0], 5);
                var middleexpected = Math.Round(results[0], 5);
                var upperexpected = Math.Round(results[0], 5);

                Math.Round(indicator.Lower.CurrentValue, 5).Should().BeInRange(lowerexpected*.99M, lowerexpected*1.01M);
                Math.Round(indicator.Middle.CurrentValue, 5).Should().BeInRange(middleexpected * .99M, middleexpected * 1.01M);
                Math.Round(indicator.Upper.CurrentValue, 5).Should().BeInRange(upperexpected * .99M, upperexpected * 1.01M);
            });
        }
    }
}