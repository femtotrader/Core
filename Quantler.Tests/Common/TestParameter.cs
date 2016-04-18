#region License
/*
Copyright Quantler BV, based on original code copyright Tradelink.org. 
This file is released under the GNU Lesser General Public License v3. http://www.gnu.org/copyleft/lgpl.html


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

using FluentAssertions;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestParameter
    {
        #region Public Methods

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Basics()
        {
            //Arrange
            int max = 100;
            int min = 10;
            int inc = 10;
            string name = "Hello World";
            Parameter sut = new Parameter(min, max, inc, name);

            //Act
            sut.ParameterValue = max * 2;

            //Assert
            sut.ParameterValue.Should().Be(max);
            sut.ParameterMin.Should().Be(min);
            sut.ParameterMax.Should().Be(max);
            sut.ParameterName.Should().Be(name);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void NegativeValues()
        {
            //Arrange
            int max = 100;
            Parameter sut = new Parameter(-100, -max, -10, "Testing");

            //Act
            sut.ParameterValue = max * 2;

            //Assert
            sut.ParameterValue.Should().Be(max);
        }

        #endregion Public Methods
    }
}