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

using System.Collections.Generic;

namespace Quantler.Interfaces
{
    /// <summary>
    /// Manages all indicators in a portfolio to make sure duplicates will not exist
    /// </summary>
    public interface IndicatorManager
    {
        #region Public Properties

        /// <summary>
        /// Returns the trading agent this indicator manager is associated with
        /// </summary>
        ITradingAgent Agent { get; }

        /// <summary>
        /// Currently subscribed indicators
        /// </summary>
        List<Indicator> Indicators { get; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Subscribe a new indicator and check if there are no duplicates
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indicator"></param>
        /// <returns></returns>
        T Subscribe<T>(T indicator);

        #endregion Public Methods
    }
}