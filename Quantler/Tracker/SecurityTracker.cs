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

namespace Quantler.Tracker
{
    /// <summary>
    /// Tracks and contains multiple securities, can be used as a single reference point for all securities used
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SecurityTracker<T> : GenericTrackerImpl<ISecurity>, ISecurityTracker
        where T : ISecurity
    {
        #region Public Constructors

        /// <summary>
        /// Initialize a new securitytracker object
        /// </summary>
        public SecurityTracker(string defaultExchange)
        {
            DefaultExchange = defaultExchange;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Default exchange for this tracker
        /// </summary>
        public string DefaultExchange { get; private set; }

        #endregion Public Properties

        #region Public Indexers

        /// <summary>
        /// Get the security object based on the symbolsname
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public new ISecurity this[string symbol]
        {
            get
            {
                int idx = getindex(symbol + DefaultExchange);
                if (idx < 0)
                {
                    var security = (T)Activator.CreateInstance(typeof(T), symbol);
                    AddSecurity(security);
                    return security;
                }
                return this[idx];
            }
        }

        /// <summary>
        /// Get the security object based on the symbol and the associated exchange
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="exchange"></param>
        /// <returns></returns>
        public ISecurity this[string symbol, string exchange]
        {
            get
            {
                int idx = getindex(symbol + exchange);
                if (idx < 0)
                {
                    var security = (T)Activator.CreateInstance(typeof(T), symbol);
                    AddSecurity(security);
                    return security;
                }
                return this[idx];
            }
        }

        /// <summary>
        /// Get the security object based on the index location
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public new ISecurity this[int idx]
        {
            get
            {
                return idx < 0 ? (T)Activator.CreateInstance(typeof(T), "UNKOWN") : base[idx];
            }
        }

        #endregion Public Indexers

        #region Public Methods

        /// <summary>
        /// Add a new security to the security tracker
        /// </summary>
        /// <param name="security"></param>
        public void AddSecurity(ISecurity security)
        {
            //Try and get the current index
            int idx = getindex(security.Name + security.DestEx);

            //If index does not exist, add it
            if (idx < 0)
                addindex(security.Name + security.DestEx, security);
        }

        #endregion Public Methods
    }
}