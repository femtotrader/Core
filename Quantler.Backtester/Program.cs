#region License
/*
Copyright (c) Quantler B.V., All rights reserved.

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

using Ionic.Zip;
using NLog;
using Quantler.Agent;
using Quantler.Broker;
using Quantler.Data.TikFile;
using Quantler.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using Quantler.Securities;

namespace Quantler.Backtester
{
    internal class Program
    {
        #region Private Fields

        private static ILogger Log = LogManager.GetCurrentClassLogger();
        private static int startDTfilter, endDTfilter;
        private static int TimeFrameInSeconds = 3600;

        #endregion Private Fields

        #region Private Methods

        //default 5-minute
        private static SimpleBacktester Execute(PortfolioManager t, string tick, int cpu)
        {
            return new SimpleBacktester(t, tick, cpu);
        }

        private static SimpleBacktester Execute(PortfolioManager t, string tick, int cpu, string[] filter)
        {
            return new SimpleBacktester(t, tick, cpu, filter);
        }

        private static PortfolioManager Get(string symbol)
        {
            //Initialize
            TradingPortfolio np = new TradingPortfolio();

            //ADD INITIAL SECURITIES
            ForexSecurity security = new ForexSecurity(symbol);
            security.LotSize = 100000;      //Lots
            security.PipSize = 0.0001M;     //PipSize in 4 decimal places
            security.TickSize = 0.00001M;   //Size of one tick
            security.PipValue = 1M;         //PipValue, if unable to calculate to base currency
            security.Digits = 5;

            var account = new SimAccount("SIMULATED", "Sim account for backtesting", 10000, 100, "SIM");
            np.SetAccount(account);
            np.Securities.AddSecurity(security);

            //ADD AGENT AND INJECT TEMPLATES
            np.InjectAgent<ExampleTradingAgent>();

            //Set streams
            TradingAgent agent = (TradingAgent)np.Agents[0];
            agent.AgentId = 115230;
            agent.TimeFrame = TimeSpan.FromSeconds(TimeFrameInSeconds);
            OHLCBarStream ls = new OHLCBarStream(np.Securities[symbol], np.Agents[0].TimeFrame);
            agent.SetDefaultStream(ls);
            ls.Initialize();
            agent.Initialize();
            agent.Start();

            SimpleBacktester.OnMessage += SimpleBacktester_OnMessage;
            SimpleBacktester.OnProgress += SimpleBacktester_OnProgress;

            return np;
        }

        /// <summary>
        /// Returns the TIK file filter based on the samples used in this backtest
        /// </summary>
        /// <param name="baseFolder"></param>
        /// <param name="symbol"></param>
        /// <param name="filter"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <returns></returns>
        private static string[] GetTIKFiles(string baseFolder, string[] symbols, out TickFileFilter filter, int startdate, int enddate)
        {
            List<string> files = new List<string>();

            //Create filter with the symbols as requested
            filter = new TickFileFilter(symbols);

            //Add the initial timeperiod
            DateTime ct = Util.Qld2Dt(startdate);
            TimeSpan periodlenght = Util.Qld2Dt(enddate) - Util.Qld2Dt(startdate);

            //Filter the period
            for (int i = 0; i < periodlenght.TotalDays; i++)
                filter.DateFilter(int.Parse(ct.AddDays(i).ToString("yyyyMMdd")), DateMatchType.Day);

            //settings
            filter.IsDateMatchUnion = true;
            filter.DefaultDeny = true;

            //Select tickfiles
            foreach (var xdate in filter.DateList)
                foreach (var symbol in filter.SymbolList)
                {
                    //Check if file exists in the zip file
                    FileInfo fi = new FileInfo(baseFolder + string.Format("\\{0}.zip", symbol));

                    if (!fi.Exists)
                        throw new FileNotFoundException("Could not find tik archive");

                    using (ZipFile zip = ZipFile.Read(fi.FullName))
                    {
                        if (!zip.ContainsEntry(symbol.ToUpper() + xdate.Date + ".TIK"))
                            continue;
                    }
                    string file = fi.FullName + @"\" + symbol.ToUpper() + xdate.Date + ".TIK";
                    files.Add(file);
                }
            return files.ToArray();
        }

        private static void Main(string[] args)
        {
            try
            {
                string tickfolder = string.Empty;
                string[] symbols;
                string[] files;
                TickFileFilter filter;

                if (args.Length > 0)
                {
                    //Get tick folder
                    tickfolder = args[0];

                    Console.WriteLine("Tickfolder: " + tickfolder);

                    //Get symbol
                    symbols = new[] { args[1] };

                    Console.WriteLine("Symbol: " + string.Join(", ", symbols));

                    //Get Timeframe
                    int tf;
                    if (int.TryParse(args[2], out tf))
                        TimeFrameInSeconds = tf;

                    Console.WriteLine("Timeframe in seconds: " + TimeFrameInSeconds);

                    //get symbol filter
                    if (args.Length > 3)
                    {
                        int start, end;
                        if (int.TryParse(args[3], out start))
                            startDTfilter = start;
                        if (int.TryParse(args[4], out end))
                            endDTfilter = end;

                        Console.WriteLine("Start: " + start);
                        Console.WriteLine("End: " + end);
                    }
                }
                else
                {
                    tickfolder = Util.AssemblyDirectory + @"\Data";
                    symbols = new[] { "EURUSD" };
                    TimeFrameInSeconds = 3600;
                }

                if (tickfolder == string.Empty || symbols.Length == 0)
                    return;

                //Create backtester
                if (startDTfilter == 0 | endDTfilter == 0)
                {
                    startDTfilter = 20150101;
                    endDTfilter = 20151231;
                }

                files = GetTIKFiles(tickfolder, symbols, out filter, startDTfilter, endDTfilter);
                Execute(Get(symbols[0]), tickfolder, 0, files);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                Console.WriteLine(exc.StackTrace);
                Console.Read();
            }

            Console.Read();
        }

        private static void SimpleBacktester_OnMessage(SimpleBacktester sender, string message)
        {
            Log.Debug("<Message> {0} = {1}", DateTime.Now.ToShortTimeString(), message);
        }

        private static void SimpleBacktester_OnProgress(SimpleBacktester sender, string progress, int cpunr)
        {
            Log.Debug("<Message> {0} = Current Progress = {1}%", DateTime.Now.ToShortTimeString(), progress);
        }

        #endregion Private Methods
    }
}