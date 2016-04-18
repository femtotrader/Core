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
    /// Extended class for TA-Lib calculations DMI only
    /// </summary>
    public class TaLibDmiResult : TaLibResult
    {
        #region Internal Constructors

        internal TaLibDmiResult(double[] plusresult, double[] minresult, Core.RetCode ret)
            : base(plusresult, ret)
        {
            PlusDmi = plusresult;
            MinDmi = minresult;
        }

        #endregion Internal Constructors

        #region Public Properties

        public double[] MinDmi { get; private set; }

        public double[] PlusDmi { get; private set; }

        #endregion Public Properties
    }
}