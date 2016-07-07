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

namespace Quantler.Interfaces
{
    public interface Result
    {
        #region Public Properties

        /// <summary>
        /// average gross per losing trade
        /// </summary>
        decimal AvgLoser { get; }

        /// <summary>
        /// average gross pl per trade
        /// </summary>
        decimal AvgPerTrade { get; }

        /// <summary>
        /// average gross per win trade
        /// </summary>
        decimal AvgWin { get; }

        /// <summary>
        /// Initial account balance for results
        /// </summary>
        decimal Balance { get; }

        /// <summary>
        /// number of buy losers
        /// </summary>
        int BuyLosers { get; }

        /// <summary>
        /// long trade pl
        /// </summary>
        decimal BuyPL { get; }

        /// <summary>
        /// number of long winners
        /// </summary>
        int BuyWins { get; }

        /// <summary>
        /// Total amount of commissions paid
        /// </summary>
        decimal Commissions { get; }

        /// <summary>
        /// Total amount consecutive losses
        /// </summary>
        int ConsecLose { get; }

        /// <summary>
        /// Total amount of consecutive wins
        /// </summary>
        int ConsecWin { get; }

        /// <summary>
        /// Amount of days traded (tades made not hold)
        /// </summary>
        int DaysTraded { get; }

        /// <summary>
        /// Absolute dollar returns per trade
        /// </summary>
        decimal[] DollarReturns { get; }

        /// <summary>
        /// number of break even trades
        /// </summary>
        int Flats { get; }

        /// <summary>
        /// Gross return averaged per day
        /// </summary>
        decimal GrossPerDay { get; }

        /// <summary>
        /// Gross return averaged per symbol
        /// </summary>
        decimal GrossPerSymbol { get; }

        /// <summary>
        /// gross pl of result
        /// </summary>
        decimal GrossPL { get; }

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
        int Losers { get; }

        /// <summary>
        /// Total amount of margin paid
        /// </summary>
        decimal MarginInterest { get; }

        /// <summary>
        /// Max drawdown (based on money in use)
        /// </summary>
        decimal MaxDD { get; }

        /// <summary>
        /// Max drawdown (based on initial capital)
        /// </summary>
        decimal MaxDDPortfolio { get; }

        /// <summary>
        /// biggest loser gross
        /// </summary>
        decimal MaxLoss { get; }

        /// <summary>
        /// max unclosed losing gross
        /// </summary>
        decimal MaxOpenLoss { get; }

        /// <summary>
        /// max unclosed winning gross
        /// </summary>
        decimal MaxOpenWin { get; }

        /// <summary>
        /// highest gross pl to achieve final result
        /// </summary>
        decimal MaxPL { get; }

        /// <summary>
        /// biggest winner gross
        /// </summary>
        decimal MaxWin { get; }

        /// <summary>
        /// lowest gross pl to achieve final result
        /// </summary>
        decimal MinPL { get; }

        /// <summary>
        /// total/max money used to achieve result
        /// </summary>
        decimal MoneyInUse { get; }

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
        long ResultsDateTime { get; }

        string ResultsId { get; }

        /// <summary>
        /// Return on investment (based on initial capital)
        /// </summary>
        decimal ROI { get; }

        /// <summary>
        /// Round turn losers
        /// </summary>
        int RoundLosers { get; }

        /// <summary>
        /// round turns
        /// </summary>
        int RoundTurns { get; }

        /// <summary>
        /// Rount turn winners
        /// </summary>
        int RoundWinners { get; }

        /// <summary>
        /// number of short losers
        /// </summary>
        int SellLosers { get; }

        /// <summary>
        /// short trade pl
        /// </summary>
        decimal SellPL { get; }

        /// <summary>
        /// number of short winners
        /// </summary>
        int SellWins { get; }

        /// <summary>
        /// shares/contracts traded during result
        /// </summary>
        int SharesTraded { get; }

        /// <summary>
        /// Current sharpe ratio
        /// </summary>
        decimal SharpeRatio { get; }

        /// <summary>
        /// Current sortino ratio
        /// </summary>
        decimal SortinoRatio { get; }

        /// <summary>
        /// Amount of symbols traded
        /// </summary>
        int SymbolCount { get; }

        /// <summary>
        /// symbols traded in result
        /// </summary>
        string Symbols { get; }

        /// <summary>
        /// Total amount of trades made
        /// </summary>
        int Trades { get; }

        /// <summary>
        /// number of winning trades
        /// </summary>
        int Winners { get; }

        #endregion Public Properties
    }
}