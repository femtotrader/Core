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

using NLog;
using Quantler.Data.Bars;
using Quantler.Data.Ticks;
using Quantler.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using Quantler.Securities;

namespace Quantler.Data.TikFile
{
    public static class TikUtil
    {
        #region Private Fields

        private static TikWriter _tw;

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// create ticks from bars on default interval
        /// </summary>
        /// <param name="bl"></param>
        /// <returns></returns>
        public static Tick[] Barlist2Tick(BarList bl)
        {
            List<Tick> k = new List<Tick>(bl.Count * 4);
            foreach (Bar b in bl)
                k.AddRange(BarImpl.ToTick(b));
            return k.ToArray();
        }

        /// <summary>
        /// converts EPF files to tick files in current directory
        /// </summary>
        /// <param name="args"></param>
        public static void Epf2Tik(string[] args)
        {
            // get a list of epf files
            foreach (string file in args)
            {
                SecurityImpl sec = SecurityImpl.FromTik(file);
                sec.HistSource.GotTick += HistSource_gotTick;
                _tw = new TikWriter(sec.Name);
                while (sec.NextTick())
                    _tw.Close();
            }
        }

        public static bool IsFileWritetable(string path)
        {
            FileStream stream = null;

            try
            {
                if (!File.Exists(path))
                    return true;
                FileInfo file = new FileInfo(path);
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return false;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return true;
        }

        /// <summary>
        /// create file from ticks
        /// </summary>
        /// <param name="ticks"></param>
        /// <returns></returns>
        public static bool TicksToFile(Tick[] ticks)
        {
            try
            {
                TikWriter tw = new TikWriter();
                foreach (Tick k in ticks)
                    tw.NewTick(k);
                tw.Close();

                Log.Debug(tw.RealSymbol + " saved " + tw.Count + " ticks to: " + tw.Filepath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating file from ticks");
                return false;
            }
            return true;
        }

        /// <summary>
        /// create file from ticks
        /// </summary>
        /// <param name="ticks"></param>
        /// <param name="tw"></param>
        /// <returns></returns>
        public static bool TicksToFile(Tick[] ticks, TikWriter tw)
        {
            try
            {
                foreach (Tick k in ticks)
                    tw.NewTick(k);
                tw.Close();
                Log.Debug(tw.RealSymbol + " saved " + tw.Count + " ticks to: " + tw.Filepath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating file from ticks");
                return false;
            }
            return true;
        }

        #endregion Public Methods

        #region Private Methods

        private static void HistSource_gotTick(Tick t)
        {
            _tw.NewTick((TickImpl)t);
        }

        #endregion Private Methods
    }
}