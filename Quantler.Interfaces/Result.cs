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

using System.Collections.Generic;

namespace Quantler.Interfaces
{
    public interface Result
    {
        #region Public Properties

        /// <summary>
        /// average gross per losing trade
        /// </summary>
        decimal AvgLoser { get; set; }

        /// <summary>
        /// average gross pl per trade
        /// </summary>
        decimal AvgPerTrade { get; set; }

        /// <summary>
        /// average gross per win trade
        /// </summary>
        decimal AvgWin { get; set; }

        /// <summary>
        /// Initial account balance for results
        /// </summary>
        decimal Balance { get; }

        /// <summary>
        /// number of buy losers
        /// </summary>
        int BuyLosers { get; set; }

        /// <summary>
        /// long trade pl
        /// </summary>
        decimal BuyPL { get; set; }

        /// <summary>
        /// number of long winners
        /// </summary>
        int BuyWins { get; set; }

        /// <summary>
        /// Total amount of commissions paid
        /// </summary>
        decimal Commissions { get; }

        /// <summary>
        /// Total amount consecutive losses
        /// </summary>
        int ConsecLose { get; set; }

        /// <summary>
        /// Total amount of consecutive wins
        /// </summary>
        int ConsecWin { get; set; }

        /// <summary>
        /// Amount of days traded (tades made not hold)
        /// </summary>
        int DaysTraded { get; set; }

        decimal[] DollarReturns { get; set; }

        /// <summary>
        /// number of break even trades
        /// </summary>
        int Flats { get; set; }

        decimal GrossPerDay { get; set; }

        decimal GrossPerSymbol { get; set; }

        /// <summary>
        /// gross pl of result
        /// </summary>
        decimal GrossPL { get; set; }

        /// <summary>
        /// Amount of lots traded
        /// </summary>
        int HundredLots { get; }

        /// <summary>
        /// Initial trading capital
        /// </summary>
        decimal InitialCapital { get; }

        /// <summary>
        /// number of total losers
        /// </summary>
        int Losers { get; set; }

        /// <summary>
        /// Max drawdown based on money in use
        /// </summary>
        decimal MaxDD { get; set; }

        /// <summary>
        /// Max drawdown absed on equity
        /// </summary>
        decimal MaxDDPortfolio { get; }

        /// <summary>
        /// biggest loser gross
        /// </summary>
        decimal MaxLoss { get; set; }

        /// <summary>
        /// max unclosed losing gross
        /// </summary>
        decimal MaxOpenLoss { get; set; }

        /// <summary>
        /// max unclosed winning gross
        /// </summary>
        decimal MaxOpenWin { get; set; }

        /// <summary>
        /// highest gross pl to acheive final result
        /// </summary>
        decimal MaxPL { get; set; }

        /// <summary>
        /// biggest winner gross
        /// </summary>
        decimal MaxWin { get; set; }

        /// <summary>
        /// lowest gross pl to acheive final result
        /// </summary>
        decimal MinPL { get; set; }

        /// <summary>
        /// total/max money used to acheive result
        /// </summary>
        decimal MoneyInUse { get; set; }

        decimal[] NegPctReturns { get; }

        /// <summary>
        /// net pl of result
        /// </summary>
        decimal NetPL { get; }

        decimal[] PctReturns { get; }
        List<string> PerSymbolStats { get; set; }
        decimal[] PortfolioNegPctReturns { get; }
        decimal[] PortfolioPctReturns { get; }
        decimal ProfitFactor { get; }

        /// <summary>
        /// date time in ticks
        /// </summary>
        long ResultsDateTime { get; set; }

        string ResultsId { get; set; }

        decimal ROI { get; }
        int RoundLosers { get; set; }

        /// <summary>
        /// round turns
        /// </summary>
        int RoundTurns { get; set; }

        int RoundWinners { get; set; }

        /// <summary>
        /// number of short losers
        /// </summary>
        int SellLosers { get; set; }

        /// <summary>
        /// short trade pl
        /// </summary>
        decimal SellPL { get; set; }

        /// <summary>
        /// number of short winners
        /// </summary>
        int SellWins { get; set; }

        /// <summary>
        /// shares/contracts traded during result
        /// </summary>
        int SharesTraded { get; set; }

        /// <summary>
        /// Current sharpe ratio
        /// </summary>
        decimal SharpeRatio { get; set; }

        string SimParameters { get; set; }

        /// <summary>
        /// Current sortino ratio
        /// </summary>
        decimal SortinoRatio { get; set; }

        int SymbolCount { get; set; }

        /// <summary>
        /// symbols traded in result
        /// </summary>
        string Symbols { get; set; }

        int Trades { get; set; }

        /// <summary>
        /// number of winning trades
        /// </summary>
        int Winners { get; set; }

        #endregion Public Properties
    }
}