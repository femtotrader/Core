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

namespace Quantler.Interfaces
{
    /// <summary>
    /// Tracks security information
    /// </summary>
    public interface ISecurityTracker
    {
        #region Public Properties

        /// <summary>
        /// Default exchange for which the securities are traded on, this could be a different broker as well
        /// </summary>
        string DefaultExchange { get; }

        #endregion Public Properties

        #region Public Indexers

        /// <summary>
        /// Return a security based on its index
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        ISecurity this[int idx] { get; }

        /// <summary>
        /// Return a security based on the symbol name and the exchange on which it is traded
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="exchange"></param>
        /// <returns></returns>
        ISecurity this[string symbol, string exchange] { get; }

        /// <summary>
        /// Return a security based on the symbol name
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        ISecurity this[string symbol] { get; }

        #endregion Public Indexers

        #region Public Methods

        /// <summary>
        /// Add a new security to the security tracker for general use
        /// </summary>
        /// <param name="security"></param>
        void AddSecurity(ISecurity security);

        /// <summary>
        /// Get all current securities in an array
        /// </summary>
        /// <returns></returns>
        ISecurity[] ToArray();

        #endregion Public Methods
    }
}