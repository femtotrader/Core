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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Quantler.Data.TikFile
{
    /// <summary>
    /// Filters tick files (EPF/IDX) based on symbol name and trading date.
    /// </summary>
    public class TickFileFilter
    {
        #region Private Fields

        private bool _allowedinvalid;
        private bool _defallowed;
        private bool _isDateMatchUnion = true;

        private bool _isSymDateMatchUnion = true;

        private List<QlDateFilter> _datelist = new List<QlDateFilter>();

        private List<string> _namelist = new List<string>();

        #endregion Private Fields

        #region Public Constructors

        public TickFileFilter()
            : this(new List<string>(), new List<QlDateFilter>())
        {
        }

        public TickFileFilter(List<string> namefilter)
            : this(namefilter, null)
        {
        }

        public TickFileFilter(List<QlDateFilter> datefilter)
            : this(null, datefilter)
        {
        }

        public TickFileFilter(List<string> namefilter, List<QlDateFilter> datefilter)
        {
            if ((namefilter != null) && (datefilter != null))
            {
                _namelist = namefilter;
                _datelist = datefilter;
                IsSymbolDateMatchUnion = false;
            }
            else if (namefilter != null)
            {
                _namelist = namefilter;
                IsSymbolDateMatchUnion = true;
            }
            else if (datefilter != null)
            {
                _datelist = datefilter;
                IsSymbolDateMatchUnion = true;
            }
        }

        public TickFileFilter(string[] symbols)
            : this(FilterSyms(symbols), null)
        {
        }

        #endregion Public Constructors

        #region Public Enums

        public enum MessageTickFileFilter
        {
            TffFiltertype = 0,
            TffFiltercontent = 1
        }

        #endregion Public Enums

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether the class will [allow invalid] tickfiles, which
        /// have undefined extensions.
        /// </summary>
        /// <value><c>true</c> if [allow invalid]; otherwise, <c>false</c>.</value>
        public bool AllowInvalid { get { return _allowedinvalid; } set { _allowedinvalid = value; } }

        public List<QlDateFilter> DateList { get { return _datelist; } set { _datelist = value; } }

        /// <summary>
        /// Gets or sets a value indicating whether [default deny] is used when Allow and Deny are called.
        /// </summary>
        /// <value><c>true</c> if [default deny]; otherwise, <c>false</c>.</value>
        public bool DefaultDeny { get { return !_defallowed; } set { _defallowed = !value; } }

        /// <summary>
        /// if true, any file that matches ANY date will be allowed. If false, all dates must match
        /// before a tick file is allowed. default is true.
        /// </summary>
        public bool IsDateMatchUnion { get { return _isDateMatchUnion; } set { _isDateMatchUnion = value; } }

        /// <summary>
        /// if true, any file matching SymbolMatch OR DateMatch will be allowed. Otherwise, it must
        /// be allowed by the Symbol filters AND the Date filters. default is true.
        /// </summary>
        public bool IsSymbolDateMatchUnion { get { return _isSymDateMatchUnion; } set { _isSymDateMatchUnion = value; } }

        public List<string> SymbolList { get { return _namelist; } set { _namelist = value; } }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// take a serialized tickfilefilter and convert back to an object
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static TickFileFilter Deserialize(string msg)
        {
            TickFileFilter tf = null;
            try
            {
                // prepare serializer
                XmlSerializer xs = new XmlSerializer(typeof(TickFileFilter));
                // read in message
                var fs = new StringReader(msg);
                // deserialize message
                tf = (TickFileFilter)xs.Deserialize(fs);
                // close serializer
                fs.Close();
            }
            catch (FileNotFoundException) { }
            catch (Exception)
            {
                // ignored
            }
            return tf;
        }

        /// <summary>
        /// save tickfilefilter to a file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static TickFileFilter FromFile(string filename)
        {
            StreamReader sr = new StreamReader(filename);
            string msg = sr.ReadToEnd();
            TickFileFilter tff = Deserialize(msg);
            return tff;
        }

        /// <summary>
        /// serialize a Quantler tick file filter
        /// </summary>
        /// <param name="tff"></param>
        /// <returns></returns>
        public static string Serialize(TickFileFilter tff)
        {
            // save everything as xml
            StringWriter fs;
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(TickFileFilter));
                fs = new StringWriter();
                xs.Serialize(fs, tff);
                fs.Close();
            }
            catch (FileNotFoundException)
            {
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
            if (fs == null) return "";
            return fs.GetStringBuilder().ToString();
        }

        /// <summary>
        /// get a filter that excludes everything but the month from QL date
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        public static int QlMonthMask(int month)
        {
            return month * 100;
        }

        /// <summary>
        /// get a filter that excludes everything but year from QL date
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public static int QlYearMask(int year)
        {
            return year * 10000;
        }

        /// <summary>
        /// restore a filter from a file
        /// </summary>
        /// <param name="tff"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool ToFile(TickFileFilter tff, string filename)
        {
            try
            {
                StreamWriter sw = new StreamWriter(filename, false) { AutoFlush = true };
                sw.WriteLine(Serialize(tff));
                sw.Close();
            }
            catch (Exception) { return false; }
            return true;
        }

        /// <summary>
        /// Allows the specified filepath, if instructed by the filter.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <returns>true if allowed, false otherwise</returns>
        public bool Allow(string filepath)
        {
            TickFileInfo tfi = Util.ParseFile(filepath);
            if (tfi.Type == TickFileType.Invalid) return _allowedinvalid;
            // make sure the default is consistent with the set intersection requested
            // see if symbols match
            bool symallowed = _namelist.Aggregate(_defallowed, (current, t) => current | tfi.Symbol == t);
            // make sure the default is consistent with the set intersection requested
            bool dateallowed = _isDateMatchUnion ? _defallowed : !_defallowed;
            if (_isDateMatchUnion)
            {
                for (int i = 0; i < _datelist.Count; i++)
                    dateallowed |= Util.QlDateMatch(Util.ToQLDate(tfi.Date), _datelist[i].Date, _datelist[i].Type);
            }
            else
            {
                for (int i = 0; i < _datelist.Count; i++)
                    dateallowed &= Util.QlDateMatch(Util.ToQLDate(tfi.Date), _datelist[i].Date, _datelist[i].Type);
            }
            // make sure intersection between dates and symbols is what is desired
            bool allowed = _isSymDateMatchUnion ? symallowed || dateallowed : symallowed && dateallowed;
            return allowed;
        }

        /// <summary>
        /// Allows the specified filepaths. Plural version of Allow.
        /// </summary>
        /// <param name="filepaths">The filepaths.</param>
        /// <returns></returns>
        public string[] Allows(string[] filepaths)
        {
            return filepaths.Where(Allow).ToArray();
        }

        public string[] AllowsIndex(string[,] index)
        {
            List<string> keep = new List<string>();
            for (int i = 0; i < index.GetLength(0); i++)
                if (Allow(index[i, 0]))
                    keep.Add(index[i, 0]);
            return keep.ToArray();
        }

        public string[,] AllowsIndexAndSize(string[,] index)
        {
            List<int> keep = new List<int>();
            for (int i = 0; i < index.GetLength(0); i++)
                if (Allow(index[i, 0]))
                    keep.Add(i);
            string[,] allow = new string[keep.Count, 2];
            for (int i = 0; i < keep.Count; i++)
            {
                allow[i, 0] = index[keep[i], 0];
                allow[i, 1] = index[keep[i], 1];
            }
            return allow;
        }

        /// <summary>
        /// Adds an array of QLDateFilters
        /// </summary>
        /// <param name="filters">The filters.</param>
        public void DateFilter(QlDateFilter[] filters)
        {
            for (int i = 0; i < filters.Length; i++)
                _datelist.Add(filters[i]);
        }

        /// <summary>
        /// Adds a single DateFilter
        /// </summary>
        /// <param name="datefilter">The datefilter.</param>
        public void DateFilter(QlDateFilter datefilter)
        {
            _datelist.Add(datefilter);
        }

        /// <summary>
        /// Adds a single DateFilter
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="type">The type.</param>
        public void DateFilter(int date, DateMatchType type)
        {
            _datelist.Add(new QlDateFilter(date, type));
        }

        /// <summary>
        /// Denies the specified filepath, if instructed by the filter.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <returns>true if denied, false otherwise</returns>
        public bool Deny(string filepath)
        {
            return !Allow(filepath);
        }

        /// <summary>
        /// Adds a symbol filter
        /// </summary>
        /// <param name="stock">The stock.</param>
        public void SymFilter(string stock)
        {
            _namelist.Add(stock);
        }

        /// <summary>
        /// Adds an array of symbol filters
        /// </summary>
        /// <param name="stocks">The stocks.</param>
        public void SymFilter(string[] stocks)
        {
            for (int i = 0; i < stocks.Length; i++)
                _namelist.Add(stocks[i]);
        }

        #endregion Public Methods

        #region Private Methods

        private static List<string> FilterSyms(string[] allowedsymbols)
        {
            List<string> f = new List<string>();
            for (int i = 0; i < allowedsymbols.Length; i++)
                f.Add(allowedsymbols[i]);
            return f;
        }

        #endregion Private Methods

        #region Public Structs

        /// <summary>
        /// match a specific portion of a Quantler date (eg month only, year only, etc)
        /// </summary>
        public struct QlDateFilter
        {
            #region Public Fields

            public int Date;

            public DateMatchType Type;

            #endregion Public Fields

            #region Private Fields

            private const char Dl = '+';

            #endregion Private Fields

            #region Public Constructors

            public QlDateFilter(int date, DateMatchType type)
            {
                Date = date; Type = type;
            }

            #endregion Public Constructors

            #region Public Methods

            public static QlDateFilter Deserialize(string msg)
            {
                string[] r = msg.Split(Dl);
                QlDateFilter df = new QlDateFilter();
                int ir;
                if (int.TryParse(r[1], out ir))
                    df.Type = (DateMatchType)ir;
                if (int.TryParse(r[0], out ir))
                    df.Date = ir;
                return df;
            }

            public static string Serialize(QlDateFilter df)
            {
                return df.Date.ToString() + Dl + (int)df.Type;
            }

            #endregion Public Methods
        }

        #endregion Public Structs
    }
}