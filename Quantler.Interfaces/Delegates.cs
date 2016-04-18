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

using System;

[assembly: CLSCompliant(true)]

namespace Quantler.Interfaces
{
    public delegate void ChartUpdate(ITradingAgent agent, ITemplate template, string Name, decimal Value, ChartType chart);

    public delegate void FillDelegate(Trade t, PendingOrder o);

    public delegate void LongSourceDelegate(long val, int source);

    public delegate void MessageDelegate(StatusType type, long source, long dest, long msgid, string request, ref string response);

    public delegate void OrderCancelDelegate(PendingOrder o);

    public delegate void OrderDelegate(PendingOrder o);

    public delegate void OrderSourceDelegate(PendingOrder o, int source);

    public delegate void OrderUpdateDelegate(PendingOrder o);

    public delegate void PositionUpdateDelegate(Position pos, Trade fill, decimal PnL);

    public delegate void ResponseStringDel(int idx, string data);

    public delegate void ResultDel(Result r);

    public delegate void ResultListDel(System.Collections.Generic.List<Result> rs);

    public delegate void SecurityDelegate(ISecurity sec);

    public delegate void SymBarIntervalDelegate(string symbol, int interval);

    public delegate void TextIdxDelegate(string txt, int idx);

    public delegate void TickDelegate(Tick t);

    public delegate void VoidDelegate();
}