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

using Quantler.Broker;
using Quantler.Data.Ticks;
using Quantler.Interfaces;
using Quantler.Simulator;
using Quantler.Trades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using Quantler.Securities;

namespace Quantler.Backtester
{
    internal class SimpleBacktester
    {
        #region Private Fields

        private bool _orders;

        private bool _trades = true;

        private string currentsample = "IN-Sample";

        private HistSim histsim;

        private double lastp;

        private List<PendingOrder> OrderHistory = new List<PendingOrder>();

        private Queue<PendingOrder> orders = new Queue<PendingOrder>();

        private long PlayTo = MultiSimImpl.Endsim;

        private PortfolioManager portfolio;

        private SimBroker SimBroker;

        private DateTime Started = DateTime.MaxValue;

        private DateTime Stopped = DateTime.MaxValue;

        private int TicksProcessed;

        private Dictionary<Trade, PendingOrder> TradeMutations = new Dictionary<Trade, PendingOrder>();

        #endregion Private Fields

        #region Public Constructors

        public SimpleBacktester(PortfolioManager nsystem, string folder, int cpunr, string[] filter = null)
        {
            try
            {
                //Set start time
                Started = DateTime.Now;
                var transactioncosts = new GenericBrokerModel() { CommPerLot = 6.35M, SlippageInPips = 2.3M, LatencyInMS = 125, SpreadInPips = 2.5M };
                SimBroker = new SimBroker((SimAccount)nsystem.Account, transactioncosts);

                //Initialize
                nsystem.Initialize();
                foreach (var stream in nsystem.Streams)
                {
                    ((OHLCBarStream)stream.Value).Initialize();
                }

                //set response
                portfolio = nsystem;

                //Set progress
                if (OnProgress != null) OnProgress(this, "Starting...", cpunr);

                //Set simulator
                if (filter == null || nsystem.Streams.Count == 1)
                    histsim = new SingleSimImpl(filter);
                else
                    histsim = new MultiSimImpl(filter);

                //Set events
                histsim.GotTick += histsim_GotTick;
                SimBroker.GotFill += SimBroker_GotFill;
                SimBroker.GotOrder += SimBroker_GotOrder;
                SimBroker.GotOrderCancel += SimBroker_GotOrderCancel;
                SimBroker.GotOrderUpdate += SimBroker_GotOrderChanged;
                BindPortfolioEvents();

                //Start simulation
                histsim.PlayTo(SingleSimImpl.Endsim);

                //Notify Complete
                if (OnProgress != null) OnProgress(this, "Finished!", cpunr);
                Stopped = DateTime.Now;

                if (OnMessage != null)
                    OnMessage(this, string.Format("SystemID: {0} - CPU: {1} - Finished in: {2} seconds - Tick p/s: {3} - Trades - {4}",
                        0,
                        cpunr,
                        Math.Round((Stopped - Started).TotalSeconds, 2),
                        Math.Round(TicksSecond),
                        SimBroker.GetTradeList().Count
                        ));

                Thread.Sleep(1000);
                WriteResults();
            }
            catch (Exception exc)
            {
                if (OnProgress != null) OnProgress(this, "FAILED: Exception!", cpunr);
                if (OnMessage != null)
                    OnMessage(this, string.Format("Error (cpu {0}) Message: {1} Stacktrace: {2}", cpunr, exc.Message, exc.StackTrace));
            }
        }

        #endregion Public Constructors

        #region Public Delegates

        public delegate void MessageEvent(SimpleBacktester sender, string message);

        public delegate void OnComplete(int cpunr);

        public delegate void ProgressEvent(SimpleBacktester sender, string progresss, int cpunr);

        #endregion Public Delegates

        #region Public Events

        public static event OnComplete oncomplete;

        public static event MessageEvent OnMessage;

        public static event ProgressEvent OnProgress;

        #endregion Public Events

        #region Public Properties

        public bool Orders { get { return _orders; } set { _orders = value; } }

        public bool Trades { get { return _trades; } set { _trades = value; } }

        #endregion Public Properties

        #region Private Properties

        private double Seconds { get { return Stopped.Subtract(Started).TotalSeconds; } }

        private double TicksSecond { get { return Seconds == 0 ? 0 : (TicksProcessed / Seconds); } }

        #endregion Private Properties

        #region Private Methods

        private void BindPortfolioEvents()
        {
            portfolio.SendOrderEvent += Portfolio_SendOrder;
        }

