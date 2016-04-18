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

using Quantler.Data.TikFile;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestTickFileFilter
    {
        #region Private Fields

        private readonly string[] _filenames = {
            "GM20070601.EPF",
            "SPX20070601.IDX",
            "LVS20080101.EPF",
            "GOOG20070926.EPF",
            "GM20090101.EPF",
        };

        #endregion Private Fields

        #region Public Methods

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void AndTest()
        {
            // build a filter with two stocks
            TickFileFilter tff = new TickFileFilter(new[] { "GM", "SPX" });
            // add date file for year
            tff.DateFilter(20070000, DateMatchType.Year);
            // add another date filter for month
            tff.DateFilter(600, DateMatchType.Month);
            // set DateFilter to AND/intersection
            tff.IsDateMatchUnion = false;
            // make sure three stocks match
            string[] result = tff.Allows(_filenames);
            Assert.Equal(3, result.Length);
            // set more exclusive filter
            tff.IsSymbolDateMatchUnion = false;
            // make sure two stocks match
            result = tff.Allows(_filenames);
            Assert.Equal(2, result.Length);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Basics()
        {
            // build a one symbol filter
            TickFileFilter tff = new TickFileFilter(new[] { "GM" });
            // get results from above data files
            string[] result = tff.Allows(_filenames);
            // make sure both files for this symbol match
            Assert.Equal(2, result.Length);
            // make sure the actual file names are the same
            Assert.Equal(result[0], _filenames[0]);
            Assert.Equal(result[1], _filenames[4]);
            // build a new filter
            tff = new TickFileFilter();
            // request all matching files for a given year
            tff.DateFilter(20070000, DateMatchType.Year);
            tff.IsDateMatchUnion = true;
            tff.IsSymbolDateMatchUnion = true;
            // do the match
            result = tff.Allows(_filenames);
            // make sure we found 3 files from this year
            Assert.Equal(3, result.Length);
            // make sure the filename is the same
            Assert.Equal(_filenames[3], result[2]);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void SerializeDeserialize()
        {
            TickFileFilter tff = new TickFileFilter(new string[] { "IBM", "MHS", "T" });
            tff.DateFilter(20070000, DateMatchType.Year);
            tff.IsDateMatchUnion = false;
            tff.IsSymbolDateMatchUnion = false;
            string msg = TickFileFilter.Serialize(tff);

            TickFileFilter f2 = TickFileFilter.Deserialize(msg);

            string msg2 = TickFileFilter.Serialize(f2);

            Assert.Equal(msg, msg2);
            Assert.Equal(tff.IsDateMatchUnion, f2.IsDateMatchUnion);
            Assert.Equal(tff.IsSymbolDateMatchUnion, f2.IsDateMatchUnion);
        }

        #endregion Public Methods
    }
}