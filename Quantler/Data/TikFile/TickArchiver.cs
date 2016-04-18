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
using System.Collections.Generic;
using System.IO;

namespace Quantler.Data.TikFile
{
    /// <summary>
    /// archive ticks as they arrive. Once archived, ticks can be replayed, viewed or analyzed
    /// </summary>
    public class TickArchiver
    {
        #region Private Fields

        private readonly Dictionary<string, int> _datedict = new Dictionary<string, int>();
        private readonly Dictionary<string, TikWriter> _filedict = new Dictionary<string, TikWriter>();
        private readonly string _path;

        private bool _stopped;

        #endregion Private Fields

        #region Public Constructors

        public TickArchiver()
        {
        }

        public TickArchiver(string folderpath)
        {
            _path = folderpath;
        }

        #endregion Public Constructors

        #region Public Methods

        public bool NewTick(Tick t)
        {
            if (_stopped) return false;
            if ((t.Symbol == null) || (t.Symbol == "")) return false;
            TikWriter tw;
            // prepare last date of tick
            int lastdate;
            // get last date
            bool havedate = _datedict.TryGetValue(t.Symbol, out lastdate);
            // if we don't have date, use present date
            if (!havedate)
            {
                lastdate = t.Date;
                _datedict.Add(t.Symbol, t.Date);
            }
            // see if we need a new day
            bool samedate = lastdate == t.Date;
            // see if we have stream already
            bool havestream = _filedict.TryGetValue(t.Symbol, out tw);
            // if no changes, just save tick
            if (samedate && havestream)
            {
                try
                {
                    tw.NewTick((TickImpl)t);
                    return true;
                }
                catch (IOException) { return false; }
            }
            try
            {
                // if new date, close stream
                if (!samedate)
                {
                    try
                    {
                        if (tw != null) tw.Close();
                    }
                    catch (IOException) { }
                }
                // ensure file is writable
                string fn = TikWriter.SafeFilename(t.Symbol, _path, t.Date);
                if (TikUtil.IsFileWritetable(fn))
                {
                    // open new stream
                    tw = new TikWriter(_path, t.Symbol, t.Date);
                    // save tick
                    tw.NewTick((TickImpl)t);
                    // save stream
                    if (!havestream)
                        _filedict.Add(t.Symbol, tw);
                    else
                        _filedict[t.Symbol] = tw;
                    // save date if changed
                    if (!samedate)
                    {
                        _datedict[t.Symbol] = t.Date;
                    }
                }
            }
            catch (IOException) { return false; }
            catch (Exception) { return false; }

            return false;
        }

        #endregion Public Methods

        public void Stop()
        {
            try
            {
                foreach (string file in _filedict.Keys)
                    _filedict[file].Close();
                _stopped = true;
            }
            catch
            {
                // ignored
            }
        }
    }
}