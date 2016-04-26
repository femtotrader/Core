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

using TicTacTec.TA.Library;

namespace Quantler.TALib
{
    /// <summary>
    /// Extended class for TA-Lib calculations STOCH only
    /// </summary>
    public class TaLibStochResult : TaLibResult
    {
        #region Internal Constructors

        internal TaLibStochResult(double[] kResult, double[] dResult, Core.RetCode ret, int index)
            : base(kResult, ret, index)
        {
            KResult = kResult;
            DResult = dResult;
        }

        #endregion Internal Constructors

        #region Public Properties

        public double[] DResult { get; private set; }

        public double[] KResult { get; private set; }

        #endregion Public Properties
    }
}