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
    /// Base class for TA-Lib calculations
    /// </summary>
    public class TaLibResult
    {
        #region Private Fields

        /// <summary>
        /// Private returncode
        /// </summary>
        private readonly Core.RetCode _ret;

        #endregion Private Fields

        #region Internal Constructors

        internal TaLibResult(double[] result, Core.RetCode ret)
        {
            _ret = ret;
            this.result = result;
        }

        #endregion Internal Constructors

        #region Public Properties

        /// <summary>
        /// Get the most recent value from the calculated set
        /// </summary>
        public double CurrentValue
        {
            get
            {
                if (IsValid)
                    return result[0];
                return 0;
            }
        }

        /// <summary>
        /// Check if the calculated values are valid
        /// </summary>
        public bool IsValid { get { return _ret == Core.RetCode.Success && result != null && result[0] != 0; } }

        /// <summary>
        /// Get the lenght of the returned result
        /// </summary>
        public int Length { get { return result.Length; } }

        #endregion Public Properties

        #region Protected Properties

        /// <summary>
        /// Get the array of returned values, most recent first = 0
        /// </summary>
        protected double[] result { get; private set; }

        #endregion Protected Properties
    }
}