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

namespace Quantler
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class Parameter : Attribute
    {
        #region Private Fields

        private int _parametervalue;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initialize an empty parameter
        /// </summary>
        public Parameter()
        {
        }

        /// <summary>
        /// Initialze a Parameter with settings
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="inc"></param>
        /// <param name="name"></param>
        public Parameter(int min, int max, int inc, string name)
        {
            ParameterMax = Math.Abs(max);
            ParameterMin = Math.Abs(min);
            ParameterName = name;
            ParameterInc = Math.Abs(inc);
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Some additional information about this parameter
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Returns the parameter ID
        /// </summary>
        public int ParameterId { get; set; }

        /// <summary>
        /// Returns the increment of this parameter
        /// </summary>
        public int ParameterInc { get; set; }

        /// <summary>
        /// Returns the max value of this parameter that can be set
        /// </summary>
        public int ParameterMax { get; set; }

        /// <summary>
        /// Returns the min value of this parameter that can be set
        /// </summary>
        public int ParameterMin { get; set; }

        /// <summary>
        /// Returns the name of this parameter
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// Returns or sets the value for this parameter
        /// </summary>
        public int ParameterValue { get { return _parametervalue; } set { _parametervalue = value > ParameterMax ? ParameterMax : value; } }

        #endregion Public Properties
    }
}