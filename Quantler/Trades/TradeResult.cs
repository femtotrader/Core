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

using Quantler.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Quantler.Trades
{
    public class TradeResult : TradeImpl
    {
        #region Public Fields

        public decimal AvgPrice;

        public decimal ClosedPl;

        public int ClosedSize;

        public decimal OpenPl;

        public int OpenSize;

        public Trade Source;

        #endregion Public Fields

        #region Private Fields

        private const int S = 7;

        #endregion Private Fields

        #region Public Methods

        // we're reading these values from file, bc it's faster than recalculating each time
        public static TradeResult Init(string resultline)
        {
            string[] res = resultline.Split(',');
            TradeResult r = new TradeResult { Source = FromString(resultline) };
            if (res[S] != string.Empty || res[S] != "")
                r.OpenPl = Convert.ToDecimal(res[S], CultureInfo.InvariantCulture);
            if (res[S + 1] != string.Empty || res[S + 1] != "")
                r.ClosedPl = Convert.ToDecimal(res[S + 1], CultureInfo.InvariantCulture);
            if (res[S + 2] != string.Empty || res[S + 2] != "")
                r.OpenSize = Convert.ToInt32(res[S + 2], CultureInfo.InvariantCulture);
            if (res[S + 3] != string.Empty || res[S + 3] != "")
                r.ClosedSize = Convert.ToInt32(res[S + 3], CultureInfo.InvariantCulture);
            if (res[S + 4] != string.Empty || res[S + 4] != "")
                r.AvgPrice = Convert.ToDecimal(res[S + 4], CultureInfo.InvariantCulture);
            return r;
        }

        public static TradeResult Init(Position pos, Trade fill, decimal pnL)
        {
            TradeResult toreturn = new TradeResult { ClosedPl = pnL };
            toreturn.ClosedPl += Calc.OpenPL(fill.Xprice, pos);
            if (toreturn.ClosedPl != 0) toreturn.ClosedSize = fill.Xsize;
            toreturn.AvgPrice = pos.AvgPrice;
            toreturn.Source = fill;
            toreturn.Commission = fill.Commission;

            return toreturn;
        }

        public static List<TradeResult> ResultsFromTradeList(List<Trade> trades)
        {
            string[] results = Util.TradesToClosedPL(trades);
            List<TradeResult> tresults = new List<TradeResult>(results.Length);
            tresults.AddRange(results.Select(Init));
            return tresults;
        }

        public override string ToString()
        {
            return Source + " cpl: " + ClosedPl.ToString("F2");
        }

        #endregion Public Methods
    }
}