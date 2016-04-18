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

namespace Quantler.Templates
{
    public abstract class StatisticTemplate : Template
    {
        public abstract double GetCurrentValue();

        public abstract bool IsReady();

        #region Public Methods

        public abstract double[] OnCalculate();

        #endregion Public Methods
    }
}