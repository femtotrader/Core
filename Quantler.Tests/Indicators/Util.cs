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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using FluentAssertions;
using Quantler.Data.Bars;
using Quantler.Interfaces;

namespace Quantler.Tests.Indicators
{
    internal static class Util
    {
        /// <summary>
        /// Tests an indicator by going through the values of the indicator and checking these values from a static file
        /// </summary>
        /// <param name="indicator"></param>
        /// <param name="filename"></param>
        /// <param name="targetColumns"></param>
        /// <param name="valuecheck"></param>
        public static void TestIndicator<T>(T indicator, string filename, string[] targetColumns, Action<T, decimal[]> valuecheck)
            where T : Indicator
        {
            bool first = true;
            List<int> targetindexes = new List<int>();

            //Check and get file
            FileInfo file = new FileInfo(Directory.GetCurrentDirectory() + @"\Indicators\ResultFiles\" + filename + ".csv");

            foreach (string[] parts in File.ReadLines(file.FullName).Select(line => line.Split(new[] { ';' }, StringSplitOptions.None)))
            {
                if (first)
                {
                    first = false;
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (targetColumns.Contains(parts[i].Trim()))
                        {
                            targetindexes.Add(i);
                        }
                    }

                    //Check for indexes to target
                    targetindexes.Count.Should().BeGreaterThan(0);

                    continue;
                }

                DateTime dt = DateTime.ParseExact(parts[0], "dd-M-yyyy HH:mm", CultureInfo.CurrentCulture);

                //Add bar data to indicator
                indicator.OnBar(new BarImpl
                    (
                    decimal.Parse(parts[1], CultureInfo.GetCultureInfo("en-US")),
                    decimal.Parse(parts[2], CultureInfo.GetCultureInfo("en-US")),
                    decimal.Parse(parts[3], CultureInfo.GetCultureInfo("en-US")),
                    decimal.Parse(parts[4], CultureInfo.GetCultureInfo("en-US")),
                    0,
                    Quantler.Util.ToQLDate(dt),
                    Quantler.Util.ToQLTime(dt),
                    "EURUSD",
                    300
                    ));

                //Check if indicator is ready
                if (!indicator.IsReady || parts[targetindexes.First()].Trim() == string.Empty)
                    continue;

                //Check values
                valuecheck.Invoke(indicator, targetindexes.Select(t => decimal.Parse(parts[t], CultureInfo.GetCultureInfo("en-US"))).ToArray());
            }
        }
    }
}
