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
using System;
using System.Globalization;

namespace Quantler.Trades
{
    /// <summary>
    /// A trade or execution of an order. Also called a fill.
    /// </summary>
    [Serializable]
    public class TradeImpl : Trade
    {
        #region Private Fields

        private string _accountid = "";
        private int _agentid;
        private string _comment = "";
        private CurrencyType _cur = CurrencyType.USD;
        private long _id;
        private string _localsymbol = "";
        private string _sym = "";
        private int _xdate;
        private decimal _xprice;
        private int _xsize;
        private int _xtime;

        #endregion Private Fields

        #region Public Constructors

        public TradeImpl(Trade copytrade)
        {
            // copy constructor, for copying using by-value (rather than by default of by-reference)
            Id = copytrade.Id;
            _cur = copytrade.Currency;
            Security = copytrade.Security;
            Exchange = copytrade.Exchange;
            _accountid = copytrade.AccountName;
            Symbol = copytrade.Symbol;
            Direction = copytrade.Direction;
            Commission = copytrade.Commission;
            AgentId = copytrade.AgentId;

            Xsize = copytrade.Xsize;
            Xprice = copytrade.Xprice;
            Xtime = copytrade.Xtime;
            Xdate = copytrade.Xdate;
        }

        public TradeImpl()
        {
        }

        /// <summary>
        /// true if this is a real Trade, otherwise it's still an order.
        /// </summary>
        public TradeImpl(string symbol, decimal fillprice, int fillsize)
            : this(symbol, fillprice, fillsize, DateTime.Now)
        {
        }

        public TradeImpl(string sym, decimal fillprice, int fillsize, DateTime tradedate)
            : this(sym, fillprice, fillsize, Util.ToQLDate(tradedate), Util.DT2FT(tradedate))
        {
        }

        public TradeImpl(string sym, decimal fillprice, int fillsize, int filldate, int filltime)
        {
            if (sym != null) Symbol = sym.ToUpper();
            if ((fillsize == 0) || (fillprice == 0)) throw new Exception("Invalid trade: Zero price or size provided.");
            Xtime = filltime;
            Xdate = filldate;
            Xsize = fillsize;
            Xprice = fillprice;
            Direction = fillsize > 0 ? Direction.Long : Direction.Short;
        }

        #endregion Public Constructors

        #region Public Properties

        public IAccount Account { get; set; }

        public string AccountName { get { return _accountid; } set { _accountid = value; } }

        public int AgentId { get { return _agentid; } set { _agentid = value; } }

        public string BrokerSymbol { get { return _localsymbol; } set { _localsymbol = value; } }
        public string Comment { get { return _comment; } set { _comment = value; } }

        public decimal Commission { get; set; }

        public CurrencyType Currency { get { return _cur; } set { _cur = value; } }

        public Direction Direction { get; set; }

        public string Exchange { get; set; }

        public DateTime Executed { get { return Util.ToDateTime(Xdate, Xtime); } }

        public long Id { get { return _id; } set { _id = value; } }

        public bool IsFilled
        {
            get { return Xprice * Xsize != 0; }
        }

        public virtual bool IsValid { get { return (Xsize != 0) && (Xprice != 0) && (Xtime + Xdate != 0) && !string.IsNullOrEmpty(Symbol); } }
        public virtual decimal Price { get { return Xprice; } }

        public ISecurity Security { get; set; }

        public decimal Swap { get; set; }

        public string Symbol { get { return _sym; } set { _sym = value; } }

        public int UnsignedSize { get { return Math.Abs(_xsize); } }

        public int Xdate { get { return _xdate; } set { _xdate = value; } }

        public decimal Xprice { get { return _xprice; } set { _xprice = value; } }

        public decimal Xquantity
        {
            get { return Xsize / Security.LotSize; }
        }

        public int Xsize { get { return _xsize; } set { _xsize = value; } }

        public int Xtime { get { return _xtime; } set { _xtime = value; } }

        #endregion Public Properties

        #region Public Methods

        public static Trade FromString(string tradestring)
        {
            return FromString(tradestring, ',');
        }

        public static Trade FromString(string tradestring, char delimiter)
        {
            string[] r = tradestring.Split(delimiter);
            TradeImpl t = new TradeImpl
            {
                Xdate = Convert.ToInt32(r[0], CultureInfo.InvariantCulture),
                Xtime = Convert.ToInt32(r[1], CultureInfo.InvariantCulture),
                Symbol = r[2],
                AccountName = r[6],
                Xprice = Convert.ToDecimal(r[5], CultureInfo.InvariantCulture),
                Direction = (Direction)Enum.Parse(typeof(Direction), r[3]),
                Xsize = Convert.ToInt32(r[4], CultureInfo.InvariantCulture)
            };
            return t;
        }

        public override string ToString()
        {
            return ToString(',', true);
        }

        public string ToString(bool includeid)
        {
            return ToString(',', includeid);
        }

        public string ToString(char delimiter)
        {
            return ToString(delimiter, true);
        }

        public string ToString(char delimiter, bool includeid)
        {
            int usize = Math.Abs(Xsize);
            string[] trade = { Xdate.ToString(CultureInfo.InvariantCulture), Xtime.ToString(CultureInfo.InvariantCulture), Symbol, Direction == Direction.Long ? "Long" : "Short", usize.ToString(CultureInfo.InvariantCulture), Xprice.ToString("0.00#####", CultureInfo.InvariantCulture), AccountName };
            if (!includeid)
                return string.Join(delimiter.ToString(), trade);
            return string.Join(delimiter.ToString(), trade) + delimiter + Id;
        }

        #endregion Public Methods
    }
}