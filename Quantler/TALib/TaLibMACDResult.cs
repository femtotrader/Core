﻿#region License
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

using TicTacTec.TA.Library;

namespace Quantler.TALib
{
    /// <summary>
    /// Extended class for TA-Lib calculations MACD only
    /// </summary>
    public class TaLibMacdResult : TaLibResult
    {
        #region Internal Constructors

        internal TaLibMacdResult(double[] line, double[] signal, double[] histogram, Core.RetCode ret)
            : base(line, ret)
        {
            MacdHistogram = histogram;
            MacdLine = line;
            MacdSignal = signal;
        }

        #endregion Internal Constructors

        #region Public Properties

        public double[] MacdHistogram { get; private set; }

        //MACDLine = 0, MACDSignal = 1, MACDHistogram = 2
        public double[] MacdLine { get; private set; }

        public double[] MacdSignal { get; private set; }

        #endregion Public Properties
    }
}