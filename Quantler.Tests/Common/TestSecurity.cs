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
using Quantler.Securities;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestSecurity
    {
        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Parsing()
        {
            // tests to parse and generate user-supplied security specifiers
            SecurityImpl nyse = new SecurityImpl("LVS");
            string p = nyse.ToString();

            SecurityImpl t = SecurityImpl.Parse(p);
            Assert.True(t.Name == nyse.Name, t.Name);
            Assert.True(!t.HasDest, t.DestEx);
            Assert.True(t.Type == nyse.Type, t.Type.ToString());

            SecurityImpl crude = SecurityImpl.Parse("CLV8 FUT GLOBEX");
            Assert.True(crude.Name == "CLV8", crude.Name);
            Assert.True(crude.HasDest, crude.DestEx);
            Assert.True(crude.Type == SecurityType.Future, crude.Type.ToString());
            SecurityImpl goog = SecurityImpl.Parse("GOOG");
            Assert.Equal("GOOG", goog.Name);
        }
    }
}