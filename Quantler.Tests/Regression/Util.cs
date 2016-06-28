using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quantler.Interfaces;
using System.IO;
using Quantler.Interfaces;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json;
using Quantler.Backtester;

namespace Quantler.Tests.Regression
{
    internal static class Util
    {
        /// <summary>
        /// Tests an indicator by going through the values of the indicator and checking these values from a static file
        /// </summary>
        /// <param name="indicator"></param>
        /// <param name="filename"></param>
        /// <param name="targetColumns"></param>
        /// <param name="valuecheck"></param>
        public static BacktestResults BacktestStrategy(PortfolioManager portfolio, string filename, Action<BacktestResults, BacktestResults> valuecheck)
        {
            //Toreturn
            BacktestResults actualresults = new BacktestResults();
            BacktestResults expectedresults = new BacktestResults();

            //Run backtest
            SimpleBacktester backtester = new SimpleBacktester(portfolio, @"", 0);

            //Set values
            actualresults.Result = backtester.Results;
            actualresults.Orders = backtester.Orders;
            actualresults.Trades = backtester.Trades;

            //Check and get file
            FileInfo file = new FileInfo(Directory.GetCurrentDirectory() + @"\Backtests\ResultFiles\" + filename + ".csv");
            bool first = true;

            if (file.Exists)
            {
                expectedresults = JsonConvert.DeserializeObject<BacktestResults>("");
            }
            else
                throw new Exception("Could not get results file, file does not exist");

            //Check for realized and expected results
            valuecheck.Invoke(actualresults, expectedresults);

            return null;
        }

        /// <summary>
        /// Check all values of a backtest result that are correct within the tolerance that was given
        /// </summary>
        /// <param name="actual"></param>
        /// <param name="expected"></param>
        /// <param name="tolerance"></param>
        public static void CheckResults(Result actual, Result expected, decimal precision = 0)
        {
            //Comare decimal values using the aproximately precision given
            actual.AvgLoser.Should().BeApproximately(expected.AvgLoser, precision);
            actual.AvgPerTrade.Should().BeApproximately(expected.AvgPerTrade, precision);
            actual.AvgWin.Should().BeApproximately(expected.AvgWin, precision);
            actual.Balance.Should().BeApproximately(expected.Balance, precision);
            actual.BuyPL.Should().BeApproximately(expected.BuyPL, precision);
            actual.Commissions.Should().BeApproximately(expected.Commissions, precision);
            actual.GrossPerDay.Should().BeApproximately(expected.GrossPerDay, precision);
            actual.GrossPerSymbol.Should().BeApproximately(expected.GrossPerSymbol, precision);
            actual.GrossPL.Should().BeApproximately(expected.GrossPL, precision);
            actual.InitialCapital.Should().BeApproximately(expected.InitialCapital, precision);
            actual.MaxDD.Should().BeApproximately(expected.MaxDD, precision);
            actual.MaxDDPortfolio.Should().BeApproximately(expected.MaxDDPortfolio, precision);
            actual.MaxLoss.Should().BeApproximately(expected.MaxLoss, precision);
            actual.MaxOpenLoss.Should().BeApproximately(expected.MaxOpenLoss, precision);
            actual.MaxOpenWin.Should().BeApproximately(expected.MaxOpenWin, precision);
            actual.MaxPL.Should().BeApproximately(expected.MaxPL, precision);
            actual.MaxWin.Should().BeApproximately(expected.MaxWin, precision);
            actual.MinPL.Should().BeApproximately(expected.MinPL, precision);
            actual.MoneyInUse.Should().BeApproximately(expected.MoneyInUse, precision);
            actual.NetPL.Should().BeApproximately(expected.NetPL, precision);
            actual.ProfitFactor.Should().BeApproximately(expected.ProfitFactor, precision);
            actual.ROI.Should().BeApproximately(expected.ROI, precision);
            actual.SellPL.Should().BeApproximately(expected.SellPL, precision);
            actual.SharpeRatio.Should().BeApproximately(expected.SharpeRatio, precision);
            actual.SortinoRatio.Should().BeApproximately(expected.SortinoRatio, precision);

            //Compare integer values
            actual.BuyLosers.Should().Be(expected.BuyLosers);
            actual.BuyWins.Should().Be(expected.BuyWins);
            actual.ConsecLose.Should().Be(expected.ConsecLose);
            actual.ConsecWin.Should().Be(expected.ConsecWin);
            actual.DaysTraded.Should().Be(expected.DaysTraded);
            actual.Flats.Should().Be(expected.Flats);
            actual.HundredLots.Should().Be(expected.HundredLots);
            actual.Losers.Should().Be(expected.Losers);
            actual.RoundLosers.Should().Be(expected.RoundLosers);
            actual.RoundTurns.Should().Be(expected.RoundTurns);
            actual.RoundWinners.Should().Be(expected.RoundWinners);
            actual.SellLosers.Should().Be(expected.SellLosers);
            actual.SellWins.Should().Be(expected.SellWins);
            actual.SharesTraded.Should().Be(expected.SharesTraded);
            actual.SymbolCount.Should().Be(expected.SymbolCount);
            actual.Trades.Should().Be(expected.Trades);
            actual.Winners.Should().Be(expected.Winners);
        }
    }
}
