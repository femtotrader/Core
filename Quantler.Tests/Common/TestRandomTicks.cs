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

using Quantler.Interfaces;
using Quantler.Tests.Common.Research;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestRandomTicks
    {
        #region Public Constructors

        public TestRandomTicks()
        {
        }

        #endregion Public Constructors

        #region Public Methods

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Basics()
        {
            Tick[] ticks = RandomTicks.GenerateSymbol("TST", 1000);
            bool v = true;
            foreach (Tick k in ticks)
                v &= k.IsValid;
            Assert.True(v);
        }

        #endregion Public Methods
    }
}