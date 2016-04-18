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

using Quantler.Broker;
using Quantler.Data.Ticks;
using Quantler.Interfaces;
using Quantler.Orders;
using Quantler.Simulator;
using Quantler.Tracker;
using System;
using System.Collections.Generic;
using Quantler.Securities;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestMulti : IDisposable
    {
        #region Private Fields

        private BarListTrackerImpl bt = new BarListTrackerImpl();

        private int desiredfills = 1000;

        private double EXPECTBARS = .6;

        private double EXPECTEX = .8;

        private double EXPECTRAW = .6;

        private int fillcount = 0;

        private bool GOODTIME = true;

        private MultiSimImpl h;

        private long lasttime = 0;

        private int loadcount = 0;

        private long product = 0;

        private bool run = true;

        private SimBroker SimBroker = new SimBroker();

        private List<string> syms = new List<string>();

        private int tickcount = 0;

        #endregion Private Fields

        #region Public Constructors

        public TestMulti()
        {
            run = true;
        }

        #endregion Public Constructors

        #region Public Methods

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void BarPerformance()
        {
            MultiSimImpl h = new MultiSimImpl(new string[] { @"Common\FTI20070926.TIK", @"Common\ABN20080318.TIK", @"Common\SPX20070926.TIK" });
            h.GotTick += h_GotTick;

            h.Initialize();

            tickcount = 0;
            lasttime = 0;

            Assert.Equal(0, lasttime);
            Assert.True(h.TicksPresent > 0);
            if (Environment.ProcessorCount == 1) EXPECTBARS *= 2.5;

            DateTime start = DateTime.Now;

            h.PlayTo(MultiSimImpl.Endsim);

            double time = DateTime.Now.Subtract(start).TotalSeconds;
            h.Stop();
            Assert.True(tickcount >= 50000);
            Assert.Equal(3, bt.SymbolCount);
            Assert.True(time <= EXPECTBARS);
        }

        public void Dispose()
        {
            run = false;
        }

        [Fact(Skip = "Slow Build Server")]
        [Trait("Quantler.Common", "Quantler")]
        public void ExecutionPerformance()
        {
            System.Threading.Thread.Sleep(100);
            h = new MultiSimImpl(new string[] { @"Common\FTI20070926.TIK", @"Common\ABN20080318.TIK", @"Common\SPX20070926.TIK" });
            h.Initialize();
            h.GotTick += execute_GotTick;
            SimBroker.GotFill += SimBroker_GotFill;

            tickcount = 0;
            lasttime = 0;

            Assert.Equal(0, tickcount);
            Assert.Equal(0, syms.Count);
            Assert.Equal(0, lasttime);
            Assert.True(h.TicksPresent > 0);
            if (Environment.ProcessorCount == 1) EXPECTEX *= 2.5;

            DateTime start = DateTime.Now;

            h.PlayTo(MultiSimImpl.Endsim);

            double time = DateTime.Now.Subtract(start).TotalSeconds;

            Assert.Equal(desiredfills, fillcount);
            Assert.True(time <= EXPECTEX);
            h.Stop();
        }

        public void rawbase(string name, int symcount, MultiSimImpl sim)
        {
            Console.WriteLine(name.ToUpper());
            MultiSimImpl h = sim;
            h.Initialize();
            h.GotTick += raw_GotTick;
            tickcount = 0;
            syms.Clear();
            lasttime = 0;
            Assert.Equal(0, tickcount);
            Assert.Equal(0, syms.Count);
            Assert.Equal(0, lasttime);
            Assert.True(h.TicksPresent > 0);

            if (Environment.ProcessorCount == 1) EXPECTRAW *= 2.5;

            DateTime start = DateTime.Now;

            h.PlayTo(MultiSimImpl.Endsim);

            // printout simulation runtime
            Console.WriteLine("Ticks received: " + tickcount + " sent: " + h.TicksProcessed + " estimate: " + h.TicksPresent);
            Console.WriteLine("Raw runtime: " + h.RunTimeSec.ToString("N2") + "sec, versus: " + EXPECTRAW + "sec expected.");
            Console.WriteLine("Raw speed: " + h.RunTimeTicksPerSec.ToString("N0") + " ticks/sec");

            // make sure ticks arrived in order
            Assert.True(h.IsTickPlaybackOrdered, "Tick arrived out-of-order.");
            // ensure got expected number of symbols
            Assert.Equal(symcount, syms.Count);
            // variance from approximate count should be less than 1%
            Assert.True((tickcount - h.TicksPresent) / h.TicksPresent < .01);
            // actual count should equal simulation count
            Assert.Equal(h.TicksProcessed, tickcount);

            h.Stop();
            Console.WriteLine(name.ToUpper());
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void RawPerformance()
        {
            MultiSimImpl h = new MultiSimImpl(new string[] { @"Common\FTI20070926.TIK", @"Common\ABN20080318.TIK", @"Common\SPX20070926.TIK" });
            rawbase("raw performance multi", 3, h);
            // tick count is = 42610 (FTI) + 5001 (SPX) + 8041 (ABN)
            Assert.Equal(42610 + 4991 + 8041, tickcount);
            // check running time
            Assert.True(h.RunTimeSec <= EXPECTRAW, "may fail on slow machines");

            // last time is 1649 on SPX
            //Assert.Equal(20080318155843, lasttime);
            Assert.Equal(20080318000155843, lasttime);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void RawPerformanceWithLoad()
        {
            System.ComponentModel.BackgroundWorker bw = new System.ComponentModel.BackgroundWorker();
            bw.DoWork += bw_DoWork;
            bw.WorkerSupportsCancellation = true;
            MultiSimImpl h = new MultiSimImpl(new string[] { @"Common\FTI20070926.TIK", @"Common\ABN20080318.TIK", @"Common\SPX20070926.TIK" });
            rawbase("raw performance multi w/load", 3, h);
            // tick count is = 42610 (FTI) + 5001 (SPX) + 8041 (ABN)
            Assert.Equal(42610 + 4991 + 8041, tickcount);
            // check running time
            Assert.True(h.RunTimeSec <= EXPECTRAW, "may fail on slow machines");

            // last time is 1649 on SPX
            //Assert.Equal(20080318155843, lasttime);
            Assert.Equal(20080318000155843, lasttime);
            RawPerformance();
            bw.CancelAsync();
            run = false;
            // tick count is = 42610 (FTI) + 5001 (SPX) + 8041 (ABN)
            Assert.Equal(42610 + 4991 + 8041, tickcount);
            // check running time
            Assert.True(h.RunTimeSec <= EXPECTRAW, "may fail on slow machines");

            // last time is 1649 on SPX
            //Assert.Equal(20080318155843, lasttime);
            Assert.Equal(20080318000155843, lasttime);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void RawSingle()
        {
            MultiSimImpl h = new MultiSimImpl(new string[] { @"Common\FTI20070926.TIK" });
            rawbase("rawsingle", 1, h);
            // tick count is = 42610 (FTI) + 5001 (SPX) + 8041 (ABN)
            Assert.True(tickcount > 40000);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void RawSingleFromZip()
        {
            MultiSimImpl h = new MultiSimImpl(new string[] { @"Common\AUDJPY.zip\AUDJPY20070510.TIK" });
            rawbase("rawsingle", 1, h);
            Assert.Equal(5616, tickcount);
        }

        #endregion Public Methods

        #region Private Methods

        private void bw_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Random r = new Random();
            run = true;
            while (!e.Cancel && run)
            {
                product = r.Next() * r.Next() + loadcount++;
                if (loadcount % 1000 == 0)
                    System.Threading.Thread.Sleep(10);
            }
            Console.WriteLine("load simulation completed. calculations performed: " + loadcount);
        }

        private void execute_GotTick(Tick t)
        {
            TickImpl tick = (TickImpl)t;
            SimBroker.Execute(tick);
            tickcount++;
            // generate fills periodically
            if (fillcount >= desiredfills) return;
            if (tickcount % 50 == 0)
            {
                bool side = fillcount % 2 == 0;

                OrderImpl o = new OrderImpl(new ForexSecurity(t.Symbol), side ? Direction.Long : Direction.Short, 100);
                PendingOrder po = new PendingOrderImpl(o);

                SimBroker.SendOrderStatus(po);
            }
        }

        private void h_GotTick(Tick t)
        {
            tickcount++;
            bt.NewTick(t);
        }

        private void raw_GotTick(Tick t)
        {
            if (!syms.Contains(t.Symbol))
                syms.Add(t.Symbol);
            tickcount++;
            bool viol = t.Datetime < lasttime;
            GOODTIME &= !viol;
            lasttime = t.Datetime;
        }

        private void SimBroker_GotFill(Trade t, PendingOrder o)
        {
            fillcount++;
        }

        #endregion Private Methods
    }
}