        private void histsim_GotTick(Tick t)
        {
            TickImpl tick = (TickImpl)t;

            // execute any pending orders
            SimBroker.Execute(tick);

            // send any new orders from our application
            while (orders.Count > 0)
                SimBroker.SendOrder(orders.Dequeue());

            //Set pending orders
            SetPendingOrders();

            //Count ticks
            TicksProcessed++;

            try
            {
                //Check if this symbol is subscribed
                if (portfolio.Streams.ContainsKey(t.Symbol))
                {
                    //Set got tick for portfolio
                    portfolio.GotTick(tick);
                }
            }
            catch (Exception ex)
            {
                if (OnMessage != null)
                    OnMessage(this, "Portfolio threw exception: - " + ex.Message + Environment.NewLine + ex.StackTrace);
            }

            double percent = Math.Round((histsim.FilesProcessed / (double)histsim.FilesPresent) * 100);
            if (percent > lastp)
            {
                //Update percentage
                if (OnProgress != null)
                    OnProgress(this, string.Format("Progress: {0}%, ROI: {1}%, DD: {2}", percent, portfolio.Results.ROI, portfolio.Results.MaxDDPortfolio), 0);
                lastp = percent;
            }
        }

        private void o_OnUpdate(PendingOrder o)
        {
        }

        private void Portfolio_SendOrder(PendingOrder o, int id)
        {
            o.OnUpdate += o_OnUpdate;
            if (histsim != null)
                SimBroker.SendOrderStatus(o);
            OrderHistory.Add(o);
        }

        private void SetPendingOrders()
        {
            //Set pending orders
            portfolio.SetPendingOrders(SimBroker.GetPendingOrderList().ToArray());
        }

        private void SimBroker_GotFill(Trade t, PendingOrder o)
        {
            // save fills so we can generate stats later
            TradeMutations.Add(t, o);
            portfolio.GotFill(t, o);
        }

        private void SimBroker_GotOrder(PendingOrder o)
        {
            //reset pending orders, sync with broker
            SetPendingOrders();

            //Send new order event
            if (portfolio != null)
                portfolio.GotOrder(o);
        }

        private void SimBroker_GotOrderCancel(PendingOrder cancelled)
        {
            if (portfolio != null)
                portfolio.GotOrderCancel(cancelled);
        }

        private void SimBroker_GotOrderChanged(PendingOrder o)
        {
        }

        private void WriteResults()
        {
            Results r = (Results)portfolio.Results;

            Console.WriteLine();
            Console.WriteLine("----- Results -----");
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(r))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(r);
                Console.WriteLine("{0} = {1}", name, value);
            }

            StreamWriter wr = new StreamWriter(Directory.GetCurrentDirectory() + @"\trades.csv", false);

            wr.WriteLine("Date,Time,Symbol,Side,xSize,xPrice,Comment,OpenPL,ClosedPL,OpenSize,ClosedSize,AvgPrice,ID,OrderType,Direction,LimitPrice,StopPrice,Size,Quantity,Status,Cancelled");
            var lines = Util.TradesToClosedPL(TradeMutations.Keys.ToList(), ',');
            var pos = TradeMutations.Values.ToArray();

            for (int i = 0; i < lines.Length; i++)
            {
                var oldpo = pos[i];
                lines[i] += string.Format(",{0},{1},{2},{3},{4},{5},{6},{7},{8}", oldpo.OrderId, oldpo.Order.Type, oldpo.Order.Direction, oldpo.Order.LimitPrice, oldpo.Order.StopPrice, oldpo.Order.Size, oldpo.Order.Quantity, oldpo.OrderStatus, oldpo.IsCancelled);
                wr.WriteLine(lines[i]);
            }
            wr.Flush();
            wr.Close();

            wr = new StreamWriter(Directory.GetCurrentDirectory() + @"\orders.csv", false);

            wr.WriteLine("ID,Date,Time,Symbol,Side,xSize,xPrice,Comment,OrderType,Direction,LimitPrice,StopPrice,Size,Quantity,Status,Cancelled");

            for (int i = 0; i < OrderHistory.Count; i++)
            {
                var oldpo = OrderHistory[i];
                string line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                    oldpo.OrderId, oldpo.Order.Created.Date.ToShortDateString(), oldpo.Order.Created.TimeOfDay.ToString(), oldpo.Order.Security.Name, oldpo.Order.Direction.ToString(), oldpo.Order.Size, oldpo.Order.LimitPrice, oldpo.Order.Comment,
                    oldpo.Order.Type, oldpo.Order.Direction, oldpo.Order.LimitPrice, oldpo.Order.StopPrice, oldpo.Order.Size, oldpo.Order.Quantity, oldpo.OrderStatus, oldpo.IsCancelled);
                wr.WriteLine(line);
            }
            wr.Flush();
            wr.Close();
        }

        #endregion Private Methods
    }
}