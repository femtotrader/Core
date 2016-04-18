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

using Quantler.Tests.Common.Research;
using System;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestRandomSymbol
    {
        #region Public Constructors

        public TestRandomSymbol()
        {
        }

        #endregion Public Constructors

        #region Public Methods

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Basics()
        {
            string[] syms = RandomSymbol.GetSymbols((int)DateTime.Now.Ticks, 4, 100);
            bool v = true;

            foreach (string sym in syms)
            {
                bool bv = v;
                v &= (sym.Length > 0) && (System.Text.RegularExpressions.Regex.Replace(sym, "[a-z]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Length == 0);
            }
            Assert.True(v);
        }

        #endregion Public Methods
    }
}