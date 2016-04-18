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
using Quantler.Orders;
using Quantler.Trades;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantler.Broker
{
    /// <summary>
    /// A simulated broker class for Quantler. Processes orders and fills them against external
    /// tick feed. (live or historical)
    /// </summary>
    public class SimBroker
    {
        #region Public Fields

        public const string Defaultbook = "DEFAULT";

        /// <summary>
        /// set to the exchange where opening prints are obtained from
        /// </summary>
        public string Opgex = "NYS";

        #endregion Public Fields

        #region Protected Fields

        protected Dictionary<string, SimAccount> Acctlist = new Dictionary<string, SimAccount>();

        protected SimAccount Default = new SimAccount(Defaultbook, "Defacto account when account not provided");

        protected Dictionary<IAccount, List<PendingOrder>> MasterOrders = new Dictionary<IAccount, List<PendingOrder>>();

        protected Dictionary<string, List<Trade>> MasterTrades = new Dictionary<string, List<Trade>>();

        #endregion Protected Fields

        #region Private Fields

        private readonly List<string> _hasopened = new List<string>();
        private FillMode _fm = FillMode.OwnBook;

        private long _nextorderid = OrderImpl.Unique;

        private int _pendorders;

        #endregion Private Fields

        #region Public Constructors

        public SimBroker()
        {
            Reset();
        }

        public SimBroker(SimAccount account, BrokerModel tcmodel)
        {
            Default = account;
            BrokerModel = tcmodel;
            Reset();
        }

        #endregion Public Constructors

        #region Public Events

        /// <summary>
        /// Occurs when [got fill].
        /// </summary>
        public event FillDelegate GotFill;

        /// <summary>
        /// Occurs when [got order].
        /// </summary>
        public event OrderDelegate GotOrder;

        /// <summary>
        /// Occurs when [got order cancel].
        /// </summary>
        public event OrderCancelDelegate GotOrderCancel;

        /// <summary>
        /// Occurs when an order has changed its state at the broker
        /// </summary>
        public event OrderUpdateDelegate GotOrderUpdate;

        #endregion Public Events

        #region Public Properties

        public string[] Accounts
        {
            get
            {
                SimAccount[] accts = new SimAccount[MasterOrders.Count];
                MasterOrders.Keys.CopyTo(accts, 0);
                return accts.Select(t => t.Id).ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the transactioncosts calculation model used for this broker
        /// </summary>
        public BrokerModel BrokerModel { get; set; }

        /// <summary>
        /// Gets or sets the fill mode this broker uses when executing orders
        /// </summary>
        public FillMode FillMode { get { return _fm; } set { _fm = value; } }

        /// <summary>
        /// whether bid/ask is used to fill orders. if false, last trade is used.
        /// </summary>
        public bool UseBidAskFills { get; set; }

        /// <summary>
        /// whether or not to assume high liquidity fills on sparse data. (should only be used on
        /// daily/EOD data)
        /// </summary>
        public bool UseHighLiquidityFillsEod { get; set; }

        #endregion Public Properties

        #region Protected Properties

        protected List<Trade> FillList { get { return MasterTrades[Default.Id]; } set { MasterTrades[Default.Id] = value; } }

        protected List<PendingOrder> Orders { get { return MasterOrders[Default]; } set { MasterOrders[Default] = value; } }

        #endregion Protected Properties

        #region Public Methods

        public Order BestBid(string symbol, SimAccount account)
        {
            return BestBidOrOffer(symbol, Direction.Long, account);
        }

        public Order BestBid(string symbol)
        {
            return BestBidOrOffer(symbol, Direction.Long);
        }

        public Order BestBidOrOffer(string symbol, Direction direction)
        {
            Order best = new OrderImpl();
            SimAccount[] accts = new SimAccount[MasterOrders.Count];
            MasterOrders.Keys.CopyTo(accts, 0);
            for (int i = 0; i < accts.Length; i++)
            {
                SimAccount a = accts[i];
                // get our first order
                if (!best.IsValid)
                {
                    // if we don't have a valid one yet, check this account
                    best = new OrderImpl(BestBidOrOffer(symbol, direction, a));
                    continue;  // keep checking the accounts till we find a valid one
                }
                // now we have our first order, which will be best if we can't find a second one
                Order next = new OrderImpl(BestBidOrOffer(symbol, direction, a));
                if (!next.IsValid) continue; // keep going till we have a second order
                best = BestBidOrOffer(best, next); // when we have two, compare and get best
                // then keep fetching next valid order to see if it's better
            }
            return best; // if there's no more orders left, this is best
        }

        public Order BestBidOrOffer(string sym, Direction direction, SimAccount account)
        {
            Order best = new OrderImpl();
            if (!MasterOrders.ContainsKey(account)) return best;
            List<PendingOrder> orders = MasterOrders[account];
            for (int i = 0; i < orders.Count; i++)
            {
                Order o = orders[i].Order;
                if (o.Symbol != sym) continue;
                if (o.Direction != direction) continue;
                if (!best.IsValid)
                {
                    best = new OrderImpl(o);
                    continue;
                }
                Order test = BestBidOrOffer(best, o);
                if (test.IsValid) best = new OrderImpl(test);
            }
            return best;
        }

        // takes two orders and returns the better one if orders aren't for same side or symbol or
        // not limit, returns invalid order if orders are equally good, adds them together
        public Order BestBidOrOffer(Order first, Order second)
        {
            if ((first.Symbol != second.Symbol) || (first.Direction != second.Direction) || first.Type != OrderType.Limit || second.Type != OrderType.Limit)
                return new OrderImpl(); // if not comparable return an invalid order
            if ((first.Direction == Direction.Long && (first.LimitPrice > second.LimitPrice)) || // if first is better, use it
                (first.Direction == Direction.Short && (first.LimitPrice < second.LimitPrice)))
                return new OrderImpl(first);
            if ((first.Direction == Direction.Long && (first.LimitPrice < second.LimitPrice)) || // if second is better, use it
                (first.Direction == Direction.Short && (first.LimitPrice > second.LimitPrice)))
                return new OrderImpl(second);

            // if order is matching then add the sizes
            OrderImpl add = new OrderImpl(first);
            add.Size = add.UnsignedSize + second.UnsignedSize * (add.Direction == Direction.Long ? 1 : -1);
            return add;
        }

        public Order BestOffer(string symbol, SimAccount account)
        {
            return BestBidOrOffer(symbol, Direction.Short, account);
        }

        public Order BestOffer(string symbol)
        {
            return BestBidOrOffer(symbol, Direction.Short);
        }

        /// <summary>
        /// Executes any open orders allowed by the specified tick.
        /// </summary>
        /// <param name="tick">The tick.</param>
        /// <returns>the number of orders executed using the tick.</returns>
        public int Execute(TickImpl tick)
        {
            //TODO: in order to calculate the floating PnL ticks are send to the account for calculations.
            //Also this location is incorrect!
            Default.OnTick(tick);
            if (_pendorders == 0) return 0;

            //Check if we need to process the order as a trade or use the bid ask fills
            UseBidAskFills = tick.HasAsk && tick.HasBid;

            int filledorders = 0;
            SimAccount[] accts = new SimAccount[MasterOrders.Count];
            MasterOrders.Keys.CopyTo(accts, 0);

            for (int idx = 0; idx < accts.Length; idx++)
            {
                // go through each account
                SimAccount a = accts[idx];

                // if account has requested no executions, skip it
                if (!a.Execute) continue;
                //Check for a margin call by the broker
                if (a.MarginLevel <= BrokerModel.StopOutLevel())
                {
                    //cancel this order
                    MasterOrders[a].ForEach(x =>
                    {
                        ((PendingOrderImpl)x).OrderStatus = StatusType.INSUFFICIENT_CAPITAL;
                        x.Cancel();
                    });
                }

                // make sure we have a record for this account
                if (!MasterTrades.ContainsKey(a.Id))
                    MasterTrades.Add(a.Id, new List<Trade>());

                // track orders being removed and trades that need notification
                List<int> notifytrade = new List<int>();
                List<int> remove = new List<int>();

                // go through each order in the account
                for (int i = 0; i < MasterOrders[a].Count; i++)
                {
                    //Get the order object
                    PendingOrderImpl po = (PendingOrderImpl)MasterOrders[a][i];
                    OrderImpl o = (OrderImpl)po.Order;

                    //make sure tick is for the right stock, and right exchange
                    if (tick.Symbol != o.Symbol)
                        continue;
                    //Check if order is in a correct state
                    if (po.OrderStatus != StatusType.OK)
                    {
                        remove.Add(i);
                        // count the trade
                        filledorders++;
                        continue;
                    }

                    if (UseHighLiquidityFillsEod)
                    {
                        po.OrderStatus = o.Fill(tick, BrokerModel, UseBidAskFills, false, true);
                    }
                    else if (o.ValidInstruct <= OrderInstructionType.GTC)
                    {
                        po.OrderStatus = o.Fill(tick, BrokerModel, UseBidAskFills, false); // fill our trade
                    }
                    else if (o.ValidInstruct == OrderInstructionType.OPG)
                    {
                        // if it's already opened, we missed our shot
                        if (_hasopened.Contains(o.Symbol))
                            continue;
                        // otherwise make sure it's really the opening
                        if (tick.Exchange == Opgex)
                        {
                            // it's the opening tick, so fill it as an opg
                            po.OrderStatus = o.Fill(tick, BrokerModel, UseBidAskFills, true);
                            // mark this symbol as already being open
                            _hasopened.Add(tick.Symbol);
                        }
                    }
                    // other orders fill normally, except MOC orders which are at 4:00PM
                    else if (o.ValidInstruct == OrderInstructionType.MOC)
                    {
                        if (tick.Time >= 160000000)
                            po.OrderStatus = o.Fill(tick, BrokerModel, UseBidAskFills, false); // fill our trade
                    }
                    else
                        po.OrderStatus = o.Fill(tick, BrokerModel, UseBidAskFills, false); // fill our trade

                    if (po.OrderStatus == StatusType.ORDER_FILLED)
                    {
                        // remove filled size from size available in trade
                        tick.Size -= o.UnsignedSize;

                        // get copy of trade for recording
                        TradeImpl trade = new TradeImpl(o);

                        // if trade represents entire requested order, mark order for removal
                        if (trade.UnsignedSize == o.UnsignedSize)
                            remove.Add(i);
                        else // otherwise reflect order's remaining size
                            o.Size = (o.UnsignedSize - trade.UnsignedSize) * (o.Direction == Direction.Long ? 1 : -1);

                        // record trade
                        MasterTrades[a.Id].Add(trade);

                        // mark it for notification
                        notifytrade.Add(MasterTrades[a.Id].Count - 1);

                        // count the trade
                        filledorders++;
                    }
                }

                // notify subscribers of trade
                if ((GotFill != null) && a.Notify)
                    for (int tradeidx = 0; tradeidx < notifytrade.Count; tradeidx++)
                    {
                        TradeImpl t = (TradeImpl)MasterTrades[a.Id][notifytrade[tradeidx]];
                        var account = Acctlist[Accounts[0]];
                        t.Account = account;
                        t.AccountName = t.Account.Id;
                        account.OnFill(t);
                        GotFill(t, MasterOrders[a][remove[tradeidx]]);
                    }

                int rmcount = remove.Count;

                // remove the filled orders
                for (int i = remove.Count - 1; i >= 0; i--)
                    MasterOrders[a].RemoveAt(remove[i]);

                // unmark filled orders as pending
                _pendorders -= rmcount;
                if (_pendorders < 0) _pendorders = 0;
            }
            return filledorders;
        }

        /// <summary>
        /// Gets the open positions for the default account.
        /// </summary>
        /// <param name="symbol">The symbol to get a position for.</param>
        /// <returns>current position</returns>
        public Position GetOpenPosition(ISecurity symbol)
        {
            return GetOpenPosition(symbol, Default);
        }

        /// <summary>
        /// Gets the open position for the specified account.
        /// </summary>
        /// <param name="symbol">The symbol to get a position for.</param>
        /// <param name="a">the account.</param>
        /// <returns>current position</returns>
        public Position GetOpenPosition(ISecurity symbol, SimAccount a)
        {
            PositionImpl pos = new PositionImpl(symbol);
            if (!MasterTrades.ContainsKey(a.Id)) return pos;
            List<Trade> trades = MasterTrades[a.Id];
            for (int i = 0; i < trades.Count; i++)
                if (trades[i].Symbol == symbol.Name)
                    pos.Adjust(trades[i]);
            return pos;
        }

        /// <summary>
        /// Gets the list of open orders for this account.
        /// </summary>
        /// <param name="a">Account.</param>
        /// <returns></returns>
        public List<PendingOrder> GetPendingOrderList(SimAccount a)
        {
            List<PendingOrder> res;
            bool worked = MasterOrders.TryGetValue(a, out res);
            return worked ? res : new List<PendingOrder>();
        }

        public List<PendingOrder> GetPendingOrderList()
        {
            return GetPendingOrderList(Default);
        }

        /// <summary>
        /// Gets the complete execution list for this account
        /// </summary>
        /// <param name="a">account to request blotter from.</param>
        /// <returns></returns>
        public List<Trade> GetTradeList(SimAccount a)
        {
            List<Trade> res; bool worked = MasterTrades.TryGetValue(a.Id, out res); return worked ? res : new List<Trade>();
        }

        public List<Trade> GetTradeList()
        {
            return GetTradeList(Default);
        }

        /// <summary>
        /// Resets this instance, clears all orders/trades/accounts held by the broker.
        /// </summary>
        public void Reset()
        {
            Acctlist.Clear();
            MasterOrders.Clear();
            MasterTrades.Clear();
            AddAccount(Default);
        }

        /// <summary>
        /// send order that is compatible with OrderDelegate events
        /// </summary>
        /// <param name="o"></param>
        public void SendOrder(PendingOrder o)
        {
            SendOrderStatus(o);
        }

        /// <summary>
        /// Sends the order to the broker for a specific account.
        /// </summary>
        /// <param name="o">The order to be sent.</param>
        /// <param name="a">the account to send with the order.</param>
        /// <returns>status code</returns>
        public int SendOrderAccount(PendingOrder o, SimAccount a)
        {
            if (o.OrderId == 0) // if order id isn't set, set it
            {
                OrderImpl order = (OrderImpl)o.Order;
                order.Id = _nextorderid++;
            }

            AddOrder(o, a);

            if ((GotOrder != null) && a.Notify)
                GotOrder(o);

            return (int)StatusType.OK;
        }

        /// <summary>
        /// Sends the order to the broker. (uses the default account)
        /// </summary>
        /// <param name="o">The order to be send.</param>
        /// <returns>status code</returns>
        public int SendOrderStatus(PendingOrder o)
        {
            if (!o.Order.IsValid || CheckOrderIntegrity(o) != StatusType.OK)
            {
                o.Cancel();
                return (int)StatusType.INVALID_TRADE_PARAMETERS;
            }

            // make sure book is clearly stamped
            if (o.Order.AccountName.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                OrderImpl order = (OrderImpl)o.Order;
                order.AccountName = Default.Id;
                return SendOrderAccount(o, Default);
            }

            // get account
            SimAccount a;
            if (!Acctlist.TryGetValue(o.Order.AccountName, out a))
            {
                a = new SimAccount(o.Order.AccountName);
                AddAccount(a);
            }
            return SendOrderAccount(o, a);
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Add account to this broker (in case multi account trading is used)
        /// </summary>
        /// <param name="a"></param>
        protected void AddAccount(SimAccount a)
        {
            SimAccount t;
            if (Acctlist.TryGetValue(a.Id, out t)) return; // already had it
            MasterOrders.Add(a, new List<PendingOrder>());
            MasterTrades.Add(a.Id, new List<Trade>());
            Acctlist.Add(a.Id, a);
        }

        /// <summary>
        /// Add pending order to local orderbook
        /// </summary>
        /// <param name="o"></param>
        /// <param name="a"></param>
        protected void AddOrder(PendingOrder o, SimAccount a)
        {
            if (!a.IsValid)
                throw new Exception("Invalid account provided"); // account must be good

            //Add event handlers
            o.OnCancel += HandleOrderCancel;
            o.OnUpdate += HandleOrderUpdate;

            // add any remaining order to book as new liquidity route
            List<PendingOrder> tmp;

            // see if we have a book for this account
            if (!MasterOrders.TryGetValue(a, out tmp))
            {
                tmp = new List<PendingOrder>();
                MasterOrders.Add(a, tmp); // if not, create one
            }
            OrderImpl order = (OrderImpl)o.Order;
            order.AccountName = a.Id; // make sure order knows his account
            tmp.Add(o); // record the order
            // increment pending count
            _pendorders++;
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Check for order integrity from the brokers perspective
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private StatusType CheckOrderIntegrity(PendingOrder o)
        {
            PendingOrderImpl porder = (PendingOrderImpl)o;

            //Check if size is correct
            return porder.OrderStatus;
        }

        /// <summary>
        /// Process order cancel events
        /// </summary>
        /// <param name="pendingorder"></param>
        private void HandleOrderCancel(PendingOrder pendingorder)
        {
            //Check if order exists
            PendingOrder currentorder = null;
            PendingOrderImpl changedorder = (PendingOrderImpl)pendingorder;

            var account = pendingorder.Account ?? Default;

            if (account == null)
            {
                changedorder.OrderStatus = StatusType.INVALID_ACCOUNT;
                return;
            }

            var masterorders = MasterOrders[account];

            foreach (var po in masterorders)
                if (po.OrderId == pendingorder.OrderId)
                    currentorder = po;

            //Check if we could find this order
            if (currentorder == null)
            {
                changedorder.OrderStatus = StatusType.ORDER_NOT_FOUND;
                return;
            }

            //Send changes to the broker book
            masterorders.Remove(pendingorder);

            //Notify cancelation
            if (GotOrderCancel != null)
                GotOrderCancel(pendingorder);
        }

        /// <summary>
        /// Process order update event
        /// </summary>
        /// <param name="pendingorder"></param>
        private void HandleOrderUpdate(PendingOrder pendingorder)
        {
            //Check if order exists
            PendingOrder currentorder = null;
            PendingOrderImpl changedorder = (PendingOrderImpl)pendingorder;

            var account = pendingorder.Account ?? Default;

            if (account == null)
            {
                changedorder.OrderStatus = StatusType.INVALID_ACCOUNT;
                return;
            }

            var masterorders = MasterOrders[account];

            foreach (var po in masterorders)
                if (po.OrderId == pendingorder.OrderId)
                    currentorder = po;

            //Check if we could find this order
            if (currentorder == null)
            {
                changedorder.Cancel();
                changedorder.OrderStatus = StatusType.ORDER_NOT_FOUND;
                return;
            }
            //Check if we need to remove this order
            if (!pendingorder.Order.IsValid || pendingorder.IsCancelled)
                pendingorder.Cancel();

            //Send updates to any templates
            if (GotOrderUpdate != null)
                GotOrderUpdate(pendingorder);
        }

        #endregion Private Methods
    }
}