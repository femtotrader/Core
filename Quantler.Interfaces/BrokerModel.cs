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
    /// Defines the settings of a broker (Commissions, Typical Spreads etc..)
    /// </summary>
    public interface BrokerModel
    {
        #region Public Methods

        /// <summary>
        /// Calculate margin interest (swap) for this order
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        decimal CalculateMarginInterest(Order o);

        /// <summary>
        /// Returns the total commission for the order presented
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        decimal GetCommission(Order o);

        /// <summary>
        /// Get the current latency used before allowing this order to be executed, in milliseconds
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        int GetLatencyInMilliseconds(Order o);

        /// <summary>
        /// Get the current slippage used for executing this order in pips
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        decimal GetSlippage(Order o);

        /// <summary>
        /// Get the current spread used for this order in pips
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        decimal GetSpread(Order o);

        /// <summary>
        /// Returns the minimum order size (1.000 equals 1 microlot)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        int MinimumOrderVolume(ISecurity s);

        /// <summary>
        /// Returns the mimimum order step size (1.000 equals 1 microlot)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        int OrderVolumeStepSize(ISecurity s);

        /// <summary>
        /// Returns the maximum order size (1.000 equals 1 microlot)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        int MaximumOrderVolume(ISecurity s);

        /// <summary>
        /// Return the stop out level used, before giving a margin call
        /// </summary>
        /// <returns></returns>
        decimal StopOutLevel();

        #endregion Public Methods
    }
}