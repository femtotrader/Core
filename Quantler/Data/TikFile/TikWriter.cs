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
using System;
using System.IO;

namespace Quantler.Data.TikFile
{
    /// <summary>
    /// write tick files
    /// </summary>
    public class TikWriter : BinaryWriter
    {
        #region Public Fields

        /// <summary>
        /// ticks written
        /// </summary>
        public int Count;

        #endregion Public Fields

        #region Private Fields

        private int _date;
        private string _file = string.Empty;
        private bool _hasheader;
        private string _path = string.Empty;
        private string _realsymbol = string.Empty;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// creates a tikwriter with no header, header is created from first tik
        /// </summary>
        public TikWriter()
        {
        }

        /// <summary>
        /// create a tikwriter for a specific symbol on todays date. auto-creates header
        /// </summary>
        /// <param name="realsymbol"></param>
        public TikWriter(string realsymbol)
            : this(realsymbol, Util.ToQLDate(DateTime.Now))
        {
        }

        /// <summary>
        /// create a tikwriter for specific symbol on specific date auto-creates header
        /// </summary>
        /// <param name="realsymbol"></param>
        /// <param name="date"></param>
        public TikWriter(string realsymbol, int date)
            : this(Environment.CurrentDirectory, realsymbol, date)
        {
        }

        /// <summary>
        /// create tikwriter with specific location, symbol and date. auto-creates header
        /// </summary>
        /// <param name="path"></param>
        /// <param name="realsymbol"></param>
        /// <param name="date"></param>
        public TikWriter(string path, string realsymbol, int date)
        {
            Init(realsymbol, date, path);
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// date represented by data
        /// </summary>
        public int Date { get { return _date; } }

        /// <summary>
        /// path of this file
        /// </summary>
        public string Filepath { get { return _file; } }

        public string FolderPath { get { return _path; } set { _path = value; } }

        /// <summary>
        /// real symbol represented by tick file
        /// </summary>
        public string RealSymbol { get { return _realsymbol; } }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// write header for tick file
        /// </summary>
        /// <param name="bw"></param>
        /// <param name="realsymbol"></param>
        /// <returns></returns>
        public static bool Header(TikWriter bw, string realsymbol)
        {
            bw.OutStream = new FileStream(bw.Filepath, FileMode.Create, FileAccess.Write, FileShare.Read);
            // version
            bw.Write(TikConst.Version);
            bw.Write(TikConst.Filecurrentversion);
            // full symbol name
            bw.Write(realsymbol); //
            // fields end
            bw.Write(TikConst.StartData);
            // flag header as created
            bw._hasheader = true;
            return true;
        }

        /// <summary>
        /// gets symbol safe to use as filename
        /// </summary>
        /// <param name="realsymbol"></param>
        /// <param name="path"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string SafeFilename(string realsymbol, string path, int date)
        {
            return path + "\\" + SafeSymbol(realsymbol) + date + TikConst.DotExt;
        }

        /// <summary>
        /// gets symbol that is safe to use as filename
        /// </summary>
        /// <param name="realsymbol"></param>
        /// <returns></returns>
        public static string SafeSymbol(string realsymbol)
        {
            char[] invalid = Path.GetInvalidPathChars();
            char[] more = "/\\*?:".ToCharArray();
            more.CopyTo(invalid, 0);
            //_more.CopyTo(0,_invalid,_invalid.Length,_more.Length);
            foreach (char c in invalid)
            {
                int p = 0;
                while (p != -1)
                {
                    p = realsymbol.IndexOf(c);
                    if (p != -1)
                        realsymbol = realsymbol.Remove(p, 1);
                }
            }
            return realsymbol;
        }

        /// <summary>
        /// write a tick to file
        /// </summary>
        /// <param name="k"></param>
        public void NewTick(Tick k)
        {
            NewTick((TickImpl)k);
        }

        /// <summary>
        /// write a tick to file
        /// </summary>
        /// <param name="k"></param>
        public void NewTick(TickImpl k)
        {
            // make sure we have a header
            if (!_hasheader) Init(k.Symbol, k.Date, _path);
            // get types
            bool t = k.IsTrade;
            bool fq = k.IsFullQuote;
            bool b = k.HasBid;
            bool a = k.HasAsk;
            bool i = k.IsIndex;

            // next we write tick type and the data
            if (!fq && b) // bid only
            {
                Write(TikConst.TickBid);
                Write(k.Date);
                Write(k.Time);
                Write(k._bid);
                Write(k.BidSize);
                Write(k.BidExchange);
                Write(k.Depth);
            }
            else if (!fq && a) // ask only
            {
                Write(TikConst.TickAsk);
                Write(k.Date);
                Write(k.Time);
                Write(k._ask);
                Write(k.OfferSize);
                Write(k.AskExchange);
                Write(k.Depth);
            }
            else if ((t && !fq) || i) // trade or index
            {
                Write(TikConst.TickTrade);
                Write(k.Date);
                Write(k.Time);
                Write(k._trade);
                Write(k.Size);
                Write(k.Exchange);
            }
            else if (t && fq) // full quote
            {
                Write(TikConst.TickFull);
                Write(k.Date);
                Write(k.Time);
                Write(k._trade);
                Write(k.Size);
                Write(k.Exchange);
                Write(k._bid);
                Write(k.BidSize);
                Write(k.BidExchange);
                Write(k._ask);
                Write(k.OfferSize);
                Write(k.AskExchange);
                Write(k.Depth);
            }
            else if (!t && fq) // quote only
            {
                Write(TikConst.TickQuote);
                Write(k.Date);
                Write(k.Time);
                Write(k._bid);
                Write(k.BidSize);
                Write(k.BidExchange);
                Write(k._ask);
                Write(k.OfferSize);
                Write(k.AskExchange);
                Write(k.Depth);
            }
            // end tick
            Write(TikConst.EndTick);
            // write to disk
            Flush();
            // count it
            Count++;
        }

        #endregion Public Methods

        #region Private Methods

        private void Init(string realsymbol, int date, string path)
        {
            // store important stuff
            _realsymbol = realsymbol;
            _path = path;
            _date = date;
            // get filename from path and symbol
            _file = SafeFilename(_realsymbol, _path, _date);

            // if file exists, assume it has a header
            _hasheader = File.Exists(_file);

            if (!_hasheader)
                Header(this, realsymbol);
            else
            {
                OutStream = new FileStream(_file, FileMode.Open, FileAccess.Write, FileShare.Read);
                OutStream.Position = OutStream.Length;
            }
        }

        #endregion Private Methods
    }
}