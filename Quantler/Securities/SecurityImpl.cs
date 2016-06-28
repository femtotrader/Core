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

using Quantler.Data.TikFile;
using Quantler.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Quantler.Securities
{
    /// <summary>
    /// used to hold and work with securities.
    /// </summary>
    public class SecurityImpl : ISecurity
    {
        #region Public Fields

        /// <summary>
        /// historical source of tick data for security
        /// </summary>
        public TikReader HistSource;

        #endregion Public Fields

        #region Private Fields

        private string _sym;

        private SecurityType _type;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// create new security
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="exchange"></param>
        /// <param name="type"></param>
        public SecurityImpl(string sym, string exchange, SecurityType type)
        {
            _sym = sym;
            DestEx = exchange;
            _type = type;
        }

        /// <summary>
        /// clone a security
        /// </summary>
        /// <param name="copy"></param>
        public SecurityImpl(ISecurity copy)
        {
            _sym = copy.Name;
            _type = copy.Type;
            DestEx = copy.DestEx;
            Details = copy.Details;
        }

        /// <summary>
        /// create new security
        /// </summary>
        public SecurityImpl()
            : this("", "", SecurityType.NIL)
        {
        }

        /// <summary>
        /// create new security
        /// </summary>
        /// <param name="sym"></param>
        public SecurityImpl(string sym)
            : this(sym, "", SecurityType.Forex)
        {
        }

        /// <summary>
        /// create new security
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="type"></param>
        public SecurityImpl(string sym, SecurityType type)
            : this(sym, "", type)
        {
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// approximate # of ticks contained in historical security
        /// </summary>
        public int ApproxTicks { get; set; }

        public decimal Ask
        {
            get;
            set;
        }

        public decimal Bid
        {
            get;
            set;
        }

        /// <summary>
        /// Name of the broker for this security
        /// </summary>
        public string BrokerName { get; set; }

        /// <summary>
        /// date associated with security
        /// </summary>
        public int Date { get; set; }

        /// <summary>
        /// exchange associated with security
        /// </summary>
        public string DestEx { get; set; }

        /// <summary>
        /// details of security
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Amount of digits for this security
        /// </summary>
        public int Digits
        {
            get;
            set;
        }

        /// <summary>
        /// whether security has a exchange
        /// </summary>
        public bool HasDest { get { return DestEx != ""; } }

        /// <summary>
        /// Says whether stock contains historical data.
        /// </summary>
        public bool HasHistorical { get; private set; }

        /// <summary>
        /// whether security has a defined security type
        /// </summary>
        public bool HasType { get { return _type != SecurityType.NIL; } }

        /// <summary>
        /// Check whether this is a fixed spread security or a floating (variable) spread
        /// </summary>
        public bool IsFloatingSpread
        {
            get;
            set;
        }

        /// <summary>
        /// whether security is valid
        /// </summary>
        public virtual bool IsValid { get { return _sym != ""; } }

        public DateTime LastTickEvent
        {
            get;
            set;
        }

        public int LotSize
        {
            get;
            set;
        }

        /// <summary>
        /// symbol associated with security
        /// </summary>
        public string Name { get { return _sym; } set { _sym = value; } }

        public int OrderMaxSize
        {
            get;
            set;
        }

        public decimal OrderMinQuantity
        {
            get { return OrderMinSize / (decimal)LotSize; }
        }

        public decimal OrderMaxQuantity
        {
            get { return OrderMaxSize / (decimal)LotSize; }
        }

        public int OrderMinSize
        {
            get;
            set;
        }

        public decimal OrderStepQuantity
        {
            get { return OrderStepSize / (decimal)LotSize; }
        }

        public int OrderStepSize
        {
            get;
            set;
        }

        /// <summary>
        /// Get the minimal change in market price, represented in one tick
        /// </summary>
        public decimal PipSize
        {
            get;
            set;
        }

        /// <summary>
        /// Current value of one pip, denominated in the accounts currency type
        /// </summary>
        public decimal PipValue
        {
            get;
            set;
        }

        /// <summary>
        /// Current spread in pips
        /// </summary>
        public int Spread
        {
            get;
            set;
        }

        public decimal TickSize
        {
            get;
            set;
        }

        /// <summary>
        /// type of security
        /// </summary>
        public SecurityType Type { get { return _type; } set { _type = value; } }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// deserialize a security from a string
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static ISecurity Deserialize(string msg)
        {
            return Parse(msg);
        }

        /// <summary>
        /// load a security from a historical tick file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static SecurityImpl FromTik(string filename)
        {
            TikReader tr = new TikReader(filename);
            SecurityImpl s = (SecurityImpl)tr.ToSecurity();
            if (s.IsValid && tr.IsValid)
            {
                s.HasHistorical = true;
                s.HistSource = tr;
                s.ApproxTicks = s.HistSource.ApproxTicks;
            }
            return s;
        }

        /// <summary>
        /// Load a security from a zipfile containing a historical tick file
        /// </summary>
        /// <param name="zipfile"></param>
        /// <param name="tikfile"></param>
        /// <returns></returns>
        public static SecurityImpl FromZip(string zipfile, string tikfile)
        {
            TikReader tr = new TikReader(zipfile, tikfile);
            SecurityImpl s = (SecurityImpl)tr.ToSecurity();
            if (s.IsValid && tr.IsValid)
            {
                s.HasHistorical = true;
                s.HistSource = tr;
                s.ApproxTicks = s.HistSource.ApproxTicks;
            }
            return s;
        }

        /// <summary>
        /// get a security from a user-specified string
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static SecurityImpl Parse(string msg)
        {
            return Parse(msg, 0);
        }

        /// <summary>
        /// get a security form a user-specified string
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static SecurityImpl Parse(string msg, int date)
        {
            string[] r = msg.Split(' ');
            SecurityImpl sec = new SecurityImpl { Name = r[0] };
            // look for option first
            if (msg.Contains("OPT") || msg.Contains("PUT") || msg.Contains("CALL") || msg.Contains("FOP") || msg.Contains("FUT"))
            {
                if (msg.Contains("OPT") || msg.Contains("PUT") || msg.Contains("CALL"))
                    sec.Type = SecurityType.Option;

                if (msg.Contains("FUT"))
                    sec.Type = SecurityType.Future;

                msg = msg.ToUpper();
                sec.Details = msg.Contains("PUT") ? "PUT" : (msg.Contains("CALL") ? "CALL" : string.Empty);
                msg = msg.Replace("CALL", "");
                msg = msg.Replace("PUT", "");
                msg = msg.Replace("OPT", "");
                msg = msg.Replace("FOP", "");
                msg = msg.Replace("FUT", "");
                r = msg.Split(' ');
                sec.Name = r[0];
                sec.Date = ExpirationDate(ref r);
                sec.DestEx = Ex(sec.Name, ref r);
            }
            else if (r.Length > 2)
            {
                int f2Id = SecurityId(r[2]);
                int f1Id = SecurityId(r[1]);
                if (f2Id != -1)
                {
                    sec.Type = (SecurityType)f2Id;
                    sec.DestEx = r[1];
                }
                else if (f1Id != -1)
                {
                    sec.Type = (SecurityType)f1Id;
                    sec.DestEx = r[2];
                }
            }
            else if (r.Length > 1)
            {
                int f1Id = SecurityId(r[1]);
                if (f1Id != -1)
                    sec.Type = (SecurityType)f1Id;
                else sec.DestEx = r[1];
            }
            else
                sec.Type = SecurityType.Equity;
            if (date != 0)
                sec.Date = date;
            if (sec.HasDest && !sec.HasType)
                sec.Type = TypeFromExchange(sec.DestEx);
            return sec;
        }

        /// <summary>
        /// determine security from the filename, without opening file (use SecurityImpl.FromFile to
        /// actually read it in)
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static SecurityImpl SecurityFromFileName(string filename)
        {
            try
            {
                filename = Path.GetFileName(filename);
                string ds = Regex.Match(filename, "([0-9]{8})[.]", RegexOptions.IgnoreCase).Result("$1");
                string sym = filename.Replace(ds, "").Replace(TikConst.DotExt, "");
                SecurityImpl s = new SecurityImpl(sym) { Date = Convert.ToInt32(ds) };
                return s;
            }
            catch (Exception) { }
            return new SecurityImpl();
        }

        /// <summary>
        /// serialize security as a string
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        public static string Serialize(ISecurity sec)
        {
            List<string> p = new List<string> { sec.Name };
            if (sec.HasDest)
                p.Add(sec.DestEx);
            if ((sec.Type != SecurityType.NIL) && (sec.Type != SecurityType.Equity))
                p.Add(sec.Type.ToString());
            return string.Join(" ", p.ToArray());
        }

        /// <summary>
        /// test whether two securities are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return ToString() == obj.ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode() + Date;
        }

        /// <summary>
        /// Fetches next historical tick for stock, or invalid tick if no historical data is available.
        /// </summary>
        public bool NextTick()
        {
            if (HistSource == null) return false;
            bool v = true;
            try
            {
                v = HistSource.NextTick();
            }
            catch (EndOfStreamException)
            {
                HistSource.Close();
            }
            catch (IOException) { }
            return v;
        }

        /// <summary>
        /// printable string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Serialize(this);
        }

        #endregion Public Methods

        #region Private Methods

        private static string Ex(string sym, ref string[] tests)
        {
            for (int i = 0; i < tests.Length; i++)
                if ((tests[i] != sym) && (tests[i] != string.Empty))
                    return tests[i];
            return string.Empty;
        }

        private static int ExpirationDate(ref string[] tests)
        {
            for (int i = 0; i < tests.Length; i++)
            {
                string test = tests[i];
                int date;
                if (int.TryParse(test, out date))
                {
                    tests[i] = string.Empty;
                    return date;
                }
            }
            return 0;
        }

        private static int SecurityId(string type)
        {
            int id = -1;
            SecurityType st;
            if (Enum.TryParse(type, out st))
                id = (int)st;
            return id;
        }

        private static SecurityType TypeFromExchange(string ex)
        {
            if ((ex == "GLOBEX") || (ex == "NYMEX") || (ex == "CFE"))
                return SecurityType.Future;
            if ((ex == "NYSE") || (ex == "NASDAQ") || (ex == "ARCA"))
                return SecurityType.Equity;
            // default to STK if not sure
            return 0;
        }

        #endregion Private Methods
    }
}