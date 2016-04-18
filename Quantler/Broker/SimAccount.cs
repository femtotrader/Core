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

using Quantler.Interfaces;
using Quantler.Tracker;
using Quantler.Trades;
using System;
using System.Collections.Generic;
using System.Linq;
using Quantler.Securities;

namespace Quantler.Broker
{
    /// <summary>
    /// Account information and reference for current states (Simulated account, used for backtesting)
    /// </summary>
    public class SimAccount : IAccount
    {
        #region Private Fields

        private readonly IPositionTracker _currentpositions;
        private readonly string _id;
        private readonly Dictionary<string, string> _pipvalueconversionsymbols;
        private readonly Dictionary<string, string> _positionvalueconversionsymbols;
        private readonly Dictionary<string, Tick> _priceinformation = new Dictionary<string, Tick>();
        private readonly ISecurityTracker _securities;
        private readonly decimal _startingbalance;
        private bool _execute = true;
        private decimal _mutations;
        private bool _notify = true;

        #endregion Private Fields

        #region Public Constructors

        public SimAccount()
            : this("empty", "", 10000, 100, "Unkown")
        {
        }

        public SimAccount(string accountId)
            : this(accountId, "", 10000, 100, "Unkown")
        {
        }

        public SimAccount(string accountId, string description)
            : this(accountId, description, 10000, 100, "Unkown")
        {
        }

        public SimAccount(string accountId, string description, decimal startingbalance, int leverage, string name)
        {
            _id = accountId;
            Desc = description;
            _startingbalance = startingbalance;
            Leverage = leverage;

            _securities = new SecurityTracker<ForexSecurity>(name);
            _currentpositions = new PositionTracker(this);
            _pipvalueconversionsymbols = Util.GetPipValueSymbolCrosses(Currency);
            _positionvalueconversionsymbols = Util.GetPositionValueSymbolCrosses(Currency);
        }

        #endregion Public Constructors

        #region Public Properties

        public decimal Balance
        {
            get { return _startingbalance + _mutations; }
        }

        public string Client
        {
            get { return "BacktestUser"; }
        }

        public string Company
        {
            get { return "Quantler"; }
        }

        public CurrencyType Currency
        {
            get { return CurrencyType.USD; }
        }

        /// <summary>
        /// Gets or sets the description for this account.
        /// </summary>
        /// <value>The desc.</value>
        public string Desc { get; set; }

        public decimal Equity
        {
            get
            {
                return Balance + _currentpositions.Securities
                    .Where(sym => !_currentpositions[sym].IsFlat)
                    .Sum(sym => Calc.ClosePL(_currentpositions[sym],
                        new TradeImpl(sym.Name, _priceinformation[sym.Name].HasBid ?
                            _priceinformation[sym.Name].Bid :
                            _priceinformation[sym.Name].Trade, int.MaxValue)));
            }
        }

        public bool Execute { get { return _execute; } set { _execute = value; } }

        public decimal FloatingPnL
        {
            get { return Equity - Balance; }
        }

        public decimal FreeMargin
        {
            get { return Math.Abs(Equity - Margin); }
        }

        public string Id { get { return _id; } }

        public bool IsLiveTrading
        {
            get
            {
                return false;
            }
        }

        public bool IsTradingAllowed
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid { get { return !String.IsNullOrWhiteSpace(Id); } }

        /// <summary>
        /// Sim account does not have latency (1ms)
        /// </summary>
        public int Latency { get { return 1; } }

        public int Leverage
        {
            get;
            private set;
        }

        public decimal Margin
        {
            // Sum of the open positions / Leverage
            get
            {
                decimal toreturn = 0;
                foreach (var sym in _currentpositions.Securities)
                {
                    Position pos = _currentpositions[sym];
                    if (pos.IsFlat)
                        continue;

                    string basesymbol;

                    if (_positionvalueconversionsymbols.TryGetValue(sym.Name, out basesymbol))
                    {
                        decimal price;

                        if (basesymbol == "USDUSD")
                            price = 1;
                        else
                            price = _priceinformation[basesymbol].HasBid ? _priceinformation[basesymbol].Bid : _priceinformation[basesymbol].Trade;

                        //Margin = (Trade Size (lot size) / leverage) * account currency exchange rate (if different from the base currency in the pair being traded).
                        toreturn += pos.UnsignedSize / Leverage * price;
                    }
                }
                return toreturn;
            }
        }

        public decimal MarginLevel
        {
            get { return Margin > 0 ? Equity / Margin * 100 : Equity * 100; }
        }

        public bool Notify { get { return _notify; } set { _notify = value; } }

        public IPositionTracker Positions { get { return _currentpositions; } }

        public ISecurityTracker Securities { get { return _securities; } }

        public string Server
        {
            get { return "Test"; }
        }

        public decimal StopOutLevel
        {
            //TODO: allign with broker model
            get { return 20; }
        }

        #endregion Public Properties

        #region Public Methods

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            SimAccount o = (SimAccount)obj;
            return Equals(o);
        }

        public bool Equals(SimAccount a)
        {
            return _id.Equals(a.Id);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public void OnFill(Trade t)
        {
            Positions.GotFill(t);
            _mutations = Positions.TotalClosedPL;
        }

        public void OnTick(Tick t)
        {
            if (_priceinformation.ContainsKey(t.Symbol))
            {
                _priceinformation[t.Symbol] = t;

                //Update security info
                ForexSecurity sec = (ForexSecurity)_securities[t.Symbol];
                UpdateSecurity(t, sec);
            }
            else
                _priceinformation.Add(t.Symbol, t);
        }

        public override string ToString()
        {
            return Id;
        }

        #endregion Public Methods

        #region Private Methods

        private void UpdateSecurity(Tick t, ForexSecurity sec)
        {
            if (sec == null || !t.IsValid)
                return;

            //Set bid ask
            sec.Bid = t.Bid;
            sec.Ask = t.Ask;

            //Set spread information
            if (t.IsFullQuote)
                sec.Spread = (int)((double)(t.Ask - t.Bid) * Math.Pow(10, sec.Digits));

            //Set pip value
            string conversionsymbol;
            if (_pipvalueconversionsymbols.TryGetValue(t.Symbol, out conversionsymbol))
            {
                if (_priceinformation.ContainsKey(conversionsymbol))
                {
                    decimal price = _priceinformation[conversionsymbol].IsFullQuote ? _priceinformation[conversionsymbol].Bid : _priceinformation[conversionsymbol].Trade;
                    sec.PipValue = sec.PipSize / price * sec.LotSize;
                }
                else if (conversionsymbol == "USDUSD")
                {
                    sec.PipValue = 10 * (sec.LotSize / 100000M);
                }
            }
        }

        #endregion Private Methods
    }
}