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
using System.Collections.Generic;
using System.Linq;
using Quantler.Securities;

namespace Quantler.Trades
{
    /// <summary>
    /// A position type used to describe the position in a stock or instrument.
    /// </summary>
    public class PositionImpl : Position
    {
        #region Private Fields

        private readonly int _date;
        private readonly int _time;
        private readonly List<Trade> _trades;
        private decimal _closedpl;
        private DateTime _lastadjust;
        private decimal _price;

        private int _size;

        #endregion Private Fields

        #region Public Constructors

        public PositionImpl()
            : this(new ForexSecurity(""))
        {
        }

        public PositionImpl(Position p)
            : this(p.Security, p.AvgPrice, p.Size, p.GrossPnL, p.Account)
        {
        }

        public PositionImpl(ISecurity security)
            : this(security, 0, 0, 0, null)
        {
        }

        public PositionImpl(ISecurity security, decimal price, int size)
            : this(security, price, size, 0, null)
        {
        }

        public PositionImpl(ISecurity security, decimal price, int size, decimal closedpl)
            : this(security, price, size, closedpl, null)
        {
        }

        public PositionImpl(ISecurity security, decimal price, int size, decimal closedpl, IAccount account)
        {
            Security = security;
            if (size == 0) price = 0;
            _price = price;
            _size = size;
            _closedpl = closedpl;
            Account = account;
            _trades = new List<Trade>();
            if (!IsValid)
                throw new Exception("Can't construct invalid position!");
            _lastadjust = security.LastTickEvent;
        }

        public PositionImpl(Trade t)
        {
            if (!t.IsValid) throw new Exception("Can't construct a position object from invalid trade.");
            Security = t.Security;
            _price = t.Xprice;
            _size = t.Xsize;
            _date = t.Xdate;
            _time = t.Xtime;
            Account = t.Account;
            _trades = new List<Trade> { t };
            if (_size > 0) _size *= t.Direction == Direction.Long ? 1 : -1;
            _lastadjust = Util.ToDateTime(t.Xdate, t.Xtime);
        }

        #endregion Public Constructors

        #region Public Properties

        public IAccount Account { get; private set; }

        public decimal AvgPrice { get { return _price; } } //TODO: should be weighted average price, based on active trades

        public Direction Direction { get { return Size == 0 ? Direction.Flat : Size > 0 ? Direction.Long : Direction.Short; } }
        public decimal FlatQuantity { get { return (decimal)FlatSize / Security.LotSize; } }
        public int FlatSize { get { return _size * -1; } }
        public decimal GrossPnL { get { return _closedpl; } }

        public bool IsFlat { get { return Direction == Direction.Flat; } }
        public bool IsLong { get { return Direction == Direction.Long; } }
        public bool IsShort { get { return Direction == Direction.Short; } }

        public bool IsValid
        {
            get { return (Security != null) && (((AvgPrice == 0) && (Size == 0)) || ((AvgPrice != 0) && (Size != 0))); }
        }

        public DateTime LastModified { get { return _lastadjust; } }
        public decimal NetPnL { get { return GrossPnL - TotalCommission + TotalSwap; } }
        public decimal Price { get { return _price; } }
        public decimal Quantity { get { return (decimal)Size / Security.LotSize; } }
        public ISecurity Security { get; private set; }
        public int Size { get { return _size; } }
        public decimal TotalCommission { get { return Trades.Sum(x => x.Commission); } }
        public decimal TotalSwap { get { return Trades.Sum(x => x.Swap); } }
        public Trade[] Trades { get { return _trades.ToArray(); } }
        public int UnsignedSize { get { return Math.Abs(_size); } }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// convert from
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static implicit operator bool(PositionImpl p)
        {
            return !p.IsFlat;
        }

        /// <summary>
        /// convert from position to decimal (price)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static implicit operator decimal(PositionImpl p)
        {
            return p.AvgPrice;
        }

        /// <summary>
        /// convert from position to integer (size)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static implicit operator int(PositionImpl p)
        {
            return p.Size;
        }

        /// <summary>
        /// Adjusts the position by applying a new position.
        /// returns any closed PL calculated on position basis (not per share)
        /// </summary>
        /// <param name="pos">The position adjustment to apply.</param>
        /// <returns></returns>
        public decimal Adjust(Position pos)
        {
            if ((Security != null) && (Security.Name != pos.Security.Name || Security.DestEx != pos.Security.DestEx)) throw new Exception("Failed because adjustment symbol did not match position symbol");
            if (Account == null) Account = pos.Account;
            //if (_acct != pos.Account.ID) throw new Exception("Failed because adjustment account did not match position account.");
            if ((Security == null) && pos.IsValid) ;
            if (!pos.IsValid) throw new Exception("Invalid position adjustment, existing:" + ToString() + " adjustment:" + pos);
            if (pos.IsFlat) return 0; // nothing to do
            bool oldside = IsLong;
            decimal pl = Calc.ClosePL(this, pos.ToTrade());
            if (IsFlat) _price = pos.AvgPrice; // if we're leaving flat just copy price
            else if ((pos.IsLong && IsLong) || (!pos.IsLong && !IsLong)) // sides match, adding so adjust price
                _price = (_price * _size + pos.AvgPrice * pos.Size) / (pos.Size + Size);
            _size += pos.Size; // adjust the size
            if (oldside != IsLong) _price = pos.AvgPrice; // this is for when broker allows flipping sides in one trade
            if (IsFlat) _price = 0; // if we're flat after adjusting, size price back to zero
            _closedpl += pl; // update running closed pl

            //Set trades this position is based on
            if (Direction == Direction.Flat)
                _trades.Clear();
            else if (pos.Direction == Direction)
                _trades.AddRange(pos.Trades);
            else
            {
                _trades.Clear();
                _trades.AddRange(pos.Trades);
            }

            return pl;
        }

        /// <summary>
        /// Adjusts the position by applying a new trade or fill.
        /// </summary>
        /// <param name="t">The new fill you want this position to reflect.</param>
        /// <returns></returns>
        public decimal Adjust(Trade t)
        {
            _lastadjust = t.Executed;
            return Adjust(new PositionImpl(t));
        }

        public override string ToString()
        {
            return Security.Name + " " + Size + "@" + AvgPrice.ToString("F2") + " [" + Account.Id + "]";
        }

        public Trade ToTrade()
        {
            DateTime dt = _date * _time != 0 ? Util.ToDateTime(_date, _time) : DateTime.Now;
            return new TradeImpl(Security.Name, AvgPrice, Size, dt);
        }

        #endregion Public Methods
    }
}