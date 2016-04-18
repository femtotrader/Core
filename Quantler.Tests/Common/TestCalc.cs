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
using Quantler.Data.Bars;
using Quantler.Interfaces;
using Quantler.Trades;
using System;
using Quantler.Securities;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestCalc
    {
        #region Private Fields

        private decimal entry = 99.47m;
        private decimal last = 100.45m;
        private TradeImpl lc, sc;
        private bool Long = true;
        private PositionImpl lp, sp;
        private int lsize = 200;
        private bool Short = false;
        private int ssize = -500;
        private string stock = "IBM";

        #endregion Private Fields

        #region Public Constructors

        public TestCalc()
        {
            ForexSecurity ls = new ForexSecurity(stock);
            lp = new PositionImpl(ls, entry, lsize);
            sp = new PositionImpl(ls, entry, ssize);

            //closing trades
            lc = new TradeImpl(ls.Name, last, lsize / -2);
            sc = new TradeImpl(ls.Name, last, -ssize);
        }

        #endregion Public Constructors

        #region Public Methods

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void ArrayArrayMath()
        {
            // integer
            int[] a1 = new int[] { 6, 10, 33, 2, -50, 7, 8 };
            int[] a2 = new int[] { 4, 0, -3, -2, 0, 3, 2 };

            Assert.Equal(20, Calc.Sum(Calc.Add(a1, a2)));
            Assert.Equal(12, Calc.Sum(Calc.Subtract(a1, a2)));
            Assert.Equal(-42, Calc.Sum(Calc.Product(a1, a2)));
            Assert.Equal(-4.166666666666666666666666667m, Calc.Sum(Calc.Divide(a1, a2)));

            // decimal
            decimal[] b1 = new decimal[] { 6.4m, 10.1m, 33.33m, 2.7m, -50, 7.1m, 8 };
            decimal[] b2 = new decimal[] { 4.1m, 0, -3, -2, 0, 3, 2.7m };

            Assert.Equal(2243, Math.Round(Calc.Sum(Calc.Add(b1, b2)) * 100));
            Assert.Equal(1283, Math.Round(Calc.Sum(Calc.Subtract(b1, b2)) * 100));
            Assert.Equal(-3625, Math.Round(Calc.Sum(Calc.Product(b1, b2)) * 100));
            Assert.Equal(-557, Math.Round(Calc.Sum(Calc.Divide(b1, b2)) * 100));
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void ArrayScalarMath()
        {
            int[] a1 = new int[] { 6, 10, 33, 2, -50, 7, 8 };

            Assert.Equal(51, Calc.Sum(Calc.Add(a1, 5)));
            Assert.Equal(-19, Calc.Sum(Calc.Subtract(a1, 5)));
            Assert.Equal(80, Calc.Sum(Calc.Product(a1, 5)));
            Assert.Equal(3.2m, Calc.Sum(Calc.Divide(a1, 5)));

            decimal[] b1 = new decimal[] { 6.4m, 10.1m, 33.33m, 2.7m, -50, 7.1m, 8 };
            Assert.Equal(56.13m, Calc.Sum(Calc.Add(b1, 5.5m)));
            Assert.Equal(-20.87m, Calc.Sum(Calc.Subtract(b1, 5.5m)));
            Assert.Equal(96.965m, Calc.Sum(Calc.Product(b1, 5.5m)));
            Assert.Equal(3.2054545454545454545454545454m, Calc.Sum(Calc.Divide(b1, 5.5m)));
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void ArraySlices()
        {
            // get input arrays
            int[] a1 = new int[] { 6, 10, 33, 2, -50, 7, 8 };
            decimal[] b1 = new decimal[] { 6.4m, 10.1m, 33.33m, 2.7m, -50, 7.1m, 8 };
            // get length of slice
            const int len = 5;
            // get slices
            int[] a2 = Calc.EndSlice(a1, len);
            decimal[] b2 = Calc.EndSlice(b1, len);
            decimal[] b3 = Calc.EndSlice(b1, 200);
            // verify lengths
            Assert.Equal(len, a2.Length);
            Assert.Equal(len, b2.Length);
            Assert.Equal(b1.Length, b3.Length);
            // verify last elements match
            Assert.Equal(a1[a1.Length - 1], a2[len - 1]);
            Assert.Equal(b1[b1.Length - 1], b2[len - 1]);
            // verify start elements match
            Assert.Equal(a1[a1.Length - len], a2[0]);
            Assert.Equal(b1[b1.Length - len], b2[0]);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void AvgAndStd()
        {
            // data
            int[] a1 = new int[] { 6, 10, 33, 2, -50, 7, 8 };
            decimal[] b1 = new decimal[] { 6.4m, 10.1m, 33.33m, 2.7m, -50, 7.1m, 8 };

            // averages
            Assert.Equal(2.2857142857142857142857142857m, Calc.Avg(a1));
            Assert.Equal(2.5185714285714285714285714286m, Calc.Avg(b1));

            // stdev
            Assert.Equal(23.315931314473m, Calc.StdDev(a1));
            Assert.Equal(23.3946162479267m, Calc.StdDev(b1));

            // stdevsample
            Assert.Equal(25.1840788564133m, Calc.StdDevSam(a1));
            Assert.Equal(25.2690694320904m, Calc.StdDevSam(b1));
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void ClosePLWithPipValueAndLotSize()
        {
            decimal pl = -490M;

            ForexSecurity ts = (ForexSecurity)sp.Security;
            sc.Security = sp.Security;
            ts.PipSize = 2;
            ts.LotSize = 1; // Lot size as micro lots
            ts.PipValue = 2; // Normal pip value is 2 dollars per pip profit per lot

            Assert.Equal(pl, Calc.ClosePL(sp, sc));

            pl /= 2;
            ts.PipValue = 1; //If the pip value is twice as low, the results are twice as low due to the value of one pip

            Assert.Equal(pl, Calc.ClosePL(sp, sc));

            pl /= 2;
            ts.LotSize = 2; //If lotsize is twice as high, results are twice as low since the pip value is based on lotsize

            Assert.Equal(pl, Calc.ClosePL(sp, sc));
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void ClosePT()
        {
            decimal pl = .98m;
            Assert.Equal(pl, Calc.ClosePt(lp, lc));
            Assert.Equal(-pl, Calc.ClosePt(sp, sc));
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void DecimalPerformance()
        {
            DateTime start = DateTime.Now;
            const int SIZE = 250;
            const int REPS = 250;
            double EXPECT = .5;
            if (Environment.ProcessorCount == 1) EXPECT = .8;
            decimal[] a = new decimal[SIZE];
            decimal[] b = new decimal[SIZE];
            decimal[] c = new decimal[SIZE];
            for (int i = 0; i < REPS; i++)
            {
                init(ref a);
                init(ref b);
                c = Calc.Add(a, b);
                c = Calc.Subtract(a, b);
                c = Calc.Product(a, b);
                c = Calc.Divide(a, b);
                Calc.StdDev(a);
            }

            double time = DateTime.Now.Subtract(start).TotalSeconds;
            Assert.True(time <= EXPECT);
            Console.WriteLine("DecimalPerformance (expected): " + time.ToString("N2") + "s (" + EXPECT.ToString("N2") + "), reps/sec: " + (REPS / time).ToString("N0"));
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void IntPerformance()
        {
            DateTime start = DateTime.Now;
            const int SIZE = 500;
            const int REPS = 500;
            const double EXPECT = .25;
            int[] a = new int[SIZE];
            int[] b = new int[SIZE];
            int[] c = new int[SIZE];
            decimal[] d = new decimal[SIZE];
            for (int i = 0; i < REPS; i++)
            {
                init(ref a);
                init(ref b);
                c = Calc.Add(a, b);
                c = Calc.Subtract(a, b);
                c = Calc.Product(a, b);
                d = Calc.Divide(a, b);
                Calc.StdDev(a);
            }

            double time = DateTime.Now.Subtract(start).TotalSeconds;
            Assert.True(time <= EXPECT);
            Console.WriteLine("IntPerformance (expected): " + time.ToString("N2") + "s (" + EXPECT.ToString("N2") + "), reps/sec: " + (REPS / time).ToString("N0"));
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void MaxDD()
        {
            const string sym = "TST";
            SimAccount account = new SimAccount("TEST");
            ForexSecurity sec = new ForexSecurity(sym);
            sec.PipValue = 1;
            sec.LotSize = 1;
            sec.PipSize = 1;
            account.Securities.AddSecurity(sec);

            IPositionTracker pt = account.Positions;
            System.Collections.Generic.List<Trade> fills = new System.Collections.Generic.List<Trade>();
            TradeImpl t = new TradeImpl(sym, 10, 100);
            t.Security = sec;
            t.Account = account;
            System.Collections.Generic.List<decimal> ret = new System.Collections.Generic.List<decimal>();
            fills.Add(t);
            pt.Adjust(t);
            t = new TradeImpl(sym, 11, -100);
            t.Security = sec;
            t.Account = account;
            fills.Add(t);
            ret.Add(pt.Adjust(t));
            t = new TradeImpl(sym, 11, -100);
            t.Security = sec;
            t.Account = account;
            pt.Adjust(t);
            fills.Add(t);
            t = new TradeImpl(sym, 13, 100);
            t.Account = account;
            t.Security = sec;
            fills.Add(t);
            ret.Add(pt.Adjust(t));
            decimal maxdd = Calc.MaxDdVal(ret.ToArray());
            decimal maxddp = Calc.MaxDDPct(fills);
            Assert.Equal(-300, maxdd);
            Assert.Equal(-.18m, Math.Round(maxddp, 2));
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void OpenPL()
        {
            decimal pl = .98m;
            Assert.Equal(pl, Calc.OpenPT(last, entry, Direction.Long));
            Assert.Equal(-pl, Calc.OpenPT(last, entry, Direction.Short));
            Assert.Equal(pl, Calc.OpenPT(last, entry, lsize));
            Assert.Equal(-pl, Calc.OpenPT(last, entry, ssize));
            Assert.Equal(pl, Calc.OpenPT(last, lp));
            Assert.Equal(-pl, Calc.OpenPT(last, sp));
            Assert.Equal(lp.Size * pl, Calc.OpenPL(last, lp));
            Assert.Equal(sp.Size * pl, Calc.OpenPL(last, sp));
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Sums()
        {
            // integer
            int[] a1 = new int[] { 6, 10, 33, 2, -50, 7, 8 };
            Assert.Equal(15, Calc.Sum(a1, 2));
            Assert.Equal(16, Calc.Sum(a1));
            Assert.Equal(45, Calc.Sum(a1, 1, 3));

            // decimal
            decimal[] a2 = new decimal[] { 6.5m, 10.3m, 33.1m, 2, -50, 7.7m, 8 };
            Assert.Equal(15.7m, Calc.Sum(a2, 2));
            Assert.Equal(17.6m, Calc.Sum(a2));
            Assert.Equal(45.4m, Calc.Sum(a2, 1, 3));
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void SumSquares()
        {
            // integer
            int[] a1 = new int[] { 6, 10, 33, 2, -50, 7, 8 };
            Assert.Equal(113, Calc.SumSquares(a1, 2));
            Assert.Equal(3842, Calc.SumSquares(a1));
            Assert.Equal(1193, Calc.SumSquares(a1, 1, 3));

            // decimal
            decimal[] a2 = new decimal[] { 6.5m, 10.3m, 33.1m, 2, -50, 7.7m, 8 };
            Assert.Equal(123.29m, Calc.SumSquares(a2, 2));
            Assert.Equal(3871.24m, Calc.SumSquares(a2));
            Assert.Equal(1205.7m, Calc.SumSquares(a2, 1, 3));
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void TrueRange()
        {
            decimal[] high = new decimal[] { 10, 12, 15, 20 };
            decimal[] close = new decimal[] { 5, 9, 13, 7 };
            decimal[] low = new decimal[] { 3, 6, 8, 5 };
            decimal[] tr = new decimal[] { 7, 7, 15 };

            BarListImpl bars = new BarListImpl(BarInterval.Day);
            for (int i = 0; i < high.Length; i++)
            {
                bars.NewPoint("TEST", high[i], 0, i, 1);
                bars.NewPoint("TEST", low[i], 1, i, 1);
                bars.NewPoint("TEST", close[i], 2, i, 1);
            }
            decimal[] res = Calc.TrueRange(bars);
            Assert.Equal(tr.Length, res.Length);
            for (int i = 0; i < tr.Length; i++)
            {
                Assert.Equal(tr[i], res[i]);
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void init(ref int[] a)
        {
            Random r = new Random((int)DateTime.Now.Ticks);
            for (int i = 0; i < a.Length; i++)
                a[i] = r.Next(0, 1000);
        }

        private void init(ref decimal[] a)
        {
            Random r = new Random((int)DateTime.Now.Ticks);
            for (int i = 0; i < a.Length; i++)
                a[i] = (decimal)r.NextDouble() * 1000;
        }

        #endregion Private Methods
    }
}