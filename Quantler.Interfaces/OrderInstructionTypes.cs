#region License
/*
Copyright Quantler BV, based on original code copyright Tradelink.org. 
This file is released under the GNU Lesser General Public License v3. http://www.gnu.org/copyleft/lgpl.html


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
    /// list of accepted special order instructions
    /// </summary>
    public enum OrderInstructionType
    {
        Invalid = -2,

        None = -1,

        /// <summary>
        /// day order
        /// </summary>
        DAY = 0,

        /// <summary>
        /// good till canceled
        /// </summary>
        GTC = 1,

        /// <summary>
        /// market on close
        /// </summary>
        MOC = 2,

        /// <summary>
        /// opening order
        /// </summary>
        OPG = 4,

        /// <summary>
        /// immediate or cancel
        /// </summary>
        IOC = 8,

        /// <summary>
        /// hidden
        /// </summary>
        HIDDEN = 512,
    }
}