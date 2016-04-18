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

using Quantler.Interfaces;
using System;

namespace Quantler.Data.Bars
{
    /// <summary>
    /// Indexer for retrieving specific bar information
    /// </summary>
    public class BarIndexerImpl : BarIndexer
    {
        #region Private Fields

        /// <summary>
        /// Portfolio where datastreams are hold
        /// </summary>
        private readonly PortfolioManager _portfolio;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initialize a new barindexer using a predefined portfolio
        /// </summary>
        /// <param name="portfolio"></param>
        public BarIndexerImpl(PortfolioManager portfolio)
        {
            _portfolio = portfolio;
        }

        #endregion Public Constructors

        #region Public Indexers

        /// <summary>
        /// Get Bar based on the default timeframe
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public Bar this[string symbol]
        {
            get
            {
                var result = GetList(symbol);
                if (result != null)
                    return result[-1];
                return null;
            }
        }

        /// <summary>
        /// Get Bar based on a specific timeframe
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public Bar this[string symbol, int interval]
        {
            get
            {
                var result = GetList(symbol, interval);
                if (result != null)
                    return result[-1];
                return null;
            }
        }

        /// <summary>
        /// Get Bar based on a specific timeframe
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public Bar this[string symbol, TimeSpan interval]
        {
            get
            {
                var result = GetList(symbol, (int)interval.TotalSeconds);
                if (result != null)
                    return result[-1];
                return null;
            }
        }

        /// <summary>
        /// Get Bar based on a specific timeframe and x amount of bars back
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="barsBack"></param>
        /// <returns></returns>
        public Bar this[string symbol, int interval, int barsBack]
        {
            get
            {
                var result = GetList(symbol, interval);
                if (result != null)
                    return result[-barsBack];
                return null;
            }
        }

        /// <summary>
        /// Get Bar based on a specific timeframe and x amount of bars back
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="barsBack"></param>
        /// <returns></returns>
        public Bar this[string symbol, TimeSpan interval, int barsBack]
        {
            get
            {
                var result = GetList(symbol, (int)interval.TotalSeconds);
                if (result != null)
                    return result[-barsBack];
                return null;
            }
        }

        /// <summary>
        /// Get Bar based on a security and using its default timeframe
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public Bar this[ISecurity symbol]
        {
            get
            {
                var result = GetList(symbol.Name);
                if (result != null)
                    return result[-1];
                return null;
            }
        }

        /// <summary>
        /// Get Bar based on security and a specific timeframe
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public Bar this[ISecurity symbol, int interval]
        {
            get
            {
                var result = GetList(symbol.Name, interval);
                if (result != null)
                    return result[-1];
                return null;
            }
        }

        /// <summary>
        /// Get Bar based on security and a specific timeframe
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public Bar this[ISecurity symbol, TimeSpan interval]
        {
            get
            {
                var result = GetList(symbol.Name, (int)interval.TotalSeconds);
                if (result != null)
                    return result[-1];
                return null;
            }
        }

        /// <summary>
        /// Get Bar based on a specific timeframe and x amount of bars back
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="barsBack"></param>
        /// <returns></returns>
        public Bar this[ISecurity symbol, int interval, int barsBack]
        {
            get
            {
                var result = GetList(symbol.Name, interval);
                if (result != null)
                    return result[-barsBack];
                return null;
            }
        }

        /// <summary>
        /// Get Bar based on a specific timeframe and x amount of bars back
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="barsBack"></param>
        /// <returns></returns>
        public Bar this[ISecurity symbol, TimeSpan interval, int barsBack]
        {
            get
            {
                var result = GetList(symbol.Name, (int)interval.TotalSeconds);
                if (result != null)
                    return result[-barsBack];
                return null;
            }
        }

        #endregion Public Indexers

        #region Private Methods

        /// <summary>
        /// Get the BarList needed for processing the request, returns null if the combination does not exist
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        private BarList GetList(string symbol, int interval = -1)
        {
            //Check if the stream exists
            if (!_portfolio.Streams.ContainsKey(symbol))
                return null;

            //Get and return the stream
            var stream = _portfolio.Streams[symbol];
            if (interval < 0)
            {
                return stream[stream.DefaultInterval];
            }
            return stream[interval];
        }

        #endregion Private Methods
    }
}