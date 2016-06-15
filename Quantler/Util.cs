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
#endregion License

using NLog;
using Quantler.Data.TikFile;
using Quantler.Interfaces;
using Quantler.Tracker;
using Quantler.Trades;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Quantler
{
    /// <summary>
    /// Utility class holding commonly used properties
    /// </summary>
    public class Util
    {
        #region Private Fields

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Get current executing directory
        /// </summary>
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// gets fasttime time for now
        /// </summary>
        /// <returns></returns>
        public static int DT2FT()
        {
            return DT2FT(DateTime.Now);
        }

        /// <summary>
        /// converts datetime to fasttime format
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static int DT2FT(DateTime d)
        {
            return QL2FT(d.Hour, d.Minute, d.Second, d.Millisecond);
        }

        /// <summary>
        /// converts fasttime to a datetime
        /// </summary>
        /// <param name="ftime"></param>
        /// <returns></returns>
        public static DateTime Ft2Dt(int ftime)
        {
            int f = ftime % 1000;
            int s = (ftime - f) / 1000 % 100;
            int m = (ftime - f - s * 1000) / 100000 % 100;
            int h = (ftime - f - s * 1000 - m * 100000) / 10000000 % 100;
            return new DateTime(1, 1, 1, h, m, s).AddMilliseconds(f);
        }

        /// <summary>
        /// converts fasttime to fasttime span, or elapsed milliseconds
        /// </summary>
        /// <param name="fasttime"></param>
        /// <returns></returns>
        public static int Ft2Fts(int fasttime)
        {
            int f1 = fasttime % 1000;
            int s1 = (fasttime - f1) / 1000 % 100;
            int m1 = (fasttime - f1 - s1 * 1000) / 100000 % 100;
            int h1 = (fasttime - f1 - s1 * 1000 - m1 * 100000) / 10000000 % 100;
            return h1 * 3600000 + m1 * 60000 + s1 * 1000 + f1;
        }

        /// <summary>
        /// adds fasttime and fasttimespan (in milliseconds). does not rollover 24hr periods.
        /// </summary>
        /// <param name="firsttime"></param>
        /// <param name="secondtime"></param>
        /// <returns></returns>
        public static int Ftadd(int firsttime, int fasttimespaninmilliseconds)
        {
            int f1 = firsttime % 1000;
            int s1 = (firsttime - f1) / 1000 % 100;
            int m1 = (firsttime - f1 - s1 * 1000) / 100000 % 100;
            int h1 = (firsttime - f1 - s1 * 1000 - m1 * 100000) / 10000000 % 100;

            f1 += fasttimespaninmilliseconds;

            if (f1 > 999)
            {
                s1 += f1 / 1000;
                f1 = f1 % 1000;
            }
            if (s1 >= 60)
            {
                m1 += s1 / 60;
                s1 = s1 % 60;
            }
            if (m1 >= 60)
            {
                h1 += m1 / 60;
                m1 = m1 % 60;
            }
            int sum = h1 * 10000000 + m1 * 100000 + s1 * 1000 + f1;
            return sum;
        }

        /// <summary>
        /// gets elapsed seconds between two fasttimes
        /// </summary>
        /// <param name="firsttime"></param>
        /// <param name="latertime"></param>
        /// <returns></returns>
        public static int Ftdiff(int firsttime, int latertime)
        {
            int span1 = Ft2Fts(firsttime);
            int span2 = Ft2Fts(latertime);
            return span2 - span1;
        }

        /// <summary>
        /// Get symbol crosses used for calculating pip values
        /// </summary>
        /// <param name="basecurrency"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetPipValueSymbolCrosses(CurrencyType basecurrency)
        {
            Dictionary<string, string> toreturn = new Dictionary<string, string>();

            if (basecurrency == CurrencyType.USD)
            {
                toreturn.Add("EURJPY", "USDJPY");
                toreturn.Add("AUDJPY", "USDJPY");
                toreturn.Add("AUDUSD", "USDUSD");
                toreturn.Add("EURGBP", "USDGBP");
                toreturn.Add("EURUSD", "USDUSD");
                toreturn.Add("GBPJPY", "USDJPY");
                toreturn.Add("GBPUSD", "USDUSD");
                toreturn.Add("NZDUSD", "USDUSD");
                toreturn.Add("USDCAD", "USDCAD");
                toreturn.Add("USDCHF", "USDCHF");
                toreturn.Add("USDJPY", "USDJPY");
                toreturn.Add("EURCHF", "USDCHF");
                toreturn.Add("USDNZD", "USDUSD");
            }

            return toreturn;
        }

        /// <summary>
        /// Get symbol crosses used for calculating base currency values
        /// </summary>
        /// <param name="basecurrency"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetPositionValueSymbolCrosses(CurrencyType basecurrency)
        {
            Dictionary<string, string> toreturn = new Dictionary<string, string>();

            if (basecurrency == CurrencyType.USD)
            {
                toreturn.Add("EURJPY", "EURUSD");
                toreturn.Add("AUDJPY", "AUDUSD");
                toreturn.Add("AUDNZD", "AUDUSD");
                toreturn.Add("AUDUSD", "AUDUSD");
                toreturn.Add("EURGBP", "EURUSD");
                toreturn.Add("EURUSD", "EURUSD");
                toreturn.Add("GBPJPY", "GBPUSD");
                toreturn.Add("GBPUSD", "GBPUSD");
                toreturn.Add("NZDUSD", "NZDUSD");
                toreturn.Add("USDCAD", "USDUSD");
                toreturn.Add("USDCHF", "USDUSD");
                toreturn.Add("USDJPY", "USDUSD");
                toreturn.Add("EURCHF", "EURUSD");
            }

            return toreturn;
        }

        public static TickFileInfo ParseFile(string filepath)
        {
            TickFileInfo tfi;
            tfi.Type = TickFileType.Invalid;
            tfi.Date = DateTime.MinValue;
            tfi.Symbol = "";

            try
            {
                string fn = Path.GetFileNameWithoutExtension(filepath);
                string ext = Path.GetExtension(filepath).Replace(".", "");
                string date = Regex.Match(fn, "[0-9]{8}$").Value;
                tfi.Type = (TickFileType)Enum.Parse(typeof(TickFileType), ext.ToUpper());
                tfi.Date = Qld2Dt(Convert.ToInt32(date));
                tfi.Symbol = Regex.Match(fn, "^[A-Z]+").Value;
            }
            catch (Exception) { tfi.Type = TickFileType.Invalid; }
            return tfi;
        }

        /// <summary>
        /// converts Quantler time to fasttime
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="min"></param>
        /// <param name="sec"></param>
        /// <returns></returns>
        public static int QL2FT(int hour, int min, int sec, int milliseconds)
        {
            return hour * 10000000 + min * 100000 + sec * 1000 + milliseconds;
        }

        /// <summary>
        /// gets fasttime from a Quantler tick
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int QL2FT(Tick t)
        {
            return t.Time;
        }

        /// <summary>
        /// Converts Quantler date to DateTime (eg 20070926 to "DateTime.Mon = 9, DateTime.Day =
        /// 26, DateTime.ShortDate = Sept 29, 2007"
        /// </summary>
        /// <param name="quantlerDate"></param>
        /// <returns></returns>
        public static DateTime Qld2Dt(int quantlerDate)
        {
            if (quantlerDate < 10000) throw new Exception("Not a date, or invalid date provided");
            return ToDateTime(quantlerDate, 0);
        }

        /// <summary>
        /// Tests if two dates are the same, given a mask as DateMatchType.
        ///
        /// ie, 20070605 will match 20070705 if DateMatchType is Day or Year.
        /// </summary>
        /// <param name="fulldate">The fulldate in QLDate format (int).</param>
        /// <param name="matchdate">The matchdate to test against (int).</param>
        /// <param name="dmt">The "mask" that says what to pay attention to when matching.</param>
        /// <returns></returns>
        public static bool QlDateMatch(int fulldate, int matchdate, DateMatchType dmt)
        {
            const int d = 0, m = 1, y = 2;
            if (dmt == DateMatchType.None)
                return false;
            bool matched = true;
            // if we're requesting a day match,
            if ((dmt & DateMatchType.Day) == DateMatchType.Day)
                matched &= QlDateSplit(fulldate)[d] == QlDateSplit(matchdate)[d];
            if ((dmt & DateMatchType.Month) == DateMatchType.Month)
                matched &= QlDateSplit(fulldate)[m] == QlDateSplit(matchdate)[m];
            if ((dmt & DateMatchType.Year) == DateMatchType.Year)
                matched &= QlDateSplit(fulldate)[y] == QlDateSplit(matchdate)[y];
            return matched;
        }

        public static int Qldt2Qld(long datetime)
        {
            var rem = datetime % 1000000;
            var date = datetime - rem;
            date = date / 1000000;
            return (int)date;
        }

        public static int Qldt2Qlt(long datetime)
        {
            var rem = datetime % 1000000;
            return (int)rem;
        }

        /// <summary>
        /// Converts Quantler Time to DateTime. If not using seconds, put a zero.
        /// </summary>
        /// <param name="quantlerTime"></param>
        /// <returns></returns>
        public static DateTime QLT2DT(int quantlerTime)
        {
            return ToDateTime(0, quantlerTime);
        }

        /// <summary>
        /// gets datetime of a tick
        /// </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        public static DateTime QLT2DT(Tick k)
        {
            return ToDateTime(0, k.Time);
        }

        /// <summary>
        /// gets list of readable tickfiles in top level of a folder. 2nd dimension of list is size
        /// of file in bytes (as string)
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static string[,] TickFileIndex(string folder)
        {
            return TickFileIndex(folder, TikConst.WildcardExt);
        }

        /// <summary>
        /// builds list of readable tick files with given extension found in top level of folder
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="tickext"></param>
        /// <returns></returns>
        public static string[,] TickFileIndex(string folder, string tickext)
        {
            return TickFileIndex(folder, tickext, false);
        }

        /// <summary>
        /// builds list of readable tickfiles (and their byte-size) found in folder
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="tickext"></param>
        /// <param name="searchSubFolders"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        public static string[,] TickFileIndex(string folder, string tickext, bool searchSubFolders)
        {
            //string[] _tickfiles = Directory.GetFiles(Folder, tickext);
            DirectoryInfo di = new DirectoryInfo(folder);
            FileInfo[] fi = di.GetFiles(tickext, searchSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            string[,] index = new string[fi.Length, 2];
            int i = 0;
            int qtr = fi.Length / 4;
            foreach (FileInfo thisfi in fi)
            {
                if (i % qtr == 0)
                    Log.Debug((fi.Length - i).ToString("N0") + " files remaining to index...");
                index[i, 0] = thisfi.Name;
                index[i, 1] = thisfi.Length.ToString();
                i++;
            }
            return index;
        }

        /// <summary>
        /// Converts Quantler Date and Time format to a DateTime. eg DateTime ticktime = ToDateTime(tick.date,tick.time);
        /// </summary>
        /// <param name="quantlerDate"></param>
        /// <param name="quantlerTime"></param>
        /// <param name="QuantlerSec"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(int quantlerDate, int quantlerTime)
        {
            int msec = quantlerTime % 1000;
            int sec = quantlerTime % 100000 / 1000;
            int hm = quantlerTime % 10000000;
            int hour = (quantlerTime - hm) / 10000000;
            int min = (quantlerTime - hour * 10000000) / 100000;
            if (sec > 59) { sec -= 60; min++; }
            if (min > 59) { hour++; min -= 60; }
            int year = 1, day = 1, month = 1;
            if (quantlerDate != 0)
            {
                int ym = quantlerDate % 10000;
                year = (quantlerDate - ym) / 10000;
                int mm = ym % 100;
                month = (ym - mm) / 100;
                day = mm;
            }
            return new DateTime(year, month, day, hour, min, sec, msec);
        }

        /// <summary>
        /// gets Quantler date for today
        /// </summary>
        /// <returns></returns>
        public static int ToQLDate()
        {
            return ToQLDate(DateTime.Now);
        }

        /// <summary>
        /// Converts a DateTime to Quantler Date (eg July 11, 2006 = 20060711)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int ToQLDate(DateTime dt)
        {
            return dt.Year * 10000 + dt.Month * 100 + dt.Day;
        }

        /// <summary>
        /// Converts a DateTime.Ticks values to QLDate (eg 8million milliseconds since 1970 ~=
        /// 19960101 (new years 1996)
        /// </summary>
        /// <param name="dateTimeTicks"></param>
        /// <returns></returns>
        public static int ToQLDate(long dateTimeTicks)
        {
            return ToQLDate(new DateTime(dateTimeTicks));
        }

        /// <summary>
        /// get long for current date + time
        /// </summary>
        /// <returns></returns>
        public static long ToQLDateTime()
        {
            return (long)ToQLDate() * 1000000 + ToQLTime();
        }

        public static long ToQLDateTime(int date, int time)
        {
            return ToQLDateTime(ToDateTime(date, time));
        }

        /// <summary>
        /// get long for date + time
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long ToQLDateTime(DateTime dt)
        {
            return (long)ToQLDate(dt) * 1000000 + ToQLTime(dt);
        }

        /// <summary>
        /// gets Quantler time for now
        /// </summary>
        /// <returns></returns>
        public static int ToQLTime()
        {
            return DT2FT(DateTime.Now);
        }

        /// <summary>
        /// gets Quantler time from date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int ToQLTime(DateTime date)
        {
            return DT2FT(date);
        }

        /// <summary>
        /// Converts a list of trades to an array of comma-delimited string data also containing
        /// closedPL, suitable for output to file for reading by excel, R, matlab, etc.
        /// </summary>
        /// <param name="tradelist"></param>
        /// <returns></returns>
        public static string[] TradesToClosedPL(List<Trade> tradelist)
        {
            return TradesToClosedPL(tradelist, ',');
        }

        /// <summary>
        /// Converts a list of trades to an array of delimited string data also containing closedPL,
        /// suitable for output to file for reading by excel, R, matlab, etc.
        /// </summary>
        /// <param name="tradelist"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string[] TradesToClosedPL(List<Trade> tradelist, char delimiter)
        {
            List<string> rowoutput = new List<string>();
            PositionTracker pt = new PositionTracker(null);

            foreach (TradeImpl t in tradelist)
            {
                string r = t.ToString(delimiter, false) + delimiter;
                string s = t.Symbol;
                int csize = 0;
                var cpl = pt.Adjust(t);
                var opl = Calc.OpenPL(t.Xprice, pt[s]);
                if (cpl != 0) csize = t.Xsize; // if we closed any pl, get the size
                string[] pl = { opl.ToString("0.00#####", CultureInfo.InvariantCulture), cpl.ToString("0.00#####", CultureInfo.InvariantCulture), pt[s].Size.ToString(CultureInfo.InvariantCulture), csize.ToString(CultureInfo.InvariantCulture), pt[s].AvgPrice.ToString("0.00#####", CultureInfo.InvariantCulture) };
                r += string.Join(delimiter.ToString(), pl);
                rowoutput.Add(r);
            }
            return rowoutput.ToArray();
        }

        #endregion Public Methods

        #region Private Methods

        private static string GetResponse(HttpWebRequest req)
        {
            var stream2 = req.GetResponse().GetResponseStream();
            StreamReader reader2 = new StreamReader(stream2);
            return reader2.ReadToEnd();
        }

        /// <summary>
        /// Converts a QLDate integer format into an array of ints
        /// </summary>
        /// <param name="fulltime">The fulltime.</param>
        /// <returns>int[3] { year, month, day}</returns>
        private static int[] QlDateSplit(int fulltime)
        {
            int[] splittime = new int[3]; // year, month, day
            splittime[2] = (int)((double)fulltime / 10000);
            splittime[1] = (int)((double)(fulltime - splittime[2] * 10000) / 100);
            double tmp = (int)((double)fulltime / 100);
            double tmp2 = (double)fulltime / 100;
            splittime[0] = (int)(Math.Round(tmp2 - tmp, 2, MidpointRounding.AwayFromZero) * 100);
            return splittime;
        }

        /// <summary>
        /// Open a file and get the content of this file in string form
        /// </summary>
        /// <param name="fn"></param>
        /// <returns></returns>
        private static string Getfile(string fn)
        {
            try
            {
                StreamReader sr = new StreamReader(fn);
                string data = sr.ReadToEnd();
                try
                {
                    sr.Close();
                    sr.Dispose();
                }
                catch
                {
                    // ignored
                }
                return data;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "error reading: {0}", fn);
            }
            return string.Empty;
        }

        #endregion Private Methods
    }
}