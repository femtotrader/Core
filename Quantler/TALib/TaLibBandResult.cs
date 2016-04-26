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
    public class TaLibBandResult : TaLibResult
    {
        #region Internal Constructors

        internal TaLibBandResult(double[] upperResult, double[] middleBand, double[] lowerResult, Core.RetCode ret, int index)
            : base(upperResult, ret, index)
        {
            UpperBand = upperResult;
            LowerBand = lowerResult;
            MiddleBand = middleBand;
        }

        #endregion Internal Constructors

        public double[] LowerBand { get; private set; }

        #region Public Properties

        public double[] MiddleBand { get; private set; }
        public double[] UpperBand { get; private set; }

        #endregion Public Properties
    }
}