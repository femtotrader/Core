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
using Quantler.Data.TikFile;
using Quantler.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestTikWriterReader
    {
        #region Private Fields

        private const int Date = 20090811;

        private const int Tickcount = 100000;
        private readonly string _path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private List<Tick> _readdata = new List<Tick>(Tickcount);

        #endregion Private Fields

        #region Public Methods

        public static bool Removefile(string file)
        {
            if (!System.IO.File.Exists(file)) return true;
            try
            {
                System.IO.File.Delete(file);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Appending()
        {
            // get symbol
            string sym = Research.RandomSymbol.GetSymbol((int)DateTime.Now.Ticks);

            // get some data to test with
            Tick[] data = Research.RandomTicks.GenerateSymbol(sym, Tickcount);
            // apply date and time to ticks
            var start = DateTime.Now;
            for (int i = 0; i < Tickcount; i++)
            {
                TickImpl t = (TickImpl)data[i];
                t.Date = Date;
                t.Time = Util.ToQLTime(start);
                start = start.AddSeconds(1);
            }

            // write and read data
            Writeandread(data, 0, true);

            // verify length
            Assert.Equal(data.Length, _readdata.Count);
            var firsttestlen = data.Length;

            // do it again with later data in same day
            for (int i = 0; i < Tickcount; i++)
            {
                TickImpl t = (TickImpl)data[i];
                t.Date = Date;
                t.Time = Util.ToQLTime(start);
                start = start.AddSeconds(1);
            }

            // write and read data, clocking time
            Writeandread(data, 0, true, false);
            // verify length
            Assert.True(firsttestlen + data.Length == _readdata.Count, "post-append data length not as expected");
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void FilenameTest()
        {
            // get symbol
            string sym = "EUR/USD";
            const int shorttest = 1000;
            // get some data to test with
            Tick[] data = Research.RandomTicks.GenerateSymbol(sym, shorttest);
            // apply date and time to ticks
            for (int i = 0; i < shorttest; i++)
            {
                TickImpl t = (TickImpl)data[i];
                t.Date = Date;
                t.Time = Util.DT2FT(DateTime.Now);
            }

            // write and read data
            Writeandread(data, 0, true);

            // verify length
            Assert.Equal(data.Length, _readdata.Count);
            // verify content
            bool equal = true;
            for (int i = 0; i < shorttest; i++)
                equal &= data[i].Trade == _readdata[i].Trade;
            Assert.True(equal, "read/write mismatch on TIK data.");
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void FilenameTestOption()
        {
            // get symbol
            string sym = "MSTR 201007 CALL 85.0000 OPT";
            const int shorttest = 1000;

            // get some data to test with
            Tick[] data = Research.RandomTicks.GenerateSymbol(sym, shorttest);

            // apply date and time to ticks
            for (int i = 0; i < shorttest; i++)
            {
                TickImpl t = (TickImpl)data[i];
                t.Date = Date;
                t.Time = Util.DT2FT(DateTime.Now);
            }

            // write and read data
            Writeandread(data, 0, true);

            // verify length
            Assert.Equal(data.Length, _readdata.Count);
            // verify content
            bool equal = true;
            for (int i = 0; i < shorttest; i++)
                equal &= data[i].Trade == _readdata[i].Trade;
            Assert.True(equal, "read/write mismatch on TIK data.");
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void TikFromTick()
        {
            // get symbol
            string sym = Research.RandomSymbol.GetSymbol((int)DateTime.Now.Ticks);

            // get some data to test with
            Tick[] data = Research.RandomTicks.GenerateSymbol(sym, 10);

            Writeandread(data, Date, false);

            Assert.Equal(data.Length, _readdata.Count);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void TikFromZip()
        {
            _readdata = new List<Tick>();
            //Get ZIP
            string zipfile = @"Common\AUDJPY.zip";
            string tikfile = "AUDJPY20070511.TIK";

            //Get reader
            TikReader zr = new TikReader(zipfile, tikfile);

            //Get tikcs
            zr.GotTick += tr_gotTick;

            while (zr.NextTick())
            {
            }

            //Check results
            Assert.Equal(4656, _readdata.Count);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void TikTypes()
        {
            // get symbol
            string sym = Research.RandomSymbol.GetSymbol((int)DateTime.Now.Ticks);

            // prepare data
            List<Tick> data = new List<Tick>();

            // bid
            data.Add(TickImpl.NewBid(sym, 10, 100));

            // ask
            data.Add(TickImpl.NewAsk(sym, 11, 200));

            // full quote
            data.Add(TickImpl.NewQuote(sym, Date, 93000, 10, 11, 300, 300, "NYSE", "ARCA"));

            // trade
            data.Add(TickImpl.NewTrade(sym, Date, 93100, 10, 400, "NYSE"));

            // full tick
            Tick full = TickImpl.Copy((TickImpl)data[2], (TickImpl)data[3]);
            data.Add(full);

            // write and read the data
            Writeandread(data.ToArray(), Date, false);

            //verify the count
            Assert.Equal(data.Count, _readdata.Count);

            // verify the data
            bool equal = true;
            StringBuilder sb = new StringBuilder(string.Empty);
            for (int i = 0; i < data.Count; i++)
            {
                bool start = equal;
                equal &= data[i].Bid == _readdata[i].Bid;
                equal &= data[i].BidSize == _readdata[i].BidSize;
                equal &= data[i].BidExchange == _readdata[i].BidExchange;
                equal &= data[i].Ask == _readdata[i].Ask;
                equal &= data[i].AskSize == _readdata[i].AskSize;
                equal &= data[i].AskExchange == _readdata[i].AskExchange;
                equal &= data[i].Trade == _readdata[i].Trade;
                equal &= data[i].Size == _readdata[i].Size;
                equal &= data[i].Exchange == _readdata[i].Exchange;
                equal &= data[i].Depth == _readdata[i].Depth;
                if (equal != start)
                    sb.Append(i + " ");
            }
            Assert.True(equal, "bad ticks: " + sb + data[0] + _readdata[0]);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void WriteandRead()
        {
            // get symbol
            string sym = Research.RandomSymbol.GetSymbol((int)DateTime.Now.Ticks);

            // get some data to test with
            Tick[] data = Research.RandomTicks.GenerateSymbol(sym, Tickcount);
            // apply date and time to ticks
            for (int i = 0; i < Tickcount; i++)
            {
                TickImpl t = (TickImpl)data[i];
                t.Date = Date;
                t.Time = Util.DT2FT(DateTime.Now);
            }

            // write and read data, clocking time
            double elapms = Writeandread(data, 0, true);

            // verify length
            Assert.Equal(data.Length, _readdata.Count);

            // verify content
            bool equal = true;
            for (int i = 0; i < Tickcount; i++)
                equal &= data[i].Trade == _readdata[i].Trade;
            Assert.True(equal, "read/write mismatch on TIK data.");

            // verify performance
            double rate = Tickcount / (elapms / 1000);
            Assert.True(rate >= 90000);
        }

        #endregion Public Methods

        #region Private Methods

        private void tr_gotTick(Tick t)
        {
            _readdata.Add(t);
        }

        private double Writeandread(Tick[] data, int date, bool printperf, bool clearreadbuffer = true)
        {
            // clear out the read buffer
            if (clearreadbuffer)
                _readdata.Clear();
            // keep track of time
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            // write new file from data
            TikWriter tw = new TikWriter(_path, data[0].Symbol, date == 0 ? data[0].Date : date);
            string file = tw.Filepath;
            sw.Start();
            foreach (Tick k in data)
                tw.NewTick(k);
            sw.Stop();
            tw.Close();
            var elapms = (double)sw.ElapsedMilliseconds;
            if (printperf)
                Console.WriteLine("write speed (ticks/sec): " + (data.Length / (elapms / 1000)).ToString("n0"));

            // read file back in from file
            TikReader tr = new TikReader(file);
            tr.GotTick += tr_gotTick;
            sw.Reset();
            sw.Start();
            while (tr.NextTick())
            {
            }
            sw.Stop();
            tr.Close();
            elapms = sw.ElapsedMilliseconds;
            if (printperf)
                Console.WriteLine("read speed (ticks/sec): " + (data.Length / (elapms / 1000)).ToString("n0"));

            // remove file
            Removefile(file);

            return elapms;
        }

        #endregion Private Methods
    }
}