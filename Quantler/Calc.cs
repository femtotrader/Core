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
using Quantler.Interfaces;
using Quantler.Tracker;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantler
{
    /// <summary>
    /// collection of calculations available in Quantler
    /// </summary>
    public static class Calc
    {
        #region Private Fields

        private const long D2LMult = 65536;

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// gets absolute return of portfolio of positions at closing or market prices, or both
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="marketprices"></param>
        /// <param name="countClosedPl"></param>
        /// <param name="countOpenPl"></param>
        /// <returns></returns>
        public static decimal[] AbsoluteReturn(PositionTracker pt, decimal[] marketprices, bool countClosedPl, bool countOpenPl)
        {
            decimal[] aret = new decimal[pt.Count];
            if (countOpenPl && (pt.Count != marketprices.Length))
                throw new Exception("market prices must have 1:1 correspondence with positions in tracker.");
            for (int i = 0; i < pt.Count; i++)
            {
                if (countOpenPl)
                    aret[i] += OpenPL(marketprices[i], pt[i]);
                if (countClosedPl)
                    aret[i] += pt[i].GrossPnL;
            }
            return aret;
        }

        /// <summary>
        /// calculate absolute return only for closed portions of positions
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static decimal[] AbsoluteReturn(PositionTracker pt)
        {
            return AbsoluteReturn(pt, new GenericTrackerImpl<decimal>(0), true);
        }

        public static decimal[] AbsoluteReturn(PositionTracker pt, GenericTrackerImpl<decimal> marketprices)
        {
            return AbsoluteReturn(pt, marketprices, true);
        }

        /// <summary>
        /// returns absolute return of all positions in order they are listed in position tracker
        /// both closed and open pl may be included
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="marketprices"></param>
        /// <param name="countClosedPl"></param>
        /// <returns></returns>
        public static decimal[] AbsoluteReturn(PositionTracker pt, GenericTrackerImpl<decimal> marketprices, bool countClosedPl)
        {
            decimal[] aret = new decimal[pt.Count];
            bool countOpenPl = marketprices.Count >= pt.Count;
            for (int i = 0; i < pt.Count; i++)
            {
                // get position
                Position p = pt[i];
                // get index
                int idx = marketprices.getindex(p.Security.Name);
                // see if we're doing open
                if (countOpenPl && (idx >= 0))
                    aret[i] += OpenPL(marketprices[idx], p);
                if (countClosedPl)
                    aret[i] += p.GrossPnL;
            }
            return aret;
        }

        /// <summary>
        /// adds two arrays
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static int[] Add(int[] array1, int[] array2)
        {
            // normalize sizes of arrays
            bool a2Bigger = array1.Length < array2.Length;
            int max = a2Bigger ? array2.Length : array1.Length;
            int[] s1 = new int[max];
            int[] s2 = new int[max];
            Buffer.BlockCopy(array1, 0, s1, 0, array1.Length * 4);
            Buffer.BlockCopy(array2, 0, s2, 0, array2.Length * 4);
            // calculate values
            for (int i = 0; i < s1.Length; i++)
                s2[i] += s1[i];
            return s2;
        }

        public static long[] Add(long[] array1, long[] array2)
        {
            // normalize sizes of arrays
            bool a2Bigger = array1.Length < array2.Length;
            int max = a2Bigger ? array2.Length : array1.Length;
            long[] s1 = new long[max];
            long[] s2 = new long[max];
            Buffer.BlockCopy(array1, 0, s1, 0, array1.Length * 8);
            Buffer.BlockCopy(array2, 0, s2, 0, array2.Length * 8);
            // calculate values
            for (int i = 0; i < s1.Length; i++)
                s2[i] += s1[i];
            return s2;
        }

        /// <summary>
        /// adds two arrays
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static decimal[] Add(decimal[] array1, decimal[] array2)
        {
            // normalize sizes of arrays
            bool a2Bigger = array1.Length < array2.Length;
            int max = a2Bigger ? array2.Length : array1.Length;
            long[] s1 = decimal2long(array1, max);
            long[] s2 = decimal2long(array2, max);
            // calculate values
            for (int i = 0; i < s1.Length; i++)
                s2[i] += s1[i];
            return long2decimal(s2);
        }

        /// <summary>
        /// adds two arrays
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static double[] Add(double[] array1, double[] array2)
        {
            // normalize sizes of arrays
            bool a2Bigger = array1.Length < array2.Length;
            int max = a2Bigger ? array2.Length : array1.Length;
            long[] s1 = double2long(array1, max);
            long[] s2 = double2long(array2, max);
            // calculate values
            for (int i = 0; i < s1.Length; i++)
                s2[i] += s1[i];
            return long2double(s2);
        }

        /// <summary>
        /// adds a constant to an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static double[] Add(int[] array, double val)
        {
            double[] r = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] + val;
            return r;
        }

        /// <summary>
        /// adds a constant to an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static double[] Add(long[] array, double val)
        {
            double[] r = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] + val;
            return r;
        }

        /// <summary>
        /// adds a constant to an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static decimal[] Add(int[] array, decimal val)
        {
            decimal[] r = new decimal[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] + val;
            return r;
        }

        /// <summary>
        /// adds a constant to an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static decimal[] Add(long[] array, decimal val)
        {
            decimal[] r = new decimal[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] + val;
            return r;
        }

        /// <summary>
        /// adds a constant to an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static long[] Add(long[] array, long val)
        {
            long[] r = new long[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] + val;
            return r;
        }

        public static int[] Add(int[] array, int val)
        {
            int[] r = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] + val;
            return r;
        }

        public static decimal[] Add(decimal[] array, decimal val)
        {
            decimal[] r = new decimal[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] + val;
            return r;
        }

        public static double[] Add(double[] array, double val)
        {
            double[] r = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] + val;
            return r;
        }

        /// <summary>
        /// adds two arrays
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static long[] AddBig(int[] array1, int[] array2)
        {
            // normalize sizes of arrays
            bool a2Bigger = array1.Length < array2.Length;
            int max = a2Bigger ? array2.Length : array1.Length;
            int[] s1 = new int[max];
            int[] s2 = new int[max];
            long[] s3 = new long[max];
            Buffer.BlockCopy(array1, 0, s1, 0, array1.Length * 4);
            Buffer.BlockCopy(array2, 0, s2, 0, array2.Length * 4);
            // calculate values
            for (int i = 0; i < s1.Length; i++)
                s3[i] = s1[i] + s2[i];
            return s3;
        }

        /// <summary>
        /// gets mean of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static decimal Avg(decimal[] array)
        {
            return Avg(array, true);
        }

        public static decimal Avg(decimal[] array, int start, int len)
        {
            return Avg(array, true, start, len);
        }

        /// <summary>
        /// gets mean of an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="returnzeroIfempty"></param>
        /// <returns></returns>
        public static decimal Avg(decimal[] array, bool returnzeroIfempty)
        {
            return Avg(array, returnzeroIfempty, 0, array.Length);
        }

        public static decimal Avg(decimal[] array, bool returnzeroIfempty, int start, int len)
        {
            return array.Length == 0 ? 0 : Sum(array, start, len) / len;
        }

        public static decimal Avg(int[] array)
        {
            return Avg(array, true);
        }

        public static decimal Avg(int[] array, int start, int len)
        {
            return Avg(array, true, start, len);
        }

        public static decimal Avg(int[] array, bool returnzeroIfempty)
        {
            return Avg(array, returnzeroIfempty, 0, array.Length);
        }

        public static decimal Avg(int[] array, bool returnzeroIfempty, int start, int len)
        {
            return array.Length == 0 ? 0 : (decimal)Sum(array, start, len) / len;
        }

        public static decimal Avg(long[] array)
        {
            return Avg(array, true);
        }

        public static decimal Avg(long[] array, int start, int len)
        {
            return Avg(array, true);
        }

        public static decimal Avg(long[] array, bool returnzeroifEmpty)
        {
            return Avg(array, returnzeroifEmpty, 0, array.Length);
        }

        public static decimal Avg(long[] array, bool returnzeroifEmpty, int start, int len)
        {
            if (returnzeroifEmpty)
            {
                if (array.Length == 0)
                    return 0;
                return Sum(array, start, len) / len;
            }
            return Sum(array, start, len) / len;
        }

        /// <summary>
        /// gets mean of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double Avg(double[] array)
        {
            return Avg(array, 0, array.Length);
        }

        public static double Avg(double[] array, int start, int len)
        {
            return Avg(array, true, start, len);
        }

        /// <summary>
        /// gets mean of an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="returnzeroIfempty"></param>
        /// <returns></returns>
        public static double Avg(double[] array, bool returnzeroIfempty)
        {
            return Avg(array, returnzeroIfempty, 0, array.Length);
        }

        public static double Avg(double[] array, bool returnzeroIfempty, int start, int len)
        {
            return array.Length == 0 ? 0 : Sum(array, start, len) / len;
        }

        /// <summary>
        /// calculates lower bollinger using default # stdev of 2.5 and opening prices. Note, for
        /// speed it's faster to calculate these yourself.
        /// </summary>
        /// <param name="bl"></param>
        /// <returns></returns>
        public static decimal BollingerLower(BarList bl)
        {
            return BollingerLower(bl, 2.5m, true);
        }

        /// <summary>
        /// calculates lower bollinger using opening prices. calculate yourself for faster speed
        /// </summary>
        /// <param name="bl"></param>
        /// <param name="numStdDevs"></param>
        /// <returns></returns>
        public static decimal BollingerLower(BarList bl, decimal numStdDevs)
        {
            return BollingerLower(bl, numStdDevs, true);
        }

        /// <summary>
        /// calculates lower bollinger using open (true) or closing (false) prices, at specified #
        /// of standard deviations
        /// </summary>
        /// <param name="bl"></param>
        /// <param name="numStdDevs"></param>
        /// <param name="useOpens"></param>
        /// <returns></returns>
        public static decimal BollingerLower(BarList bl, decimal numStdDevs, bool useOpens)
        {
            decimal[] prices = useOpens ? bl.Open() : bl.Close();
            decimal mean = Avg(prices);
            decimal stdev = StdDev(prices);
            return mean - stdev * numStdDevs;
        }

        /// <summary>
        /// calculates upper bollinger using default # stdev of 2.5 and opening prices. Note, for
        /// speed it's faster to calculate these yourself.
        /// </summary>
        /// <param name="bl"></param>
        /// <returns></returns>
        public static decimal BollingerUpper(BarList bl)
        {
            return BollingerUpper(bl, 2.5m, true);
        }

        public static decimal BollingerUpper(BarList bl, decimal numStdDevs)
        {
            return BollingerUpper(bl, numStdDevs, true);
        }

        public static decimal BollingerUpper(BarList bl, decimal numStdDevs, bool useOpens)
        {
            decimal[] prices = useOpens ? bl.Open() : bl.Close();
            decimal mean = Avg(prices);
            decimal stdev = StdDev(prices);
            return mean + stdev * numStdDevs;
        }

        /// <summary>
        /// Gets the closed PL on a position basis, the PL that is registered to the account for the
        /// entire shares transacted. Takes into account the value of one pip for the current object, using the USD current point in time pip value
        /// </summary>
        /// <param name="existing">The existing position.</param>
        /// <param name="adjust">The portion of the position being changed/closed.</param>
        /// <returns></returns>
        public static decimal ClosePL(Position existing, Trade adjust)
        {
            int closedsize = Math.Abs(adjust.Xsize) > existing.UnsignedSize ? existing.UnsignedSize : Math.Abs(adjust.Xsize);

            decimal closedlots = closedsize / (decimal)existing.Security.LotSize;

            decimal pips = ClosePt(existing, adjust) / existing.Security.PipSize;

            return closedlots * pips * existing.Security.PipValue;
        }

        /// <summary>
        /// Gets the closed PL on a position basis, the PL that is registered to the account for the
        /// entire shares transacted. Incorporating the calculation of a pipvalue from another base currency in tick form.
        /// </summary>
        /// <param name="existing"></param>
        /// <param name="adjust"></param>
        /// <param name="accountcurrency"></param>
        /// <returns></returns>
        public static decimal ClosePL(Position existing, Trade adjust, Tick accountcurrency)
        {
            int closedsize = Math.Abs(adjust.Xsize) > existing.UnsignedSize ? existing.UnsignedSize : Math.Abs(adjust.Xsize);

            //((close_price - open_price) * Position size /(or *) Currency rate)
            return ClosePt(existing, adjust) * closedsize * (adjust.Direction == Direction.Long ? accountcurrency.Ask : accountcurrency.Bid);
        }

        /// <summary>
        /// Gets the closed PL on a position basis, the PL that is registered to the account for the
        /// entire shares transacted. Incorporating the calculation of a pipvalue from another base currency in bar data.
        /// </summary>
        /// <param name="existing"></param>
        /// <param name="adjust"></param>
        /// <param name="accountcurrency"></param>
        /// <returns></returns>
        public static decimal ClosePL(Position existing, Trade adjust, Bar accountcurrency)
        {
            int closedsize = Math.Abs(adjust.Xsize) > existing.UnsignedSize ? existing.UnsignedSize : Math.Abs(adjust.Xsize);

            //((close_price - open_price) * Position size /(or *) Currency rate)
            return ClosePt(existing, adjust) * closedsize * ((accountcurrency.High - accountcurrency.Low) / 2);
        }

        // these are for calculating closed pl they do not adjust positions themselves
        /// <summary>
        /// Gets the closed PL on a per-share basis, ignoring how many shares are held.
        /// </summary>
        /// <param name="existing">The existing position.</param>
        /// <param name="adjust">The portion of the position that's being closed/changed.</param>
        /// <returns></returns>
        public static decimal ClosePt(Position existing, Trade adjust)
        {
            if (!existing.IsValid || !adjust.IsValid)
                throw new Exception("Invalid position provided. (existing:" + existing + " adjustment:" + adjust);
            if (existing.IsFlat) return 0; // nothing to close
            if (existing.IsLong && adjust.Direction == Direction.Long) return 0; // if we're adding, nothing to close
            if (existing.IsShort && adjust.Direction == Direction.Short) return 0; // if we're adding, nothing to close
            return existing.IsLong ? adjust.Xprice - existing.AvgPrice : existing.AvgPrice - adjust.Xprice;
        }

        /// <summary>
        /// gets the most recent closing prices for a certain number of bars
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="bars"></param>
        /// <returns></returns>
        public static decimal[] Closes(BarList chart, int bars)
        {
            return EndSlice(chart.Close(), bars);
        }

        /// <summary>
        /// gets most recent closing prices for ALL bars, default bar interval
        /// </summary>
        /// <param name="chart"></param>
        /// <returns></returns>
        public static decimal[] Closes(BarList chart)
        {
            return chart.Close();
        }

        /// <summary>
        /// gets array of close to open ranges for default interval of a barlist
        /// </summary>
        /// <param name="chart"></param>
        /// <returns></returns>
        public static decimal[] CoRange(BarList chart)
        {
            List<decimal> l = new List<decimal>();
            foreach (BarImpl b in chart)
                l.Add(b.Close - b.Open);
            return l.ToArray();
        }

        /// <summary>
        /// Returns a bardate as an array of ints in the form [year,month,day]
        /// </summary>
        /// <param name="bardate">The bardate.</param>
        /// <returns></returns>
        public static int[] Date(int bardate)
        {
            int day = bardate % 100;
            int month = (bardate - day) / 100 % 100;
            int year = (bardate - month * 100 - day) / 10000;
            return new[] { year, month, day };
        }

        public static int[] Date(Bar bar)
        {
            return Date(bar.Bardate);
        }

        /// <summary>
        /// convert an array of decimals to less precise doubles
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double[] Decimal2Double(decimal[] array)
        {
            double[] vals = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
                vals[i] = (double)array[i];
            return vals;
        }

        /// <summary>
        /// convert an array of decimals to less precise doubles
        /// </summary>
        /// <param name="array"></param>
        /// <param name="vals"></param>
        public static void Decimal2Double(decimal[] array, ref double[] vals)
        {
            for (int i = 0; i < array.Length; i++)
                vals[i] = (double)array[i];
        }

        /// <summary>
        /// convert an array of decimals to less precise doubles
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double[] Decimal2Double_ref(ref decimal[] array)
        {
            double[] vals = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
                vals[i] = (double)array[i];
            return vals;
        }

        /// <summary>
        /// convert an array of decimals to less precise doubles
        /// </summary>
        /// <param name="array"></param>
        /// <param name="vals"></param>
        public static void Decimal2Double_ref(ref decimal[] array, ref double[] vals)
        {
            for (int i = 0; i < array.Length; i++)
                vals[i] = (double)array[i];
        }

        /// <summary>
        /// divides first array by second array.
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static double[] Divide(double[] array1, double[] array2)
        {
            return Divide(array1, array2, true);
        }

        public static double[] Divide(double[] array1, double[] array2, bool zeroIfundefined)
        {
            // normalize sizes of arrays
            bool a2Bigger = array1.Length < array2.Length;
            int max = a2Bigger ? array2.Length : array1.Length;
            long[] s1 = double2long(array1, max);
            long[] s2 = double2long(array2, max);
            double[] r = new double[max];
            // calculate values
            for (int i = 0; i < s1.Length; i++)
            {
                if (zeroIfundefined)
                    r[i] = s2[i] != 0 ? (double)s1[i] / s2[i] : 0;
                else
                    r[i] = (double)s1[i] / s2[i];
            }
            return r;
        }

        /// <summary>
        /// divides first array by second array.
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static decimal[] Divide(int[] array1, int[] array2)
        {
            return Divide(array1, array2, true);
        }

        public static decimal[] Divide(int[] array1, int[] array2, bool zeroIfundefined)
        {
            // normalize sizes of arrays
            bool a2Bigger = array1.Length < array2.Length;
            int max = a2Bigger ? array2.Length : array1.Length;
            int[] s1 = new int[max];
            int[] s2 = new int[max];
            decimal[] r = new decimal[max];
            Buffer.BlockCopy(array1, 0, s1, 0, array1.Length * 4);
            Buffer.BlockCopy(array2, 0, s2, 0, array2.Length * 4);
            // calculate values
            for (int i = 0; i < s1.Length; i++)
                if (zeroIfundefined)
                    r[i] = s2[i] != 0 ? (decimal)s1[i] / s2[i] : 0;
                else
                    r[i] = (decimal)s1[i] / s2[i];
            return r;
        }

        /// <summary>
        /// divides first array by second array.
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static decimal[] Divide(long[] array1, long[] array2)
        {
            return Divide(array1, array2, true);
        }

        public static decimal[] Divide(long[] array1, long[] array2, bool zeroIfundefined)
        {
            // normalize sizes of arrays
            bool a2Bigger = array1.Length < array2.Length;
            int max = a2Bigger ? array2.Length : array1.Length;
            long[] s1 = new long[max];
            long[] s2 = new long[max];
            decimal[] r = new decimal[max];
            Buffer.BlockCopy(array1, 0, s1, 0, array1.Length * 4);
            Buffer.BlockCopy(array2, 0, s2, 0, array2.Length * 4);
            // calculate values
            for (int i = 0; i < s1.Length; i++)
                if (zeroIfundefined)
                    r[i] = s2[i] != 0 ? (decimal)s1[i] / s2[i] : 0;
                else
                    r[i] = (decimal)s1[i] / s2[i];
            return r;
        }

        /// <summary>
        /// divides first array by second array.
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static decimal[] Divide(decimal[] array1, decimal[] array2)
        {
            return Divide(array1, array2, true);
        }

        public static decimal[] Divide(decimal[] array1, decimal[] array2, bool zeroIfundefined)
        {
            // normalize sizes of arrays
            bool a2Bigger = array1.Length < array2.Length;
            int max = a2Bigger ? array2.Length : array1.Length;
            long[] s1 = decimal2long(array1, max);
            long[] s2 = decimal2long(array2, max);
            decimal[] r = new decimal[max];
            // calculate values
            for (int i = 0; i < s1.Length; i++)
            {
                if (zeroIfundefined)
                    r[i] = s2[i] != 0 ? (decimal)s1[i] / s2[i] : 0;
                else
                    r[i] = (decimal)s1[i] / s2[i];
            }
            return r;
        }

        public static double[] Divide(double[] array, double val)
        {
            double[] r = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] / val;
            return r;
        }

        /// <summary>
        /// divides array by a constant
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static decimal[] Divide(int[] array, decimal val)
        {
            decimal[] r = new decimal[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] / val;
            return r;
        }

        public static decimal[] Divide(decimal[] array, decimal val)
        {
            decimal[] r = new decimal[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] / val;
            return r;
        }

        /// <summary>
        /// divides first array by second array.
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static double[] Divide2Double(int[] array1, int[] array2)
        {
            return Divide2Double(array1, array2, true);
        }

        public static double[] Divide2Double(int[] array1, int[] array2, bool zeroIfundefined)
        {
            // normalize sizes of arrays
            bool a2Bigger = array1.Length < array2.Length;
            int max = a2Bigger ? array2.Length : array1.Length;
            int[] s1 = new int[max];
            int[] s2 = new int[max];
            double[] r = new double[max];
            Buffer.BlockCopy(array1, 0, s1, 0, array1.Length * 4);
            Buffer.BlockCopy(array2, 0, s2, 0, array2.Length * 4);
            // calculate values
            for (int i = 0; i < s1.Length; i++)
                if (zeroIfundefined)
                    r[i] = s2[i] != 0 ? (double)s1[i] / s2[i] : 0;
                else
                    r[i] = (double)s1[i] / s2[i];
            return r;
        }

        /// <summary>
        /// divides first array by second array.
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static double[] Divide2Double(long[] array1, long[] array2)
        {
            return Divide2Double(array1, array2, true);
        }

        public static double[] Divide2Double(long[] array1, long[] array2, bool zeroIfundefined)
        {
            // normalize sizes of arrays
            bool a2Bigger = array1.Length < array2.Length;
            int max = a2Bigger ? array2.Length : array1.Length;
            long[] s1 = new long[max];
            long[] s2 = new long[max];
            double[] r = new double[max];
            Buffer.BlockCopy(array1, 0, s1, 0, array1.Length * 4);
            Buffer.BlockCopy(array2, 0, s2, 0, array2.Length * 4);
            // calculate values
            for (int i = 0; i < s1.Length; i++)
                if (zeroIfundefined)
                    r[i] = s2[i] != 0 ? (double)s1[i] / s2[i] : 0;
                else
                    r[i] = (double)s1[i] / s2[i];
            return r;
        }

        /// <summary>
        /// convert double array to decimal
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static decimal[] Double2Decimal(double[] a)
        {
            decimal[] b = new decimal[a.Length];
            for (int i = 0; i < a.Length; i++)
                b[i] = (decimal)a[i];
            return b;
        }

        /// <summary>
        /// convert double array to decimal
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static decimal[] Double2Decimal_ref(ref double[] a)
        {
            decimal[] b = new decimal[a.Length];
            for (int i = 0; i < a.Length; i++)
                b[i] = (decimal)a[i];
            return b;
        }

        /// <summary>
        /// Takes slice of last N elements of an array
        /// </summary>
        /// <param name="inputarray"></param>
        /// <param name="lastNumElements"></param>
        /// <returns></returns>
        public static double[] EndSlice(double[] inputarray, int lastNumElements)
        {
            int end = inputarray.Length >= lastNumElements ? inputarray.Length - lastNumElements : 0;
            double[] output = new double[inputarray.Length - end];
            int count = output.Length - 1;
            for (int i = inputarray.Length - 1; i >= end; i--)
                output[count--] = inputarray[i];
            return output;
        }

        /// <summary>
        /// Takes slice of last N elements of an array
        /// </summary>
        /// <param name="inputarray"></param>
        /// <param name="lastNumElements"></param>
        /// <returns></returns>
        public static decimal[] EndSlice(decimal[] inputarray, int lastNumElements)
        {
            int end = inputarray.Length >= lastNumElements ? inputarray.Length - lastNumElements : 0;
            decimal[] output = new decimal[inputarray.Length - end];
            int count = output.Length - 1;
            for (int i = inputarray.Length - 1; i >= end; i--)
                output[count--] = inputarray[i];
            return output;
        }

        /// <summary>
        /// Takes slice of last N elements of an array
        /// </summary>
        /// <param name="inputarray"></param>
        /// <param name="lastNumElements"></param>
        /// <returns></returns>
        public static int[] EndSlice(int[] inputarray, int lastNumElements)
        {
            int[] output = new int[lastNumElements];
            int count = lastNumElements - 1;
            int end = inputarray.Length >= lastNumElements ? inputarray.Length - lastNumElements : 0;
            for (int i = inputarray.Length - 1; i >= end; i--)
                output[count--] = inputarray[i];
            return output;
        }

        /// <summary>
        /// Takes slice of last N elements of an array
        /// </summary>
        /// <param name="inputarray"></param>
        /// <param name="lastNumElements"></param>
        /// <returns></returns>
        public static long[] EndSlice(long[] inputarray, int lastNumElements)
        {
            long[] output = new long[lastNumElements];
            int count = lastNumElements - 1;
            int end = inputarray.Length >= lastNumElements ? inputarray.Length - lastNumElements : 0;
            for (int i = inputarray.Length - 1; i >= end; i--)
                output[count--] = inputarray[i];
            return output;
        }

        public static double[] fillarray(double val, int len)
        {
            double[] a = new double[len];
            for (int i = 0; i < len; i++)
                a[i] = val;
            return a;
        }

        /// <summary>
        /// fill an array with a value
        /// </summary>
        /// <param name="val"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static decimal[] fillarray(decimal val, int len)
        {
            decimal[] a = new decimal[len];
            for (int i = 0; i < len; i++)
                a[i] = val;
            return a;
        }

        /// <summary>
        /// fill an array with a value
        /// </summary>
        /// <param name="val"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static int[] fillarray(int val, int len)
        {
            int[] a = new int[len];
            for (int i = 0; i < len; i++)
                a[i] = val;
            return a;
        }

        public static double HH(double[] array)
        {
            return Max(array);
        }

        /// <summary>
        /// Returns the highest-high of the barlist, for so many bars back.
        /// </summary>
        /// <param name="b">The barlist.</param>
        /// <param name="barsback">The barsback to consider.</param>
        /// <returns></returns>
        public static decimal HH(BarList b, int barsback)
        {// gets highest high
            return Max(EndSlice(b.High(), barsback));
        }

        /// <summary>
        /// highest high of array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static decimal HH(decimal[] array)
        {
            return Max(array);
        }

        /// <summary>
        /// Returns the highest high for the entire barlist.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public static decimal HH(BarList b)
        {
            return Max(b.High());
        }

        /// <summary>
        /// gets most recent number of highs from a barlist
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="bars"></param>
        /// <returns></returns>
        public static decimal[] Highs(BarList chart, int bars)
        {
            return EndSlice(chart.High(), bars);
        }

        public static decimal[] Highs(BarList chart)
        {
            return chart.High();
        }

        /// <summary>
        /// gets the high to low range of a barlist, for the default interval
        /// </summary>
        /// <param name="chart"></param>
        /// <returns></returns>
        public static decimal[] HlRange(BarList chart)
        {
            List<decimal> l = new List<decimal>();
            foreach (BarImpl b in chart)
                l.Add(b.High - b.Low);
            return l.ToArray();
        }

        /// <summary>
        /// lowest low of array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double LL(double[] array)
        {
            return Min(array);
        }

        /// <summary>
        /// lowest low of array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static decimal LL(decimal[] array)
        {
            return Min(array);
        }

        /// <summary>
        /// The lowest low for the barlist, considering so many bars back.
        /// </summary>
        /// <param name="b">The barlist.</param>
        /// <param name="barsback">The barsback to consider.</param>
        /// <returns></returns>
        public static decimal LL(BarList b, int barsback)
        { // gets lowest low
            return Min(EndSlice(b.Low(), barsback));
        }

        /// <summary>
        /// Lowest low for the entire barlist.
        /// </summary>
        /// <param name="b">The barlist.</param>
        /// <returns></returns>
        public static decimal LL(BarList b)
        {
            return Min(b.Low());
        }

        /// <summary>
        /// gets most recent lows from barlist, for certain number of bars (default is entire list)
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="bars"></param>
        /// <returns></returns>
        public static decimal[] Lows(BarList chart, int bars)
        {
            return EndSlice(chart.Low(), bars);
        }

        /// <summary>
        /// gets ALL lows from barlist, at default bar interval
        /// </summary>
        /// <param name="chart"></param>
        /// <returns></returns>
        public static decimal[] Lows(BarList chart)
        {
            return chart.Low();
        }

        /// <summary>
        /// gets maximum in an array (will return MaxValue if array has no elements)
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double Max(double[] array)
        {
            return array.Length > 0 ? array.Max() : double.MaxValue;
        }

        /// <summary>
        /// gets maximum in an array (will return MaxValue if array has no elements)
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static decimal Max(decimal[] array)
        {
            return array.Length > 0 ? array.Max() : decimal.MaxValue;
        }

        /// <summary>
        /// gets maximum in an array (will return MaxValue if array has no elements)
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static int Max(int[] array)
        {
            return array.Length > 0 ? array.Max() : int.MaxValue;
        }

        /// <summary>
        /// gets maximum in an array (will return MaxValue if array has no elements)
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static long Max(long[] array)
        {
            return array.Length > 0 ? array.Max() : long.MaxValue;
        }

        /// <summary>
        /// maximum drawdown as a percentage
        /// </summary>
        /// <param name="fills"></param>
        /// <returns></returns>
        public static decimal MaxDDPct(List<Trade> fills)
        {
            PositionTracker pt = new PositionTracker(null);
            List<decimal> ret = new List<decimal>();
            decimal mmiu = 0;
            for (int i = 0; i < fills.Count; i++)
            {
                pt.Adjust(fills[i]);
                decimal miu = Sum(MoneyInUse(pt));
                if (miu > mmiu)
                    mmiu = miu;
                ret.Add(Sum(AbsoluteReturn(pt, new decimal[0], true, false)));
            }
            decimal maxddval = MaxDdVal(ret.ToArray());
            decimal pct = mmiu == 0 ? 0 : maxddval / mmiu;
            return pct;
        }

        /// <summary>
        /// maximum drawdown as a percentage from the initial capital
        /// </summary>
        /// <param name="fills"></param>
        /// <param name="initialcapital"></param>
        /// <returns></returns>
        public static decimal MaxDDPct(List<Trade> fills, decimal initialcapital)
        {
            PositionTracker pt = new PositionTracker(null);
            List<decimal> ret = new List<decimal>();
            for (int i = 0; i < fills.Count; i++)
            {
                pt.Adjust(fills[i]);
                ret.Add(Sum(AbsoluteReturn(pt, new decimal[0], true, false)));
            }
            decimal maxddval = MaxDdVal(ret.ToArray());
            decimal pct = initialcapital == 0 ? 0 : maxddval / initialcapital;
            return pct;
        }

        /// <summary>
        /// maximum drawdown based on individual percentages
        /// </summary>
        /// <param name="fills"></param>
        /// <param name="initialcapital"></param>
        /// <returns></returns>
        public static decimal MaxDDPct(decimal[] pcreturns)
        {
            if (pcreturns == null || pcreturns.Length == 0)
                return 0;

            //Get drawdown based on percentages
            decimal currentdd = 0;
            decimal calculateddd = 0;

            for (int i = 0; i < pcreturns.Length; i++)
            {
                calculateddd += pcreturns[i];

                if (calculateddd <= 0)
                    currentdd = calculateddd;
                else
                    calculateddd = 0;
            }

            return currentdd;
        }

        /// <summary>
        /// calculate maximum drawdown from a PL stream for a given security/portfolio as a dollar value
        /// </summary>
        /// <param name="ret">array containing pl values for portfolio or security</param>
        /// <returns></returns>
        public static decimal MaxDdVal(decimal[] ret)
        {
            int maxi = 0;
            int prevmaxi = 0;
            int prevmini = 0;
            for (int i = 0; i < ret.Length; i++)
            {
                if (ret[i] >= ret[maxi])
                    maxi = i;
                else
                {
                    if (ret[maxi] - ret[i] > ret[prevmaxi] - ret[prevmini])
                    {
                        prevmaxi = maxi;
                        prevmini = i;
                    }
                }
            }
            return ret[prevmini] - ret[prevmaxi];
        }

        /// <summary>
        /// gets minum of an array (will return MinValue if array has no elements)
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double Min(double[] array)
        {
            return array.Length > 0 ? array.Min() : double.MinValue;
        }

        /// <summary>
        /// gets minum of an array (will return MinValue if array has no elements)
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static decimal Min(decimal[] array)
        {
            return array.Length > 0 ? array.Min() : decimal.MinValue;
        }

        /// <summary>
        /// gets minimum of an array (will return MinValue if array has no elements)
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static int Min(int[] array)
        {
            return array.Length > 0 ? array.Min() : int.MinValue;
        }

        /// <summary>
        /// gets minimum of an array (will return MinValue if array has no elements)
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static long Min(long[] array)
        {
            return array.Length > 0 ? array.Min() : long.MinValue;
        }

        /// <summary>
        /// computes money used to purchase a portfolio of positions. uses average price for position.
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static decimal[] MoneyInUse(PositionTracker pt)
        {
            decimal[] miu = new decimal[pt.Count];
            for (int i = 0; i < pt.Count; i++)
                miu[i] += pt[i].AvgPrice * pt[i].UnsignedSize;
            return miu;
        }

        public static double[] MoneyInUse2Double(PositionTracker pt)
        {
            double[] miu = new double[pt.Count];
            for (int i = 0; i < pt.Count; i++)
                miu[i] += (double)pt[i].AvgPrice * pt[i].UnsignedSize;
            return miu;
        }

        /// <summary>
        /// Normalizes any order size to the minimum lot size specified by MinSize.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        public static int Norm2Min(decimal size, int minsize)
        {
            return Norm2Min(size, minsize, true);
        }

        public static int Norm2Min(decimal size, int minsize, bool roundup)
        {
            int sign = size >= 0 ? 1 : -1;
            int mult = (int)Math.Floor(size / minsize);
            if (roundup)
            {
                mult = (int)Math.Ceiling(size / minsize);
            }

            int result = mult * minsize;
            int final = sign * Math.Max(Math.Abs(result), minsize);
            return final;
        }

        /// <summary>
        /// Provides an offsetting price from a position.
        /// </summary>
        /// <param name="p">Position</param>
        /// <param name="offset">Offset amount</param>
        /// <returns>Offset price</returns>
        public static decimal OffsetPrice(Position p, decimal offset)
        {
            return OffsetPrice(p.AvgPrice, p.IsLong ? Direction.Long : Direction.Short, offset);
        }

        public static decimal OffsetPrice(decimal avgPrice, Direction direction, decimal offset)
        {
            return direction == Direction.Long ? avgPrice + offset : avgPrice - offset;
        }

        /// <summary>
        /// Gets the open PL considering all the shares held in a position.
        /// </summary>
        /// <param name="lastTrade">The last trade.</param>
        /// <param name="avgPrice">The avg price.</param>
        /// <param name="posSize">Size of the pos.</param>
        /// <returns></returns>
        public static decimal OpenPL(decimal lastTrade, decimal avgPrice, int posSize)
        {
            return posSize * (lastTrade - avgPrice);
        }

        /// <summary>
        /// get open pl for position given the last trade
        /// </summary>
        /// <param name="lastTrade"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static decimal OpenPL(decimal lastTrade, Position pos)
        {
            return OpenPL(lastTrade, pos.AvgPrice, pos.Size);
        }

        /// <summary>
        /// Gets the open PL on a per-share basis, ignoring the size of the position.
        /// </summary>
        /// <param name="lastTrade">The last trade.</param>
        /// <param name="avgPrice">The avg price.</param>
        /// <param name="posSize">Size of the pos.</param>
        /// <returns></returns>
        public static decimal OpenPT(decimal lastTrade, decimal avgPrice, int posSize)
        {
            return posSize == 0 ? 0 : OpenPT(lastTrade, avgPrice, posSize > 0 ? Direction.Long : Direction.Short);
        }

        /// <summary>
        /// Gets the open PL on a per-share basis (also called points or PT), ignoring the size of
        /// the position.
        /// </summary>
        /// <param name="lastTrade">The last trade.</param>
        /// <param name="avgPrice">The avg price.</param>
        /// <param name="direction">Direction of the position (Long or Short).</param>
        /// <returns></returns>
        public static decimal OpenPT(decimal lastTrade, decimal avgPrice, Direction direction)
        {
            return direction == Direction.Long ? lastTrade - avgPrice : avgPrice - lastTrade;
        }

        public static decimal OpenPT(decimal lastTrade, Position pos)
        {
            return OpenPT(lastTrade, pos.AvgPrice, pos.Size);
        }

        /// <summary>
        /// multiplies two arrays
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static int[] Product(int[] array1, int[] array2)
        {
            // normalize sizes of arrays
            bool a2Bigger = array1.Length < array2.Length;
            int max = a2Bigger ? array2.Length : array1.Length;
            int[] s1 = new int[max];
            int[] s2 = new int[max];
            Buffer.BlockCopy(array1, 0, s1, 0, array1.Length * 4);
            Buffer.BlockCopy(array2, 0, s2, 0, array2.Length * 4);
            // calculate values
            for (int i = 0; i < s1.Length; i++)
                s2[i] *= s1[i];
            return s2;
        }

        /// <summary>
        /// multiplies two arrays
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static decimal[] Product(decimal[] array1, decimal[] array2)
        {
            // normalize sizes of arrays
            bool a2Bigger = array1.Length < array2.Length;
            int max = a2Bigger ? array2.Length : array1.Length;
            long[] s1 = decimal2long(array1, max);
            long[] s2 = decimal2long(array2, max);
            // calculate values
            for (int i = 0; i < s1.Length; i++)
                s2[i] *= s1[i];
            return Long2Decimalp(s2);
        }

        /// <summary>
        /// multiplies two arrays
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static double[] Product(double[] array1, double[] array2)
        {
            // normalize sizes of arrays
            bool a2Bigger = array1.Length < array2.Length;
            int max = a2Bigger ? array2.Length : array1.Length;
            long[] s1 = double2long(array1, max);
            long[] s2 = double2long(array2, max);
            // calculate values
            for (int i = 0; i < s1.Length; i++)
                s2[i] *= s1[i];
            return Long2Doublep(s2);
        }

        /// <summary>
        /// takes product of constant and an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static double[] Product(int[] array, double val)
        {
            double[] r = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] * val;
            return r;
        }

        /// <summary>
        /// takes product of constant and an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static double[] Product(double[] array, double val)
        {
            double[] r = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] * val;
            return r;
        }

        /// <summary>
        /// takes product of constant and an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static double[] Product(long[] array, double val)
        {
            double[] r = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] * val;
            return r;
        }

        /// <summary>
        /// takes product of constant and an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static decimal[] Product(int[] array, decimal val)
        {
            decimal[] r = new decimal[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] * val;
            return r;
        }

        /// <summary>
        /// takes product of constant and an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static decimal[] Product(long[] array, decimal val)
        {
            decimal[] r = new decimal[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] * val;
            return r;
        }

        /// <summary>
        /// takes product of constant and an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static int[] Product(int[] array, int val)
        {
            int[] r = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] * val;
            return r;
        }

        /// <summary>
        /// takes product of constant and an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static long[] Product(long[] array, long val)
        {
            long[] r = new long[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] * val;
            return r;
        }

        /// <summary>
        /// takes product of constant and an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static decimal[] Product(decimal[] array, decimal val)
        {
            decimal[] r = new decimal[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] * val;
            return r;
        }

        /// <summary>
        /// multiplies two arrays
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static long[] ProductBig(int[] array1, int[] array2)
        {
            // normalize sizes of arrays
            bool a2Bigger = array1.Length < array2.Length;
            int max = a2Bigger ? array2.Length : array1.Length;
            int[] s1 = new int[max];
            int[] s2 = new int[max];
            long[] s3 = new long[max];
            Buffer.BlockCopy(array1, 0, s1, 0, array1.Length * 4);
            Buffer.BlockCopy(array2, 0, s2, 0, array2.Length * 4);
            // calculate values
            for (int i = 0; i < s1.Length; i++)
                s3[i] = s2[i] * s1[i];
            return s3;
        }

        /// <summary>
        /// multiplies two arrays
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static long[] ProductBig(long[] array1, long[] array2)
        {
            // normalize sizes of arrays
            bool a2Bigger = array1.Length < array2.Length;
            int max = a2Bigger ? array2.Length : array1.Length;
            long[] s1 = new long[max];
            long[] s2 = new long[max];
            long[] s3 = new long[max];
            Buffer.BlockCopy(array1, 0, s1, 0, array1.Length * 4);
            Buffer.BlockCopy(array2, 0, s2, 0, array2.Length * 4);
            // calculate values
            for (int i = 0; i < s1.Length; i++)
                s3[i] = s2[i] * s1[i];
            return s3;
        }

        /// <summary>
        /// calculate a percentage return based upon a given amount of money used and the absolute
        /// return for this money, for each respective securtiy in a portfolio.
        /// </summary>
        /// <param name="moneyInUse"></param>
        /// <param name="absoluteReturn"></param>
        /// <returns></returns>
        public static double[] RateOfReturn(double[] moneyInUse, double[] absoluteReturn)
        {
            if (moneyInUse.Length != absoluteReturn.Length)
                throw new Exception("Money in use and Absolute return must have 1:1 security correspondence");
            double[] ror = new double[moneyInUse.Length];
            for (int i = 0; i < moneyInUse.Length; i++)
                ror[i] = absoluteReturn[i] / moneyInUse[i];
            return ror;
        }

        /// <summary>
        /// calculate a percentage return based upon a given amount of money used and the absolute
        /// return for this money, for each respective securtiy in a portfolio.
        /// </summary>
        /// <param name="moneyInUse"></param>
        /// <param name="absoluteReturn"></param>
        /// <returns></returns>
        public static decimal[] RateOfReturn(decimal[] moneyInUse, decimal[] absoluteReturn)
        {
            if (moneyInUse.Length != absoluteReturn.Length)
                throw new Exception("Money in use and Absolute return must have 1:1 security correspondence");
            decimal[] ror = new decimal[moneyInUse.Length];
            for (int i = 0; i < moneyInUse.Length; i++)
                ror[i] = absoluteReturn[i] / moneyInUse[i];
            return ror;
        }

        /// <summary>
        /// round number to nearest decimal places (eg MINTICK)
        /// </summary>
        /// <param name="num"></param>
        /// <param name="round2Nearest"></param>
        /// <returns></returns>
        public static decimal Round2Decimals(decimal num, decimal round2Nearest)
        {
            return Math.Round(num * (1 / round2Nearest)) / (1 / round2Nearest);
        }

        public static double SharpeRatio(double ratereturn, double stdevRate, double riskFreeRate)
        {
            return (ratereturn - riskFreeRate) / stdevRate;
        }

        /// <summary>
        /// computes sharpe ratio for a constant rate of risk free returns, give portfolio rate of
        /// return and portfolio volatility
        /// </summary>
        /// <param name="ratereturn"></param>
        /// <param name="stdevRate"></param>
        /// <param name="riskFreeRate"></param>
        /// <returns></returns>
        public static decimal SharpeRatio(decimal ratereturn, decimal stdevRate, decimal riskFreeRate)
        {
            return (ratereturn - riskFreeRate) / stdevRate;
        }

        /// <summary>
        /// takes slice of first N elements of array
        /// </summary>
        /// <param name="input"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static double[] Slice(double[] input, int count)
        {
            return Slice(input, 0, count);
        }

        /// <summary>
        /// takes slice of any N elements of array
        /// </summary>
        /// <param name="input"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static double[] Slice(double[] input, int start, int count)
        {
            int len = count < input.Length ? count : input.Length;
            double[] o = new double[len];
            if (start > input.Length) return o;
            for (int i = start; i < start + len; i++)
                o[i - start] = input[i];
            return o;
        }

        /// <summary>
        /// takes slice of first N elements of array
        /// </summary>
        /// <param name="a"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static long[] Slice(long[] a, int count)
        {
            return Slice(a, 0, count);
        }

        /// <summary>
        /// takes slice of some N elements of array
        /// </summary>
        /// <param name="a"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static long[] Slice(long[] input, int start, int count)
        {
            int len = count < input.Length ? count : input.Length;
            long[] o = new long[len];
            if (start > input.Length)
                return o;
            for (int i = start; i < start + len; i++)
                o[i - start] = input[i];
            return o;
        }

        /// <summary>
        /// takes slice of first N elements of array
        /// </summary>
        /// <param name="input"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int[] Slice(int[] input, int count)
        {
            return Slice(input, 0, count);
        }

        /// <summary>
        /// takes slice of any N elements of array
        /// </summary>
        /// <param name="input"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int[] Slice(int[] input, int start, int count)
        {
            int len = count < input.Length ? count : input.Length;
            int[] o = new int[len];
            if (start > input.Length) return o;
            for (int i = start; i < start + len; i++)
                o[i - start] = input[i];
            return o;
        }

        /// <summary>
        /// takes slice of first N elements of array
        /// </summary>
        /// <param name="input"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static decimal[] Slice(decimal[] input, int count)
        {
            return Slice(input, 0, count);
        }

        /// <summary>
        /// takes slice of any N elements of array
        /// </summary>
        /// <param name="input"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static decimal[] Slice(decimal[] input, int start, int count)
        {
            int len = count < input.Length ? count : input.Length;
            decimal[] o = new decimal[len];
            if (start > input.Length) return o;
            for (int i = start; i < start + len; i++)
                o[i - start] = input[i];
            return o;
        }

        public static double SortinoRatio(double ratereturn, double stdevRateDownside, double riskFreeRate)
        {
            return (ratereturn - riskFreeRate) / stdevRateDownside;
        }

        /// <summary>
        /// computes sortinio ratio for constant rate of risk free return, give portfolio rate of
        /// return and downside volatility
        /// </summary>
        /// <param name="ratereturn"></param>
        /// <param name="stdevRateDownside"></param>
        /// <param name="riskFreeRate"></param>
        /// <returns></returns>
        public static decimal SortinoRatio(decimal ratereturn, decimal stdevRateDownside, decimal riskFreeRate)
        {
            return (ratereturn - riskFreeRate) / stdevRateDownside;
        }

        /// <summary>
        /// gets standard deviation for values of a population
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static decimal StdDev(int[] array)
        {
            decimal avg = Avg(array);
            decimal sq = SumSquares(array);
            decimal tmp = sq / array.Length - avg * avg;
            decimal stdev = (decimal)Math.Pow((double)tmp, .5);
            return stdev;
        }

        /// <summary>
        /// gets standard deviation for values of a population
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static decimal StdDev(long[] array)
        {
            decimal avg = Avg(array);
            decimal sq = SumSquares(array);
            decimal tmp = sq / array.Length - avg * avg;
            decimal stdev = (decimal)Math.Pow((double)tmp, .5);
            return stdev;
        }

        public static decimal StdDev(decimal[] array)
        {
            decimal avg = Avg(array);
            decimal sq = SumSquares(array);
            decimal tmp = sq / array.Length - avg * avg;
            decimal stdev = (decimal)Math.Pow((double)tmp, .5);
            return stdev;
        }

        public static double StdDevSam(double[] array)
        {
            double avg = Avg(array);
            double[] var = Subtract(array, avg);
            double[] varsq = Product(var, var);
            double sumvar = Sum(varsq);
            double stdev = Math.Pow(sumvar / (array.Length - 1), .5);
            return stdev;
        }

        /// <summary>
        /// gets standard deviation for values of a sample
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static decimal StdDevSam(int[] array)
        {
            decimal avg = Avg(array);
            decimal[] var = Subtract(array, avg);
            decimal[] varsq = Product(var, var);
            decimal sumvar = Sum(varsq);
            decimal stdev = (decimal)Math.Pow((double)sumvar / (array.Length - 1), .5);
            return stdev;
        }

        /// <summary>
        /// gets standard deviation for values of a sample
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static decimal StdDevSam(long[] array)
        {
            decimal avg = Avg(array);
            decimal[] var = Subtract(array, avg);
            decimal[] varsq = Product(var, var);
            decimal sumvar = Sum(varsq);
            decimal stdev = (decimal)Math.Pow((double)sumvar / (array.Length - 1), .5);
            return stdev;
        }

        public static decimal StdDevSam(decimal[] array)
        {
            decimal avg = Avg(array);
            decimal[] var = Subtract(array, avg);
            decimal[] varsq = Product(var, var);
            decimal sumvar = Sum(varsq);
            decimal stdev = (decimal)Math.Pow((double)sumvar / (array.Length - 1), .5);
            return stdev;
        }

        /// <summary>
        /// subtracts 2nd array from first array
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static int[] Subtract(int[] array1, int[] array2)
        {
            // normalize sizes of arrays
            bool a2Bigger = array1.Length < array2.Length;
            int max = a2Bigger ? array2.Length : array1.Length;
            int[] s1 = new int[max];
            int[] s2 = new int[max];
            Buffer.BlockCopy(array1, 0, s1, 0, array1.Length * 4);
            Buffer.BlockCopy(array2, 0, s2, 0, array2.Length * 4);
            // calculate values
            for (int i = 0; i < s1.Length; i++)
                s1[i] -= s2[i];
            return s1;
        }

        /// <summary>
        /// subtracts 2nd array from first array
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static long[] Subtract(long[] array1, long[] array2)
        {
            // normalize sizes of arrays
            bool a2Bigger = array1.Length < array2.Length;
            int max = a2Bigger ? array2.Length : array1.Length;
            long[] s1 = new long[max];
            long[] s2 = new long[max];
            Buffer.BlockCopy(array1, 0, s1, 0, array1.Length * 4);
            Buffer.BlockCopy(array2, 0, s2, 0, array2.Length * 4);
            // calculate values
            for (int i = 0; i < s1.Length; i++)
                s1[i] -= s2[i];
            return s1;
        }

        /// <summary>
        /// subtracts 2nd array from first array
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static decimal[] Subtract(decimal[] array1, decimal[] array2)
        {
            // normalize sizes of arrays
            bool a2Bigger = array1.Length < array2.Length;
            int max = a2Bigger ? array2.Length : array1.Length;
            long[] s1 = decimal2long(array1, max);
            long[] s2 = decimal2long(array2, max);
            // calculate values
            for (int i = 0; i < s1.Length; i++)
                s1[i] -= s2[i];
            return long2decimal(s1);
        }

        /// <summary>
        /// subtracts 2nd array from first array
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static double[] Subtract(double[] array1, double[] array2)
        {
            // normalize sizes of arrays
            bool a2Bigger = array1.Length < array2.Length;
            int max = a2Bigger ? array2.Length : array1.Length;
            long[] s1 = double2long(array1, max);
            long[] s2 = double2long(array2, max);
            // calculate values
            for (int i = 0; i < s1.Length; i++)
                s1[i] -= s2[i];
            return long2double(s1);
        }

        public static double[] Subtract(double[] array, double val)
        {
            double[] r = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] - val;
            return r;
        }

        /// <summary>
        /// subtracts constant from an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static decimal[] Subtract(int[] array, decimal val)
        {
            decimal[] r = new decimal[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] - val;
            return r;
        }

        /// <summary>
        /// subtracts constant from an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static decimal[] Subtract(long[] array, decimal val)
        {
            decimal[] r = new decimal[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] - val;
            return r;
        }

        /// <summary>
        /// subtracts constant from an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static double[] Subtract(int[] array, double val)
        {
            double[] r = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] - val;
            return r;
        }

        /// <summary>
        /// subtracts constant from an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static double[] Subtract(long[] array, double val)
        {
            double[] r = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] - val;
            return r;
        }

        /// <summary>
        /// subtracts constant from an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static int[] Subtract(int[] array, int val)
        {
            int[] r = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] - val;
            return r;
        }

        /// <summary>
        /// subtracts constant from an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static long[] Subtract(long[] array, long val)
        {
            long[] r = new long[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] - val;
            return r;
        }

        /// <summary>
        /// subtracts constant from an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static decimal[] Subtract(decimal[] array, decimal val)
        {
            decimal[] r = new decimal[array.Length];
            for (int i = 0; i < array.Length; i++)
                r[i] = array[i] - val;
            return r;
        }

        /// <summary>
        /// sum last elements of array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="barsback"></param>
        /// <returns></returns>
        public static decimal Sum(decimal[] array, int barsback)
        {
            return Sum(array, array.Length - barsback, barsback);
        }

        /// <summary>
        /// sum part of an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="startindex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static decimal Sum(decimal[] array, int startindex, int length)
        {
            decimal sum = 0;
            for (int i = startindex; i < startindex + length; i++)
                sum += array[i];
            return sum;
        }

        /// <summary>
        /// gets sum of entire array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static decimal Sum(decimal[] array)
        {
            return Sum(array, 0, array.Length);
        }

        /// <summary>
        /// sum last elements of array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="barsback"></param>
        /// <returns></returns>
        public static long Sum(int[] array, int barsback)
        {
            return Sum(array, array.Length - barsback, barsback);
        }

        /// <summary>
        /// sum part of an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="startindex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static long Sum(int[] array, int startindex, int length)
        {
            long sum = 0;
            for (int i = startindex; i < startindex + length; i++)
                sum += array[i];
            return sum;
        }

        /// <summary>
        /// sum part of an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="startindex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static long Sum(long[] array, int startindex, int length)
        {
            long sum = 0;
            for (int i = startindex; i < startindex + length; i++)
                sum += array[i];
            return sum;
        }

        /// <summary>
        /// gets sum of entire array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static long Sum(long[] array)
        {
            return Sum(array, 0, array.Length);
        }

        /// <summary>
        /// gets sum of entire array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static long Sum(int[] array)
        {
            return Sum(array, 0, array.Length);
        }

        /// <summary>
        /// sum last elements of array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="barsback"></param>
        /// <returns></returns>
        public static double Sum(double[] array, int barsback)
        {
            return Sum(array, array.Length - barsback, barsback);
        }

        /// <summary>
        /// sum part of an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="startindex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static double Sum(double[] array, int startindex, int length)
        {
            double sum = 0;
            for (int i = startindex; i < startindex + length; i++)
                sum += array[i];
            return sum;
        }

        /// <summary>
        /// gets sum of entire array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double Sum(double[] array)
        {
            return Sum(array, 0, array.Length);
        }

        /// <summary>
        /// gets sum of squares for end of an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="barsback"></param>
        /// <returns></returns>
        public static decimal SumSquares(decimal[] array, int barsback)
        {
            return SumSquares(array, array.Length - barsback, barsback);
        }

        /// <summary>
        /// get sums of squares for part of an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="startindex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static decimal SumSquares(decimal[] array, int startindex, int length)
        {
            decimal sum = 0;
            for (int i = startindex; i < startindex + length; i++)
                sum += array[i] * array[i];
            return sum;
        }

        /// <summary>
        /// gets sum of squares for entire array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static decimal SumSquares(decimal[] array)
        {
            return SumSquares(array, 0, array.Length);
        }

        /// <summary>
        /// gets sum of squares for end of an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="barsback"></param>
        /// <returns></returns>
        public static long SumSquares(int[] array, int barsback)
        {
            return SumSquares(array, array.Length - barsback, barsback);
        }

        /// <summary>
        /// get sums of squares for part of an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="startindex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static long SumSquares(int[] array, int startindex, int length)
        {
            long sum = 0;
            for (int i = startindex; i < startindex + length; i++)
                sum += array[i] * array[i];
            return sum;
        }

        /// <summary>
        /// get sums of squares for part of an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="startindex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static long SumSquares(long[] array, int startindex, int length)
        {
            long sum = 0;
            for (int i = startindex; i < startindex + length; i++)
                sum += array[i] * array[i];
            return sum;
        }

        /// <summary>
        /// gets sum of squares for entire array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static long SumSquares(int[] array)
        {
            return SumSquares(array, 0, array.Length);
        }

        /// <summary>
        /// gets sum of squares for entire array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static long SumSquares(long[] array)
        {
            return SumSquares(array, 0, array.Length);
        }

        /// <summary>
        /// gets sum of squares for end of an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="barsback"></param>
        /// <returns></returns>
        public static double SumSquares(double[] array, int barsback)
        {
            return SumSquares(array, array.Length - barsback, barsback);
        }

        /// <summary>
        /// get sums of squares for part of an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="startindex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static double SumSquares(double[] array, int startindex, int length)
        {
            double sum = 0;
            for (int i = startindex; i < startindex + length; i++)
                sum += array[i] * array[i];
            return sum;
        }

        /// <summary>
        /// gets sum of squares for entire array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double SumSquares(double[] array)
        {
            return SumSquares(array, 0, array.Length);
        }

        /// <summary>
        /// gets an array of true range values representing each bar in chart (uses default bar interval)
        /// </summary>
        /// <param name="chart"></param>
        /// <returns></returns>
        public static decimal[] TrueRange(BarList chart)
        {
            if (chart.Count < 2) return new decimal[0];

            decimal[] res = new decimal[chart.Count - 1];
            for (int i = 1; i < chart.Count; i++)
            {
                Bar t = chart[i];
                Bar p = chart[i - 1];
                decimal max = t.High > p.Close ? t.High : p.Close;
                decimal min = t.Low < p.Close ? t.Low : p.Close;
                res[i - 1] = max - min;
            }
            return res;
        }

        /// <summary>
        /// gets the most recent volumes from a barlist, given a certain number of bars back
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="bars"></param>
        /// <returns></returns>
        public static long[] Volumes(BarList chart, int bars)
        {
            List<long> l = new List<long>();
            for (int i = chart.Count - bars; i < chart.Count; i++)
                l.Add(chart[i].Volume);
            return l.ToArray();
        }

        /// <summary>
        /// gets volumes for ALL bars, with default bar interval
        /// </summary>
        /// <param name="chart"></param>
        /// <returns></returns>
        public static long[] Volumes(BarList chart)
        {
            return Volumes(chart, chart.Count);
        }

        #endregion Public Methods

        #region Private Methods

        private static long[] decimal2long(decimal[] a, int length)
        {
            long[] r = new long[length];
            for (int i = 0; i < length; i++)
                r[i] = (long)(a[i] * D2LMult);
            return r;
        }

        private static long[] double2long(double[] a, int length)
        {
            long[] r = new long[length];
            for (int i = 0; i < length; i++)
                r[i] = (long)(a[i] * D2LMult);
            return r;
        }

        private static decimal[] long2decimal(long[] a)
        {
            return long2decimal(a, a.Length);
        }

        private static decimal[] long2decimal(long[] a, int length)
        {
            decimal[] r = new decimal[length];
            for (int i = 0; i < length; i++)
                r[i] = (decimal)a[i] / D2LMult;
            return r;
        }

        private static decimal[] Long2Decimalp(long[] a)
        {
            decimal[] r = new decimal[a.Length];
            long m = D2LMult * D2LMult;
            for (int i = 0; i < a.Length; i++)
                r[i] = (decimal)a[i] / m;
            return r;
        }

        private static double[] long2double(long[] a)
        {
            return long2double(a, a.Length);
        }

        private static double[] long2double(long[] a, int length)
        {
            double[] r = new double[length];
            for (int i = 0; i < length; i++)
                r[i] = (double)a[i] / D2LMult;
            return r;
        }

        private static double[] Long2Doublep(long[] a)
        {
            double[] r = new double[a.Length];
            long m = D2LMult * D2LMult;
            for (int i = 0; i < a.Length; i++)
                r[i] = (double)a[i] / m;
            return r;
        }

        #endregion Private Methods
    }
}