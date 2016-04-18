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
using Ionic.Zip;
using Quantler.Securities;

namespace Quantler.Data.TikFile
{
    /// <summary>
    /// read tick files
    /// </summary>
    public sealed class TikReader : BinaryReader
    {
        #region Public Fields

        /// <summary>
        /// estimate of ticks contained in file
        /// </summary>
        public int ApproxTicks;

        /// <summary>
        /// count of ticks presently read
        /// </summary>
        public int Count;

        #endregion Public Fields

        #region Private Fields

        private readonly string _path;
        private int _fileversion;
        private bool _haveheader;
        private string _realsymbol = string.Empty;
        private SecurityImpl _sec = new SecurityImpl();
        private string _sym = string.Empty;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Create a tik reader pointing to a TIK file on disk
        /// </summary>
        /// <param name="filepath"></param>
        public TikReader(string filepath)
            : base(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            _path = filepath;
            FileInfo fi = new FileInfo(filepath);
            ApproxTicks = (int)((double)fi.Length / 39);
            ReadHeader();
        }

        /// <summary>
        /// Create a tik reader pointing to a TIK file in ZIP
        /// </summary>
        /// <param name="zippath"></param>
        /// <param name="filename"></param>
        public TikReader(string zippath, string filename)
            : base(GetStreamFromZip(zippath, filename))
        {
            _path = filename;
            ApproxTicks = (int)((double)BaseStream.Length / 39);
            ReadHeader();
        }

        #endregion Public Constructors

        #region Public Events

        public event TickDelegate GotTick;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// file is readable, has version and real symbol
        /// </summary>
        public bool IsValid { get { return (_fileversion != 0) && (_realsymbol != string.Empty) && BaseStream.CanRead; } }

        /// <summary>
        /// real symbol for data represented in file
        /// </summary>
        public string RealSymbol { get { return _realsymbol; } }

        /// <summary>
        /// security-parsed symbol
        /// </summary>
        public string Symbol { get { return _sym; } }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// returns true if more data to process, false otherwise
        /// </summary>
        /// <returns></returns>
        public bool NextTick()
        {
            if (!_haveheader)
                ReadHeader();

            try
            {
                // get tick type
                byte type = ReadByte();

                // prepare a tick
                TickImpl k = new TickImpl(_realsymbol);

                // get the tick
                switch (type)
                {
                    case TikConst.EndData: return false;
                    case TikConst.EndTick: return true;
                    case TikConst.StartData: return true;
                    case TikConst.Version: return true;
                    case TikConst.TickAsk:
                        {
                            k.Date = ReadInt32();
                            k.Time = ReadInt32();
                            k.Datetime = (long)k.Date * 1000000000 + k.Time;
                            k._ask = ReadUInt64();
                            k.AskSize = ReadInt32();
                            k.AskExchange = ReadString();
                            k.Depth = ReadInt32();
                            break;
                        }
                    case TikConst.TickBid:
                        {
                            k.Date = ReadInt32();
                            k.Time = ReadInt32();
                            k.Datetime = (long)k.Date * 1000000000 + k.Time;
                            k._bid = ReadUInt64();
                            k.BidSize = ReadInt32();
                            k.BidExchange = ReadString();
                            k.Depth = ReadInt32();
                        }
                        break;

                    case TikConst.TickFull:
                        {
                            k.Date = ReadInt32();
                            k.Time = ReadInt32();
                            k.Datetime = (long)k.Date * 1000000000 + k.Time;
                            k._trade = ReadUInt64();
                            k.Size = ReadInt32();
                            k.Exchange = ReadString();
                            k._bid = ReadUInt64();
                            k.BidSize = ReadInt32();
                            k.BidExchange = ReadString();
                            k._ask = ReadUInt64();
                            k.AskSize = ReadInt32();
                            k.AskExchange = ReadString();
                            k.Depth = ReadInt32();
                        }
                        break;

                    case TikConst.TickQuote:
                        {
                            k.Date = ReadInt32();
                            k.Time = ReadInt32();
                            k.Datetime = (long)k.Date * 1000000000 + k.Time;
                            k._bid = ReadUInt64();
                            k.BidSize = ReadInt32();
                            k.BidExchange = ReadString();
                            k._ask = ReadUInt64();
                            k.OfferSize = ReadInt32();
                            k.AskExchange = ReadString();
                            k.Depth = ReadInt32();
                        }
                        break;

                    case TikConst.TickTrade:
                        {
                            k.Date = ReadInt32();
                            k.Time = ReadInt32();
                            k.Datetime = (long)k.Date * 1000000000 + k.Time;
                            k._trade = ReadUInt64();
                            k.Size = ReadInt32();
                            k.Exchange = ReadString();
                        }
                        break;

                    default:
                        // weird data, try to keep reading
                        ReadByte();

                        // but don't send this tick, just get next record
                        return true;
                }
                // send any tick we have
                if (GotTick != null)
                    GotTick(k);

                // read end of tick
                ReadByte();

                // count it
                Count++;
                
                // assume there is more
                return true;
            }
            catch (EndOfStreamException)
            {
                Close();
                return false;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }

        /// <summary>
        /// security represented by parsing realsymbol
        /// </summary>
        /// <returns></returns>
        public ISecurity ToSecurity()
        {
            return _sec;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Get stream from zip (unzips the file into memory)
        /// </summary>
        /// <param name="zippath"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        private static Stream GetStreamFromZip(string zippath, string filename)
        {
            MemoryStream toreturn = new MemoryStream();
            FileInfo fi = new FileInfo(zippath);
            using (ZipFile zip = ZipFile.Read(fi.FullName))
            {
                ZipEntry e = zip[filename];
                e.Extract(toreturn);
            }
            toreturn.Position = 0;
            return toreturn;
        }

        /// <summary>
        /// Read file header for TIK file information
        /// </summary>
        private void ReadHeader()
        {
            // get version id
            ReadByte();

            // get version
            _fileversion = ReadInt32();
            if (_fileversion != TikConst.Filecurrentversion)
                throw new BadTikFile("version: " + _fileversion + " expected: " + TikConst.Filecurrentversion);
            
            // get real symbol
            _realsymbol = ReadString();
            
            // get security from symbol
            _sec = SecurityImpl.Parse(_realsymbol);
            _sec.Date = SecurityImpl.SecurityFromFileName(_path).Date;
            
            // get short symbol
            _sym = _sec.Name;
            
            // get end of header
            ReadByte();
            
            // make sure we read something
            if (_realsymbol.Length <= 0)
                throw new BadTikFile("no symbol defined in tickfile");
            
            // flag header as read
            _haveheader = true;
        }

        #endregion Private Methods
    }
}