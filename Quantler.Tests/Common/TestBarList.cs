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

using Quantler.Data.Bars;
using Quantler.Data.Ticks;
using Quantler.Data.TikFile;
using Quantler.Interfaces;
using Quantler.Tracker;
using System;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestBarList
    {
        #region Private Fields

        private const string Sym = "TST";

        private readonly BarListTrackerImpl _dailyBarTracker;

        private readonly BarListTrackerImpl _intraBarTracker;

        private readonly string[] _spyDaily = {  // verified from Yahoo! Finance
            "127.440000,127.740000,126.950000,127.430000,110183900,20110111,93000000,SPY,86400",
            "128.210000,128.720000,127.460000,128.580000,107436100,20110112,93000000,SPY,86400",
            "128.630000,128.690000,128.050000,128.370000,129048400,20110113,93000000,SPY,86400",
            "128.190000,129.330000,128.100000,129.300000,117611100,20110114,93000000,SPY,86400",
            "129.180000,129.640000,129.030000,129.520000,114249600,20110118,93000000,SPY,86400",

            "129.410000,129.540000,127.910000,128.250000,151709000,20110119,93000000,SPY,86400",
            "127.960000,128.400000,127.130000,128.080000,175511200,20110120,93000000,SPY,86400",
            "128.880000,129.170000,128.230000,128.370000,151377200,20110121,93000000,SPY,86400",
            "128.290000,129.250000,128.260000,129.100000,113647600,20110124,93000000,SPY,86400",
            "128.760000,129.280000,128.110000,129.170000,167388000,20110125,93000000,SPY,86400",

            "129.490000,130.050000,129.230000,129.670000,141139500,20110126,93000000,SPY,86400",
            "129.700000,130.210000,129.470000,129.990000,123206300,20110127,93000000,SPY,86400",
            "130.140000,130.350000,127.510000,127.720000,295569200,20110128,93000000,SPY,86400",
            "128.070000,128.780000,127.750000,128.680000,149126600,20110131,93000000,SPY,86400",
            "129.460000,130.970000,129.380000,130.740000,166962200,20110201,93000000,SPY,86400",

            "130.400000,130.840000,130.330000,130.490000,118163000,20110202,93000000,SPY,86400",
            "130.260000,130.980000,129.570000,130.780000,145726000,20110203,93000000,SPY,86400",
            "130.830000,131.200000,130.230000,131.150000,134584900,20110204,93000000,SPY,86400",
            "131.440000,132.400000,131.430000,131.970000,112330500,20110207,93000000,SPY,86400",
            "132.090000,132.640000,131.730000,132.570000, 98858300,20110208,93000000,SPY,86400"
        };

        private readonly string[] _spyIntra = {
            "132.200000,132.220000,132.200000,132.210000,108263,20110208,131000000,SPY,300",
            "132.205000,132.220000,132.140000,132.180000,596368,20110208,131500000,SPY,300",
            "132.180000,132.240000,132.160600,132.220000,504343,20110208,132000000,SPY,300",
            "132.229900,132.260000,132.220000,132.230000,301859,20110208,132500000,SPY,300",
            "132.240000,132.250000,132.210000,132.230000,173798,20110208,133000000,SPY,300",

            "132.225000,132.230000,132.200000,132.210000,251372,20110208,133500000,SPY,300",
            "132.205000,132.270000,132.205000,132.265000,151243,20110208,134000000,SPY,300",
            "132.260000,132.310000,132.250000,132.300000,338762,20110208,134500000,SPY,300",
            "132.290000,132.340000,132.270000,132.319000,570296,20110208,135000000,SPY,300",
            "132.300000,132.320000,132.290000,132.320000,183190,20110208,135500000,SPY,300",

            "132.320000,132.330000,132.290000,132.310000,235016,20110208,140000000,SPY,300",
            "132.310000,132.320000,132.290000,132.299900,117468,20110208,140500000,SPY,300",
            "132.300000,132.310000,132.280000,132.308000,199589,20110208,141000000,SPY,300",
            "132.300000,132.320000,132.290000,132.310000,319106,20110208,141500000,SPY,300",
            "132.300000,132.370000,132.300000,132.340000,576937,20110208,142000000,SPY,300",

            "132.330000,132.390000,132.330000,132.380000,320253,20110208,142500000,SPY,300",
            "132.385000,132.450000,132.380000,132.440000,881651,20110208,143000000,SPY,300",
            "132.430200,132.440000,132.390000,132.400000,281805,20110208,143500000,SPY,300",
            "132.400000,132.480000,132.400000,132.470000,624035,20110208,144000000,SPY,300",
            "132.462000,132.490000,132.430000,132.450000,785918,20110208,144500000,SPY,300"
        };

        private int _newbars;

        #endregion Private Fields

        #region Public Constructors

        public TestBarList()
        {
            _dailyBarTracker = new BarListTrackerImpl(BarInterval.Day);
            _intraBarTracker = new BarListTrackerImpl(BarInterval.FiveMin);
        }

        #endregion Public Constructors

        #region Public Methods

        public static Tick[] SampleData()
        {
            const int d = 20070517;
            const int t = 93500000;
            const string x = "NYSE";
            Tick[] tape = {
                TickImpl.NewTrade(Sym,d,t,10,100,x), // new on all intervals
                TickImpl.NewTrade(Sym,d,t+100000,10,100,x), // new on 1min
                TickImpl.NewTrade(Sym,d,t+200000,10,100,x),
                TickImpl.NewTrade(Sym,d,t+300000,10,100,x),
                TickImpl.NewTrade(Sym,d,t+400000,15,100,x),
                TickImpl.NewTrade(Sym,d,t+500000,16,100,x), //new on 5min
                TickImpl.NewTrade(Sym,d,t+600000,16,100,x),
                TickImpl.NewTrade(Sym,d,t+700000,10,100,x),
                TickImpl.NewTrade(Sym,d,t+710000,10,100,x),
                TickImpl.NewTrade(Sym,d,t+10000000,10,100,x), // new on hour interval
            };
            return tape;
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void AddDailyTicks()
        {
            SpoolDailyBars();

            TickImpl k = new TickImpl("SPY");
            k.Ask = 101;
            k.Bid = 100;
            k.Date = 20110208;
            k.Trade = 100.5m;
            k.Size = 5000;
            int baseTime = 144500;

            for (int x = 0; x < 20; x++)
            {
                k.Time = baseTime + x;
                _dailyBarTracker.NewTick(k);
            }

            Assert.Equal(_dailyBarTracker["SPY"].RecentBar.Close, k.Trade);
            DisplayTrackerContent(_dailyBarTracker["SPY"]);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void AddIntraTicks()
        {
            SpoolIntraBars();

            TickImpl k = new TickImpl("SPY");
            k.Ask = 101;
            k.Bid = 100;
            k.Date = 20110208;
            k.Trade = 100.5m;
            k.Size = 5000;
            int baseTime = 144500000;

            for (int x = 0; x < 20; x++)
            {
                k.Time = baseTime + (x * 1000);
                _intraBarTracker.NewTick(k);
            }

            Assert.Equal(_intraBarTracker["SPY"].RecentBar.Close, k.Trade);
            DisplayTrackerContent(_intraBarTracker["SPY"]);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void BarMath()
        {
            // get tickdata
            Tick[] tape = SampleData();

            // create bar
            BarListImpl bl = new BarListImpl(BarInterval.Minute);

            // pass ticks to bar
            foreach (Tick k in tape)
                bl.NewTick(k);

            // verify HH
            Assert.Equal(16, Calc.HH(bl));

            // verify LL
            Assert.Equal(10, Calc.LL(bl));

            // verify average
            Assert.Equal(11.888888888888888888888888889m, Calc.Avg(bl.Open()));
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void CustomInterval()
        {
            // request 5 second bars
            const int myinterval = 5;
            BarListImpl bl = new BarListImpl(Sym, myinterval);

            // verify custom interval
            Assert.Equal(BarInterval.CustomTime, bl.DefaultInterval);
            Assert.Equal(BarInterval.CustomTime, bl.Intervals[0]);
            Assert.Equal(myinterval, bl.DefaultCustomInterval);
            Assert.Equal(myinterval, bl.CustomIntervals[0]);

            // iterate ticks
            foreach (Tick k in SampleData())
                bl.NewTick(k);

            // count em
            Assert.Equal(10, bl.Count);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void DailyMixedTickAndBars()
        {
            BarListImpl bl = (BarListImpl)_dailyBarTracker["SPY", (int)BarInterval.Day];
            for (int x = 0; x < _spyDaily.Length; x++)
            {
                BarImpl b = (BarImpl)BarImpl.Deserialize(_spyDaily[x]);
                bl.NewPoint(b.Symbol, b.Open, b.Bartime, b.Bardate, 0);
                bl.NewPoint(b.Symbol, b.High, b.Bartime, b.Bardate, 0);
                bl.NewPoint(b.Symbol, b.Low, b.Bartime, b.Bardate, 0);
                bl.NewPoint(b.Symbol, b.Close, b.Bartime, b.Bardate, (int)b.Volume);

                // interleave ticks with the bars to simulate datafeed asynchronous-ness
                int baseTime = 144500;
                TickImpl k = new TickImpl("SPY");
                k.Ask = 101;
                k.Bid = 100;
                k.Date = 20110208;
                k.Trade = 100.5m;
                k.Size = 5000;
                k.Time = baseTime + x;
                _dailyBarTracker.NewTick(k);
            }

            Console.WriteLine("Count: " + bl.Count);
            DisplayTrackerContent(_dailyBarTracker["SPY"]);
            Assert.Equal(_spyDaily.Length, bl.Count);
            for (int x = 0; x < _spyDaily.Length - 1; x++)
            {
                Bar t = BarImpl.Deserialize(_spyDaily[x]);
                Assert.Equal(bl[x].Bardate, t.Bardate);
                Assert.Equal(bl[x].Close, t.Close);
            }
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void DefaultIntervalAndReset()
        {
            // get some data
            Tick[] tape = SampleData();

            // setup an hour barlist
            BarListImpl bl = new BarListImpl();
            bl.DefaultInterval = BarInterval.Hour;

            // build the barlist
            foreach (Tick k in tape)
                bl.NewTick(k);

            // make sure we have 2 hour bars
            Assert.Equal(2, bl.Count);

            // switch default
            bl.DefaultInterval = BarInterval.FiveMin;

            // make sure we have 3 5min bars
            Assert.Equal(3, bl.Count);

            // reset it
            bl.Reset();

            // verify we have no data
            Assert.Equal(0, bl.Count);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void FiveMin()
        {
            // get some sample data to fill barlist
            Tick[] ticklist = SampleData();

            // prepare barlist
            BarListImpl bl = new BarListImpl(BarInterval.FiveMin);
            bl.GotNewBar += bl_GotNewBar;

            // reset count
            _newbars = 0;

            // create bars from all ticks available
            foreach (TickImpl k in ticklist)
            {
                // add tick to bar
                bl.NewTick(k);
            }

            // verify we had expected number of bars
            Assert.Equal(3, _newbars);

            // verify symbol was set
            Assert.Equal(Sym, bl.Symbol);

            // verify each bar symbol matches barlist
            foreach (Bar b in bl)
                Assert.Equal(bl.Symbol, b.Symbol);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void FromTIK()
        {
            const string tf = @"Common\EURUSD20080826.TIK";

            // get sample tick data
            BarList bl = BarListImpl.FromTIK(tf);

            // verify expected number of 5min bars exist (78 in 9:30-4p)
            Assert.True(bl.Count > 82);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void HourTest()
        {
            // get data
            Tick[] tape = SampleData();

            // count new hour bars
            _newbars = 0;

            // setup hour bar barlist
            BarListImpl bl = new BarListImpl(BarInterval.Hour, Sym);

            // handle new bar events
            bl.GotNewBar += bl_GotNewBar;

            // add ticks to bar
            foreach (Tick k in tape)
            {
                // add ticks
                bl.NewTick(k);
            }

            // make sure we have at least 1 bars
            Assert.True(bl.Has(1));

            // make sure we actually have two bars
            Assert.Equal(2, _newbars);
            Assert.Equal(bl.Count, _newbars);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void InsertBar_MultipleInsert()
        {
            string sym = "FTI";
            int d = 20070926;
            var bint = BarInterval.FiveMin;
            var bsize = (int)bint;
            BarList org = new BarListImpl(bint, sym);
            Assert.True(org.IsValid, "your original barlist is not valid 1");
            int orgcount = org.Count;
            Assert.Equal(0, orgcount);

            int h = 7;
            int m = 55;
            for (int i = 0; i < 10; i++)
            {
                int t = h * 10000 + m * 100;
                t *= 1000;
                Bar insert = new BarImpl(30, 30, 30, 30, 10000, d, t, sym, bsize);
                Assert.True(insert.IsValid, "your bar to insert is not valid #" + i);
                int insertpos = BarListImpl.GetBarIndexPreceeding(org, insert.Bardate, insert.Bartime);
                Assert.Equal(i - 1, insertpos);
                BarList inserted = BarListImpl.InsertBar(org, insert, insertpos);
                //Assert.True(g.ta(i + 1 == inserted.Count, BarListImpl.Bars2String(org)+Environment.NewLine+ BarListImpl.Bars2String(inserted)), "element count after insertion #" + i + " pos: " + insertpos);
                m += 5;
                if (m >= 60)
                {
                    h += m / 60;
                    m = m % 60;
                }
                org = inserted;
            }
            Assert.Equal(orgcount + 10, org.Count);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void InsertBar_NoexistingBars()
        {
            string sym = "FTI";
            int d = 20070926;

            // historical bar filename
            string filename = sym + d + TikConst.DotExt;

            // test for the parameter's prescence
            Assert.NotEmpty(filename);

            // unit test case 1 no existing bars aka (empty or brand-new insertion)
            BarList org = new BarListImpl(BarInterval.FiveMin, sym);
            Assert.True(org.IsValid, "your original barlist is not valid 1");
            int orgcount = org.Count;
            Assert.Equal(0, orgcount);

            // make up a bar here  (eg at 755 in FTI there are no ticks so this should add a new bar in most scenarios)
            Bar insert = new BarImpl(30, 30, 30, 30, 10000, d, 75500000, sym, (int)BarInterval.FiveMin);
            Assert.True(insert.IsValid, "your bar to insert is not valid 1");
            BarList inserted = BarListImpl.InsertBar(org, insert, org.Count);
            Assert.Equal(inserted.Count, orgcount + 1);
            Bar actualinsert = inserted.RecentBar;
            Assert.True(actualinsert.IsValid);
            Assert.Equal(insert.Close, actualinsert.Close);
            Assert.Equal(insert.Open, actualinsert.Open);
            Assert.Equal(insert.High, actualinsert.High);
            Assert.Equal(insert.Low, actualinsert.Low);
            Assert.Equal(insert.Symbol, actualinsert.Symbol);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void InsertBar_OutOfOrderTicks()
        {
            string sym = "TPX";
            int d = 20120724;

            // historical bar filename
            string filename = @"Common\" + sym + d + TikConst.DotExt;

            //string filename = @"TPX20120724.TIK";
            // unit test case 2 existing bars with front insertion (aka historical bar insert)
            int[] intervals = { (int)BarInterval.Minute };
            BarInterval[] types = { BarInterval.Minute };

            BarList org = BarListImpl.FromTIK(filename, true, false, intervals, types);

            Assert.True(org.IsValid, "your original bar is not valid 2");
            var orgcount = org.Count;
            //Assert.Equal(2, orgcount);
            Assert.Equal(1, orgcount);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void IntraMixedTickAndBars()
        {
            BarListImpl bl = (BarListImpl)_intraBarTracker["SPY", (int)BarInterval.FiveMin];
            for (int x = 0; x < _spyIntra.Length; x++)
            {
                BarImpl b = (BarImpl)BarImpl.Deserialize(_spyIntra[x]);

                bl.NewPoint(b.Symbol, b.Open, b.Bartime, b.Bardate, 0);
                bl.NewPoint(b.Symbol, b.High, b.Bartime, b.Bardate, 0);
                bl.NewPoint(b.Symbol, b.Low, b.Bartime, b.Bardate, 0);
                bl.NewPoint(b.Symbol, b.Close, b.Bartime, b.Bardate, (int)b.Volume);

                // interleave ticks with the bars to simulate datafeed asynchronous-ness
                int baseTime = 144500;
                TickImpl k = new TickImpl("SPY");
                k.Ask = 101;
                k.Bid = 100;
                k.Date = 20110208;
                k.Trade = 100.5m;
                k.Size = 5000;
                k.Time = baseTime + x;
                k.Time *= 1000;
                _intraBarTracker.NewTick(k);
            }

            Console.WriteLine("Count: " + bl.Count);
            DisplayTrackerContent(_intraBarTracker["SPY"]);
            Assert.Equal(_spyIntra.Length, bl.Count);
            for (int x = 0; x < _spyIntra.Length - 1; x++)
            {
                Bar t = BarImpl.Deserialize(_spyIntra[x]);
                Assert.Equal(bl[x].Bardate, t.Bardate);
                Assert.Equal(bl[x].Close, t.Close);
            }
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void NewBarEvent()
        {
            // get tickdata
            Tick[] tape = SampleData();
            // reset bar count
            _newbars = 0;
            // request hour interval
            BarListImpl bl = new BarListImpl(BarInterval.Hour, Sym);
            // handle new bars
            bl.GotNewBar += bl_GotNewBar;

            foreach (TickImpl k in tape)
                bl.NewTick(k);

            Assert.Equal(2, _newbars);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void OneMinute()
        {
            // prepare barlist
            BarListImpl bl = new BarListImpl(BarInterval.Minute);

            // reset count
            int newbars = 0;

            // build bars from ticks available
            foreach (TickImpl k in SampleData())
            {
                // add tick to bar
                bl.NewTick(k);
                // count if it's a new bar
                if (bl.RecentBar.IsNew)
                    newbars++;
            }

            // verify expected # of bars are present
            Assert.Equal(9, newbars);

            // verify barcount is same as newbars
            Assert.Equal(newbars, bl.Count);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void PointFiveMin()
        {
            // get some sample data to fill barlist
            Tick[] ticklist = SampleData();

            // prepare barlist
            BarListImpl bl = new BarListImpl(BarInterval.FiveMin);
            bl.GotNewBar += bl_GotNewBar;

            // reset count
            _newbars = 0;

            // create bars from all ticks available
            foreach (TickImpl k in ticklist)
            {
                // add tick to bar
                bl.NewPoint(k.Symbol, k.Trade, k.Time, k.Date, k.Size);
            }

            // verify we had expected number of bars
            Assert.Equal(3, bl.Count);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void PointHour()
        {
            // get data
            Tick[] tape = SampleData();

            // count new hour bars
            _newbars = 0;

            // setup hour bar barlist
            BarListImpl bl = new BarListImpl(BarInterval.Hour, Sym);

            // handle new bar events
            bl.GotNewBar += bl_GotNewBar;

            // add ticks to bar
            foreach (Tick k in tape)
            {
                // add ticks
                bl.NewPoint(k.Symbol, k.Trade, k.Time, k.Date, k.Size);
            }

            // make sure we have at least 1 bars
            Assert.True(bl.Has(1));

            // make sure we actually have two bars
            Assert.Equal(2, bl.Count);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void PointMinute()
        {
            // prepare barlist
            BarListImpl bl = new BarListImpl(BarInterval.Minute);

            // reset count
            int newbars = 0;

            // build bars from ticks available
            foreach (TickImpl k in SampleData())
            {
                // add tick to bar
                bl.NewPoint(k.Symbol, k.Trade, k.Time, k.Date, k.Size);
                // count if it's a new bar
                if (bl.RecentBar.IsNew)
                    newbars++;
            }

            // verify expected # of bars are present
            Assert.Equal(9, newbars);

            // verify barcount is same as newbars
            Assert.Equal(newbars, bl.Count);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void SpoolDailyBars()
        {
            BarListImpl bl = (BarListImpl)_dailyBarTracker["SPY", (int)BarInterval.Day];
            for (int x = 0; x < _spyDaily.Length; x++)
            {
                Bar b = BarImpl.Deserialize(_spyDaily[x]);

                bl.NewPoint(b.Symbol, b.Open, b.Bartime, b.Bardate, 0);
                bl.NewPoint(b.Symbol, b.High, b.Bartime, b.Bardate, 0);
                bl.NewPoint(b.Symbol, b.Low, b.Bartime, b.Bardate, 0);
                bl.NewPoint(b.Symbol, b.Close, b.Bartime, b.Bardate, (int)b.Volume);
            }

            Console.WriteLine("Count: " + bl.Count);
            Assert.Equal(_spyDaily.Length, bl.Count);
            for (int x = 0; x < _spyDaily.Length; x++)
            {
                Bar t = BarImpl.Deserialize(_spyDaily[x]);
                Assert.Equal(bl[x].Bardate, t.Bardate);
                Assert.Equal(bl[x].Close, t.Close);
            }

            DisplayTrackerContent(_dailyBarTracker["SPY"]);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void SpoolIntraBars()
        {
            BarListImpl bl = (BarListImpl)_intraBarTracker["SPY", (int)BarInterval.FiveMin];
            for (int x = 0; x < _spyIntra.Length; x++)
            {
                Bar b = BarImpl.Deserialize(_spyIntra[x]);

                bl.NewPoint(b.Symbol, b.Open, b.Bartime, b.Bardate, 0);
                bl.NewPoint(b.Symbol, b.High, b.Bartime, b.Bardate, 0);
                bl.NewPoint(b.Symbol, b.Low, b.Bartime, b.Bardate, 0);
                bl.NewPoint(b.Symbol, b.Close, b.Bartime, b.Bardate, (int)b.Volume);
            }

            Console.WriteLine("Count: " + bl.Count);

            Assert.Equal(_spyIntra.Length, bl.Count);
            for (int x = 0; x < _spyIntra.Length; x++)
            {
                Bar t = BarImpl.Deserialize(_spyIntra[x]);
                Assert.Equal(bl[x].Bardate, t.Bardate);
                Assert.Equal(bl[x].Close, t.Close);
            }

            DisplayTrackerContent(_intraBarTracker["SPY"]);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void TestMaximumHistory()
        {
            const string tf = @"Common\EURUSD20080826.TIK";

            // get sample tick data
            BarList bl = BarListImpl.FromTIK(tf);

            // verify that the max amount is set to 200
            Assert.True(bl.Count == 200);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void TickInterval()
        {
            // request 2 tick bars
            const int myinterval = 2;
            BarListImpl bl = new BarListImpl(Sym, myinterval, BarInterval.CustomTicks);

            // verify custom interval
            Assert.Equal(myinterval, bl.DefaultCustomInterval);
            Assert.Equal(myinterval, bl.CustomIntervals[0]);

            // iterate ticks
            foreach (Tick k in SampleData())
                bl.NewTick(k);

            // count em
            Assert.Equal(5, bl.Count);
        }

        #endregion Public Methods

        #region Private Methods

        private void bl_GotNewBar(string symbol, int interval)
        {
            _newbars++;
        }

        private void DisplayTrackerContent(BarList bl)
        {
            Console.WriteLine("Displaying Content.");

            foreach (Bar b in bl)
                Console.WriteLine("d: " + b.Bardate + " t: " + b.Bartime
                    + " o: " + b.Open
                    + " h: " + b.High
                    + " l: " + b.Low
                    + " c: " + b.Close);
        }

        #endregion Private Methods
    }
}