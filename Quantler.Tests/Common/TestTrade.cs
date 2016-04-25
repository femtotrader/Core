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

using Quantler.Trades;
using System;
using FluentAssertions;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestTrade
    {
        #region Public Methods

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Construction()
        {
            TradeImpl t = new TradeImpl("TST", 10, 100, DateTime.Now);
            t.IsValid.Should().BeTrue(t.ToString());
            t.IsFilled.Should().BeTrue(t.ToString());

            //midnight check
            t.Xdate = 20081205;
            t.Xtime = 0;
            t.IsValid.Should().BeTrue();
            t.Xtime = 0;
            t.Xdate = 0;
            t.IsValid.Should().BeFalse();
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Defaults()
        {
            TradeImpl t = new TradeImpl();
            t.IsValid.Should().BeFalse(t.ToString());
            t.IsFilled.Should().BeFalse(t.ToString());
        }

        #endregion Public Methods
    }
}