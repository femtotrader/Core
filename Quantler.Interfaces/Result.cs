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

        /// <summary>
        /// Absolute dollar returns per trade 
        /// </summary>
        decimal[] DollarReturns { get; set; }

        /// <summary>
        /// number of break even trades
        /// </summary>
        int Flats { get; set; }

        /// <summary>
        /// Gross return averaged per day
        /// </summary>
        decimal GrossPerDay { get; set; }

        /// <summary>
        /// Gross return averaged per symbol
        /// </summary>
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
        /// Max drawdown (based on money in use)
        /// </summary>
        decimal MaxDD { get; set; }

        /// <summary>
        /// Max drawdown (based on initial capital)
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
        /// highest gross pl to achieve final result
        /// </summary>
        decimal MaxPL { get; set; }

        /// <summary>
        /// biggest winner gross
        /// </summary>
        decimal MaxWin { get; set; }

        /// <summary>
        /// lowest gross pl to achieve final result
        /// </summary>
        decimal MinPL { get; set; }

        /// <summary>
        /// total/max money used to achieve result
        /// </summary>
        decimal MoneyInUse { get; set; }

        /// <summary>
        /// Negative result in percentage (based on money in use)
        /// </summary>
        decimal[] NegPctReturns { get; }

        /// <summary>
        /// net pl of result
        /// </summary>
        decimal NetPL { get; }

        /// <summary>
        /// Percentage returns of past trades
        /// </summary>
        decimal[] PctReturns { get; }

        /// <summary>
        /// Negative percentage returns of past trades (based on initial capital)
        /// </summary>
        decimal[] PortfolioNegPctReturns { get; }
        
        /// <summary>
        /// Percentage resturns of past trades (based on initial capital)
        /// </summary>
        decimal[] PortfolioPctReturns { get; }

        /// <summary>
        /// Calculated profit factor
        /// </summary>
        decimal ProfitFactor { get; }

        /// <summary>
        /// date time in ticks
        /// </summary>
        long ResultsDateTime { get; set; }

        string ResultsId { get; set; }

        /// <summary>
        /// Return on investment (based on initial capital)
        /// </summary>
        decimal ROI { get; }

        /// <summary>
        /// Round turn losers
        /// </summary>
        int RoundLosers { get; set; }

        /// <summary>
        /// round turns
        /// </summary>
        int RoundTurns { get; set; }

        /// <summary>
        /// Rount turn winners
        /// </summary>
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

        /// <summary>
        /// Current sortino ratio
        /// </summary>
        decimal SortinoRatio { get; set; }

        /// <summary>
        /// Amount of symbols traded
        /// </summary>
        int SymbolCount { get; set; }

        /// <summary>
        /// symbols traded in result
        /// </summary>
        string Symbols { get; set; }

        /// <summary>
        /// Total amount of trades made
        /// </summary>
        int Trades { get; set; }

        /// <summary>
        /// number of winning trades
        /// </summary>
        int Winners { get; set; }

        #endregion Public Properties
    }
}