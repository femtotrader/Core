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
    /// Ordering of fields in SENDORDER and ORDERNOTIFY messages
    /// </summary>
    public enum OrderField
    {
        Symbol = 0,
        Side,
        Size,
        Price,
        Stop,
        Comment,
        Exchange,
        Account,
        Security,
        Currency,
        LocalSymbol, // non-pretty symbol or contract symbol for futures
        OrderID,
        OrderTIF,
        oDate,
        oTime,
        oUNUSED,
        Trail,
    }
}