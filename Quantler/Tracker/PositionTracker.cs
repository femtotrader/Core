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
using Quantler.Trades;
using System;
using System.Collections.Generic;

namespace Quantler.Tracker
{
    /// <summary>
    /// easily trade positions for a collection of securities. automatically builds positions from
    /// existing positions and new trades.
    /// </summary>
    public class PositionTracker : GenericTrackerImpl<Position>, IPositionTracker
    {
        #region Private Fields

        private readonly Dictionary<string, ISecurityTracker> _securitytracker = new Dictionary<string, ISecurityTracker>();
        private IAccount _defaultacct;

        private decimal _totalclosedpl;

        #endregion Private Fields

        #region Public Constructors

        public PositionTracker(IAccount defaultaccount)
        {
            if (defaultaccount != null)
            {
                DefaultAccount = defaultaccount;
                _securitytracker[defaultaccount.Id] = defaultaccount.Securities;
            }
        }

        #endregion Public Constructors

        #region Public Events

        /// <summary>
        /// called when a new position is added to tracker.
        /// </summary>
        public event PositionUpdateDelegate OnPositionUpdate;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Default account used when querying positions (if never set by user, defaults to first
        /// account provided via adjust)
        /// </summary>
        public IAccount DefaultAccount { get { return _defaultacct; } set { _defaultacct = value; } }

        public ISecurity[] Securities { get { return DefaultAccount.Securities.ToArray(); } }

        /// <summary>
        /// gets sum of all closed pl for all positions
        /// </summary>
        public decimal TotalClosedPL { get { return _totalclosedpl; } }

        public override Type TrackedType
        {
            get
            {
                return typeof(Position);
            }
        }

        #endregion Public Properties

        #region Public Indexers

        /// <summary>
        /// get position given positions symbol (assumes default account)
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public new Position this[string symbol]
        {
            get
            {
                int idx = getindex(symbol + GetAccountId());
                if (idx < 0)
                {
                    var sec = _securitytracker[GetAccountId()][symbol];
                    var toadd = new PositionImpl(sec, 0, 0, 0, DefaultAccount);
                    addindex(symbol + GetAccountId(), toadd);
                    AddSecurity(sec, DefaultAccount);
                    return toadd;
                }
                return this[idx];
            }
        }

        /// <summary>
        /// get a position in tracker given symbol and account
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        public Position this[string symbol, IAccount account]
        {
            get
            {
                int idx = getindex(symbol + account.Id);
                if (idx < 0)
                {
                    var sec = _securitytracker[DefaultAccount.Id][symbol];
                    var toadd = new PositionImpl(sec, 0, 0, 0, DefaultAccount);
                    addindex(symbol + account.Id, toadd);
                    AddSecurity(sec, account);
                    return toadd;
                }
                return this[idx];
            }
        }

        /// <summary>
        /// get position given positions symbol (assumes default account)
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public new Position this[int idx]
        {
            get
            {
                if (idx < 0)
                    return new PositionImpl();
                Position p = base[idx];
                if (p == null)
                {
                    var sec = _securitytracker[GetAccountId()][getlabel(idx)];
                    var toadd = new PositionImpl(sec);
                    addindex(sec.Name + GetAccountId(), toadd);
                    AddSecurity(sec, DefaultAccount);
                    return toadd;
                }
                return p;
            }
        }

        /// <summary>
        /// get position given positions security (assumes default account)
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public Position this[ISecurity security]
        {
            get
            {
                int idx = getindex(security.Name + GetAccountId());
                if (idx < 0)
                {
                    var toadd = new PositionImpl(security, 0, 0, 0, DefaultAccount);
                    addindex(security.Name + GetAccountId(), toadd);
                    AddSecurity(security, DefaultAccount);
                    return toadd;
                }
                return this[idx];
            }
        }

        /// <summary>
        /// get a position in tracker given security and account
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        public Position this[ISecurity security, IAccount account]
        {
            get
            {
                int idx = getindex(security.Name + account.Id);
                if (idx < 0)
                {
                    var toadd = new PositionImpl(security, 0, 0, 0, DefaultAccount);
                    addindex(security.Name + account.Id, toadd);
                    AddSecurity(security, account);
                    return toadd;
                }
                return this[idx];
            }
        }

        #endregion Public Indexers

        #region Public Methods

        /// <summary>
        /// Adjust an existing position, or create new one if none exists.
        /// </summary>
        /// <param name="fill"></param>
        /// <returns>any closed PL for adjustment</returns>
        public decimal Adjust(Trade fill)
        {
            int idx = getindex(fill.Symbol + fill.Account.Id);
            return Adjust(fill, idx);
        }

        /// <summary>
        /// Adjust an existing position, or create a new one... given a trade and symbol+account index
        /// </summary>
        /// <param name="fill"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        public decimal Adjust(Trade fill, int idx)
        {
            decimal cpl = 0;

            if (_defaultacct == null)
                _defaultacct = fill.Account;

            var pos = new PositionImpl(fill);

            if (idx < 0)
                addindex(fill.Symbol + fill.Account.Id, pos);
            else
            {
                cpl += ((PositionImpl)this[idx]).Adjust(fill);
            }
            _totalclosedpl += cpl;

            //send event for position updated
            if (OnPositionUpdate != null)
                OnPositionUpdate(pos, fill, cpl);

            return cpl;
        }

        /// <summary>
        /// overwrite existing position, or start new position
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public decimal Adjust(Position newpos)
        {
            if (_defaultacct == null)
                _defaultacct = newpos.Account;

            int idx = getindex(newpos.Security.Name + newpos.Account.Id);

            if (idx < 0)
                addindex(newpos.Security.Name + newpos.Account.Id, new PositionImpl(newpos));
            else
            {
                base[idx] = new PositionImpl(newpos);
                _totalclosedpl += newpos.GrossPnL;
            }

            return 0;
        }

        /// <summary>
        /// clear all positions. use with caution. also resets default account.
        /// </summary>
        public override void Clear()
        {
            _defaultacct = null;
            base.Clear();
        }

        public new IEnumerator<Position> GetEnumerator()
        {
            List<Position> toreturn = new List<Position>();
            for (int i = 0; i < Count; i++)
                toreturn.Add(this[i]);
            return toreturn.GetEnumerator();
        }

        public void GotFill(Trade f)
        {
            Adjust(f);
        }

        public void GotPosition(Position p)
        {
            Adjust(p);
        }

        /// <summary>
        /// Create a new position, or overwrite existing position
        /// </summary>
        /// <param name="newpos"></param>
        public void NewPosition(Position newpos)
        {
            Adjust(newpos);
        }

        #endregion Public Methods

        #region Private Methods

        private void AddSecurity(ISecurity sec, IAccount account)
        {
            if (account == null || sec == null)
                return;

            if (_securitytracker.ContainsKey(account.Id))
                _securitytracker[account.Id].AddSecurity(sec);
            else
            {
                _securitytracker.Add(account.Id, account.Securities);
                _securitytracker[account.Id].AddSecurity(sec);
            }
        }

        private string GetAccountId()
        {
            return DefaultAccount != null ? DefaultAccount.Id : string.Empty;
        }

        #endregion Private Methods
    }
}