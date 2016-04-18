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

namespace Quantler.Data.TikFile
{
    /// <summary>
    /// constants for working with Tik files
    /// </summary>
    public static class TikConst
    {
        #region Public Fields

        public const string DotExt = ".TIK";
        public const byte EndData = 3;
        public const byte EndTick = 2;
        public const string Ext = "TIK";
        public const int Filecurrentversion = 2;
        public const byte StartData = 1;
        public const byte TickAsk = 35;
        public const byte TickBid = 34;

        // end header, start ticks next tick coming no more ticks
        public const byte TickFull = 32;

        // quote and trade present
        public const byte TickQuote = 33;

        // bid and ask only bid only ask only
        public const byte TickTrade = 36;

        // file field identifiers
        public const byte Version = 0;

        public const string WildcardExt = "*.TIK";

        #endregion Public Fields
    }
}