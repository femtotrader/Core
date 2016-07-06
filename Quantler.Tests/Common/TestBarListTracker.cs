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

using Quantler.Data.Ticks;
using Quantler.Interfaces;
using Quantler.Tracker;
using System.Collections.Generic;
using Quantler.Securities;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestBarListTracker
    {
        #region Private Fields

        private List<string> syms = new List<string>();

        #endregion Private Fields

        #region Public Constructors

        public TestBarListTracker()
        {
        }

        #endregion Public Constructors

        #region Public Methods

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void AddIntervalWhileRunning()
        {
            OHLCBarStream blt = new OHLCBarStream(new ForexSecurity("TST"));
            blt.AddInterval(BarInterval.Minute);
            blt.AddInterval(BarInterval.FiveMin);
            blt.GotNewBar += new SymBarIntervalDelegate(blt_GotNewBar);

            Tick[] tape = TestBarList.SampleData();
            blt.Initialize();

            // add ticks from tape to tracker
            for (int i = 0; i < tape.Length; i++)
            {
                blt.GotTick(tape[i]);
                if (i == 1)
                    blt.AddInterval(120);
            }

            //make sure we got one symbol as bar events
            Assert.Equal(1, syms.Count);

            // make sure our symbols matched barlist count
            Assert.Equal(blt.SymbolCount, syms.Count);

            // make sure same on individual bars
            Assert.True(blt[120].Count > 0);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void MultipleIntervalsRequests()
        {
            OHLCBarStream blt = new OHLCBarStream(new ForexSecurity("TST"));
            blt.AddInterval(BarInterval.Minute);
            blt.AddInterval(BarInterval.FiveMin);
            blt.GotNewBar += new SymBarIntervalDelegate(blt_GotNewBar);

            Tick[] tape = TestBarList.SampleData();
            blt.Initialize();

            // add ticks from tape to tracker
            for (int i = 0; i < tape.Length; i++)
            {
                blt.GotTick(tape[i]);
                if (i == 1)
                    blt.AddInterval(120);
            }

            // Check if the data differs per interval
            Assert.NotEqual(blt[BarInterval.Minute][-2].Close, blt[BarInterval.FiveMin][-2].Close);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void IntervalRequestDuringRun()
        {
            OHLCBarStream blt = new OHLCBarStream(new ForexSecurity("TST"));
            blt.AddInterval(BarInterval.FiveMin);
            blt.GotNewBar += new SymBarIntervalDelegate(blt_GotNewBar);

            Tick[] tape = TestBarList.SampleData();
            blt.Initialize();

            // add ticks from tape to tracker
            for (int i = 0; i < tape.Length; i++)
            {
                blt.GotTick(tape[i]);
                var value = blt[BarInterval.Minute].RecentBar;
            }

            // Check if the data differs per interval
            Assert.NotEqual(blt[BarInterval.Minute][-2].Close, blt[BarInterval.FiveMin][-2].Close);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void DefaultInt()
        {
            BarListTrackerImpl blt = new BarListTrackerImpl();
            blt.GotNewBar += new SymBarIntervalDelegate(blt_GotNewBar);

            Tick[] tape = TestBarList.SampleData();
            // get second tape and change symbol
            Tick[] tape2 = new Tick[tape.Length];
            for (int i = 0; i < tape2.Length; i++)
            {
                TickImpl t = (TickImpl)tape[i];
                t.Symbol = "TST2";
                tape2[i] = t;
            }

            // add ticks from both tape to tracker
            for (int i = 0; i < tape.Length; i++)
            {
                blt.NewTick(tape[i]);
                blt.NewTick(tape2[i]);
            }

            //make sure we got two symbols as bar events
            Assert.Equal(2, syms.Count);
            // make sure our symbols matched barlist count
            Assert.Equal(blt.SymbolCount, syms.Count);

            int secondcount = 0;
            string symstring = string.Empty;
            foreach (string sym in blt)
            {
                secondcount++;
                symstring += sym;
            }

            // make sure enumeration equals symbol count
            Assert.Equal(syms.Count, secondcount);
            // make sure symbols are there
            Assert.True(symstring.Contains("TST") && symstring.Contains("TST2"));

            // change default interval
            blt.DefaultInterval = BarInterval.Minute;
            // make sure same on individual bars
            Assert.Equal(blt.DefaultInterval, blt["TST"].DefaultInterval);

            Assert.Equal(9, blt["TST"].IntervalCount(BarInterval.Minute));
            Assert.Equal(3, blt["TST"].IntervalCount(BarInterval.FiveMin));
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void TickInt()
        {
            Tick[] tape = TestBarList.SampleData();
            BarListTrackerImpl blt = new BarListTrackerImpl(new[] { 3 }, new BarInterval[] { BarInterval.CustomTicks });

            foreach (Tick k in tape)
                blt.NewTick(k);

            Assert.Equal(4, blt[tape[0].Symbol].Count);
        }

        #endregion Public Methods

        #region Private Methods

        private void blt_GotNewBar(string symbol, int interval)
        {
            if (!syms.Contains(symbol))
                syms.Add(symbol);
        }

        #endregion Private Methods
    }
}