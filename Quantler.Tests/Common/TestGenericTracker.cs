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
using Quantler.Tracker;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestGenericTracker
    {
        #region Private Fields

        private GenericTrackerImpl<decimal> price1 = new GenericTrackerImpl<decimal>();

        private GenericTrackerImpl<decimal> price2 = new GenericTrackerImpl<decimal>();

        private GenericTrackerImpl<bool> special = new GenericTrackerImpl<bool>();

        // track something
        private GenericTrackerImpl<bool> sym = new GenericTrackerImpl<bool>();

        #endregion Private Fields

        #region Public Methods

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Basics()
        {
            // track something
            GenericTrackerImpl<object> gt = new GenericTrackerImpl<object>();
            // get some symbols
            string[] syms = new string[] { "IBM", "LVS", "IBM", "WAG", "GOOG" };
            // add them
            foreach (string sym in syms)
                gt.addindex(sym, sym == "IBM" ? null : new object());
            // ensure we have them
            //Assert.Equal(4, newtxt);
            //Assert.NotEqual(gt.Count, syms.Length);
            // test fetching by label
            Assert.Null(gt["IBM"]);
            Assert.NotNull(gt["GOOG"]);
            Assert.Equal(0, gt.getindex("IBM"));
            // get label from index
            Assert.Equal("GOOG", gt.getlabel(3));
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void TrackedTypes()
        {
            // create generic tracker for strings
            var gt = new GenericTrackerImpl<string>();
            // verify tracked type
            Assert.True(gt.TrackedType == typeof(string), "wrong type");
            // test other common types
            var gt1 = new GenericTrackerImpl<bool>();
            // verify tracked type
            Assert.True(gt1.TrackedType == typeof(bool), "wrong type");

            var gt2 = new GenericTrackerImpl<decimal>();
            // verify tracked type
            Assert.True(gt2.TrackedType == typeof(decimal), "wrong type");
            var gt3 = new GenericTrackerImpl<int>();
            // verify tracked type
            Assert.True(gt3.TrackedType == typeof(int), "wrong type");

            var gt4 = new GenericTrackerImpl<long>();
            // verify tracked type
            Assert.True(gt4.TrackedType == typeof(long), "wrong type");

            var gt5 = new GenericTrackerImpl<object>();
            // verify tracked type
            Assert.True(gt5.TrackedType == typeof(object), "wrong type");

            var gt6 = new GenericTrackerImpl<Order>();
            // verify tracked type
            Assert.True(gt6.TrackedType == typeof(Order), "wrong type");
        }

        #endregion Public Methods
    }
}