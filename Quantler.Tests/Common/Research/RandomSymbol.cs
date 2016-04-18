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

using System;
using System.Collections.Generic;
using System.Text;

namespace Quantler.Tests.Common.Research
{
    /// <summary>
    /// used for generating random symbol names in studies. (eg if you want to randomly walk the
    /// security space) Whenever 'seed' is specified, use a randomized value... eg
    /// (int)DateTime.Now.Ticks or likewise
    /// </summary>
    public class RandomSymbol
    {
        #region Public Methods

        /// <summary>
        /// convert a list of ASCII integers to corresponding string
        /// </summary>
        /// <param name="codes"></param>
        /// <returns></returns>
        public static string Alphacodes2string(int[] codes)
        {
            StringBuilder s = new StringBuilder();
            foreach (int c in codes)
            {
                char ch = (char)(c + 65);
                s.Append(ch);
            }
            return s.ToString();
        }

        /// <summary>
        /// convert from base ten to another number system
        /// </summary>
        /// <param name="num"></param>
        /// <param name="destbase"></param>
        /// <returns></returns>
        public static int[] BaseTenConvert(long num, int destbase)
        {
            List<int> ordinals = new List<int>();
            if (destbase == 0) return ordinals.ToArray();
            long rem = num % destbase;
            int ans = (int)num / destbase;
            while (ans != 0)
            {
                ordinals.Add((int)rem);
                rem = ans % destbase;
                ans = (int)ans / destbase;
            }
            ordinals.Add((int)rem);
            return ordinals.ToArray();
        }

        /// <summary>
        /// gets a single random symbol.
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static string GetSymbol(int seed)
        {
            return GetSymbol(seed, 4);
        }

        /// <summary>
        /// gets a single random symbol with a specified maximum length
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="maxlength"></param>
        /// <returns></returns>
        public static string GetSymbol(int seed, int maxlength)
        {
            Random r = new Random();
            if (seed > 0)
                r = new Random(seed + DateTime.Now.DayOfYear + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond);
            string sym = "";
            int symbolinbaseten = r.Next(1, (int)Math.Pow(26, maxlength));
            int[] alphacodes = BaseTenConvert(symbolinbaseten, 26);
            sym = Alphacodes2string(alphacodes);
            return sym;
        }

        /// <summary>
        /// get a random list of symbols, given seed, maximum symbol length and desired number of
        /// symbols. (seed eg (int)DateTime.Now.Ticks
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="symlength"></param>
        /// <param name="symbolcount"></param>
        /// <returns></returns>
        public static string[] GetSymbols(int seed, int symlength, int symbolcount)
        {
            List<string> l = new List<string>();
            for (int i = 0; i < symbolcount; i++)
                l.Add(GetSymbol(seed + i, symlength));
            return l.ToArray();
        }

        #endregion Public Methods
    }
}