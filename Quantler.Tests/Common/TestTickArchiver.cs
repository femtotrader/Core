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
using Quantler.Tests.Common.Research;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestTickArchiver
    {
        #region Private Fields

        private const int Date = 20090815;
        private const int Maxticks = 1000;
        private const string Sym = "TST";
        private readonly string _path = Environment.CurrentDirectory;
        private readonly List<Tick> _readdata = new List<Tick>();

        private readonly List<Tick> _readdata2 = new List<Tick>();

        private string _file;

        #endregion Private Fields

        #region Public Methods

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void CreateRead()
        {
            _readdata.Clear();
            _readdata2.Clear();
            _file = TikWriter.SafeFilename(Sym, _path, Date);
            TestTikWriterReader.Removefile(_file);
            {
                TickImpl[] data = RandomTicks.GenerateSymbol(Sym, Maxticks).Select(x => (TickImpl)x).ToArray();

                TickArchiver ta = new TickArchiver(Environment.CurrentDirectory);
                for (int i = 0; i < data.Length; i++)
                {
                    data[i].Date = Date;
                    data[i].Time = Util.DT2FT(DateTime.Now);
                    ta.NewTick(data[i]);
                }
                ta.Stop();

                // read file back in from file
                TikReader tr = new TikReader(_file);
                tr.GotTick += tr_gotTick;
                while (tr.NextTick())
                {
                }

                // verify length
                Assert.Equal(data.Length, _readdata.Count);
                // verify content
                bool equal = true;
                for (int i = 0; i < Maxticks; i++)
                    equal &= data[i].Trade == _readdata[i].Trade;
                tr.Close();

                _readdata.Clear();
                Assert.True(equal, "ticks did not matched archive.");
                TestTikWriterReader.Removefile(_file);
            }
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Multiday()
        {
            _readdata.Clear();
            _readdata2.Clear();
            int d = 20100223;
            int t = 235900;
            int t1 = 0;
            const decimal p = 50;
            int s = 100;

            string file1 = TikWriter.SafeFilename(Sym, _path, d);
            TestTikWriterReader.Removefile(file1);
            string file2 = TikWriter.SafeFilename(Sym, _path, d + 1);
            TestTikWriterReader.Removefile(file2);

            Tick[] data = {
                TickImpl.NewTrade(Sym,d,t++,p,s,string.Empty),
                TickImpl.NewTrade(Sym,d,t++,p,s,string.Empty),
                TickImpl.NewTrade(Sym,d,t++,p,s,string.Empty),
                TickImpl.NewTrade(Sym,d,t++,p,s,string.Empty),
                TickImpl.NewTrade(Sym,d,t++,p,s,string.Empty),
                // day two
                TickImpl.NewTrade(Sym,++d,t1++,p,s,string.Empty),
                TickImpl.NewTrade(Sym,d,t1++,p,s,string.Empty),
                TickImpl.NewTrade(Sym,d,t1++,p,s,string.Empty),
                TickImpl.NewTrade(Sym,d,t1++,p,s,string.Empty),
                TickImpl.NewTrade(Sym,d,t1++,p,s,string.Empty),
            };

            TickArchiver ta = new TickArchiver(Environment.CurrentDirectory);
            for (int i = 0; i < data.Length; i++)
            {
                ta.NewTick(data[i]);
            }
            ta.Stop();

            // read file back in from files
            if (System.IO.File.Exists(file1))
            {
                TikReader tr = new TikReader(file1);
                tr.GotTick += tr_gotTick;
                while (tr.NextTick()) ;
                tr.Close();
            }

            if (System.IO.File.Exists(file2))
            {
                TikReader tr2 = new TikReader(file2);
                tr2.GotTick += tr2_gotTick;
                while (tr2.NextTick()) ;
                tr2.Close();
            }

            // verify length
            Assert.Equal(5, _readdata2.Count);
            Assert.Equal(5, _readdata.Count);

            TestTikWriterReader.Removefile(file1);
            TestTikWriterReader.Removefile(file2);
        }

        #endregion Public Methods

        #region Private Methods

        private void tr_gotTick(Tick t)
        {
            _readdata.Add(t);
        }

        private void tr2_gotTick(Tick t)
        {
            _readdata2.Add(t);
        }

        #endregion Private Methods
    }
}