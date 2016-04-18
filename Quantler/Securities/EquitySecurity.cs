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

using System;
using Quantler.Interfaces;

namespace Quantler.Securities
{
    /// <summary>
    /// Equity Security Implementation
    /// </summary>
    public class EquitySecurity : ISecurity
    {
        #region Public Constructors

        public EquitySecurity(string symbol)
        {
            BrokerName = symbol;
            DestEx = "SIM";
            Name = symbol;
            Type = SecurityType.Equity;
            LotSize = 1;
            PipSize = 0.01M;
            OrderStepSize = 1;
            OrderMinSize = 1;
            TickSize = PipSize;
            PipValue = PipSize;
        }

        #endregion Public Constructors

        #region Public Properties

        public decimal Ask
        {
            get;
            set;
        }

        public decimal Bid
        {
            get;
            set;
        }

        public string BrokerName
        {
            get;
            private set;
        }

        public int Date
        {
            get;
            set;
        }

        public string DestEx
        {
            get;
            set;
        }

        public string Details
        {
            get;
            set;
        }

        public int Digits
        {
            get;
            set;
        }

        public bool HasDest
        {
            get { return true; }
        }

        public bool HasType
        {
            get { return true; }
        }

        public bool IsFloatingSpread
        {
            get;
            set;
        }

        public bool IsValid
        {
            get { return true; }
        }

        public DateTime LastTickEvent
        {
            get;
            set;
        }

        public int LotSize
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public decimal OrderMinQuantity
        {
            get { return OrderMinSize / (decimal)LotSize; }
        }

        public int OrderMinSize
        {
            get;
            set;
        }

        public decimal OrderStepQuantity
        {
            get { return OrderStepSize / (decimal)LotSize; }
        }

        public int OrderStepSize
        {
            get;
            set;
        }

        public decimal PipSize
        {
            get;
            set;
        }

        public decimal PipValue
        {
            get;
            set;
        }

        public int Spread
        {
            get;
            set;
        }

        public decimal TickSize
        {
            get;
            set;
        }

        public SecurityType Type
        {
            get;
            set;
        }

        #endregion Public Properties
    }
}
