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

namespace Quantler.Templates
{
    public abstract class BrokerModelTemplate : Template, BrokerModel
    {
        #region Public Methods

        public virtual decimal CalculateMarginInterest(Order o)
        {
            return 0;
        }

        public virtual decimal GetCommission(Order o)
        {
            return 0;
        }

        public virtual int GetLatencyInMilliseconds(Order o)
        {
            return 0;
        }

        public virtual decimal GetSlippage(Order o)
        {
            return 0;
        }

        public virtual decimal GetSpread(Order o)
        {
            return 0;
        }

        public int MinimumOrderVolume(ISecurity s)
        {
            return 1000;
        }

        public int OrderVolumeStepSize(ISecurity s)
        {
            return 1000;
        }

        public int MaximumOrderVolume(ISecurity s)
        {
            return 1000 * 1000;
        }

        public virtual decimal StopOutLevel()
        {
            return 20;
        }

        #endregion Public Methods
    }
}