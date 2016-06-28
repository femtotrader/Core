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
using Quantler.Tracker;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantler.Trades
{
    /// <summary>
    /// track results
    /// </summary>
    public class Results : Result
    {
        #region Private Fields

        private readonly int _agentid = -1;
        private readonly List<int> _days = new List<int>();
        private readonly List<long> _exitscounted = new List<long>();
        private readonly List<Trade> _fills = new List<Trade>();
        private readonly List<decimal> _grossreturn = new List<decimal>();
        private readonly bool _islive = false;
        private readonly List<decimal> _miu = new List<decimal>();
        private readonly List<decimal> _negret = new List<decimal>();
        private readonly List<decimal> _netreturns = new List<decimal>();
        private readonly List<decimal> _pctrets = new List<decimal>();
        private readonly List<decimal> _portfNegpctreturns = new List<decimal>();
        private readonly List<decimal> _portfPctreturns = new List<decimal>();
        private readonly IPositionTracker _positions;
        private readonly Dictionary<string, int> _tradecount = new Dictionary<string, int>();
        private readonly List<TradeResult> _traderesults = new List<TradeResult>();
        private int _consecLosers;
        private int _consecWinners;
        private int _livecheckafterXticks = 1;
        private int _livetickdelaymax = 60;
        private decimal _losepnl;
        private Position _prevPos;
        private string _resultid = string.Empty;

        private decimal _rfr = .01m;
        private string _simparam = string.Empty;

        private string _syms = string.Empty;
        private decimal _winpnl;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// create default results instance
        /// </summary>
        public Results()
            : this(.01m, null)
        {
        }

        /// <summary>
        /// create results instance with risk free return, comission and report time
        /// </summary>
        /// <param name="rfr"></param>
        /// <param name="account"></param>
        public Results(decimal rfr, IAccount account)
        {
            RiskFreeRate = rfr;

            //Subscribe for updates (updated during trading)
            if (account == null) return;
            _positions = account.Positions;
            _positions.OnPositionUpdate += OnPositionUpdate;

            //Set initial values
            Balance = account.Balance;
            InitialCapital = account.Balance;
        }

        /// <summary>
        /// Create a results object that will create results based on trades performed for a specific agent
        /// </summary>
        /// <param name="rfr"></param>
        /// <param name="account"></param>
        /// <param name="agentid"></param>
        public Results(decimal rfr, IAccount account, int agentid)
            : this(rfr, account)
        {
            _agentid = agentid;
        }

        #endregion Public Constructors

        #region Public Properties

        public decimal AvgLoser { get; set; }

        public decimal AvgPerTrade { get; set; }

        public decimal AvgWin { get; set; }

        public decimal Balance { get; private set; }

        public int BuyLosers { get; set; }

        public decimal BuyPL { get; set; }

        public int BuyWins { get; set; }

        /// <summary>
        /// wait to do live test after X ticks have arrived
        /// </summary>
        public int CheckLiveAfterTickCount { get { return _livecheckafterXticks; } set { _livecheckafterXticks = value; } }

        /// <summary>
        /// if a tick is within this many seconds of current system time on same day, tick stream is
        /// considered live and reports can be sent
        /// </summary>
        public int CheckLiveMaxDelaySec { get { return _livetickdelaymax; } set { _livetickdelaymax = value; } }

        public decimal Commissions { get; set; }

        public int ConsecLose { get; set; }

        public int ConsecWin { get; set; }

        public int DaysTraded { get; set; }

        public decimal[] DollarReturns { get; set; }

        public int Flats { get; set; }

        public string GrossMargin { get { return V2S(GrossPL == 0 ? 0 : (GrossPL - Commissions) / GrossPL); } }

        public decimal GrossPerDay { get; set; }

        public decimal GrossPerSymbol { get; set; }

        public decimal GrossPL { get; set; }

        public int HundredLots { get { return (int)Math.Round((double)SharesTraded / 100, 0); } }

        public decimal InitialCapital { get; private set; }

        public bool IsLive { get { return _islive; } }

        public int Losers { get; set; }

        public string LoseSeqProbEffHyp { get { return V2S(Math.Min(100, (decimal)Math.Pow(1 / 2.0, ConsecLose) * (Trades - Flats - ConsecLose + 1) * 100)) + @" %"; } }

        public decimal MaxDD { get; set; }

        public string MaxDdNice { get { return MaxDD.ToString("P2"); } }

        public decimal MaxDDPortfolio { get; private set; }

        public string MaxDdPortfolioNice { get { return MaxDDPortfolio.ToString("P2"); } }

        public decimal MaxLoss { get; set; }

        public decimal MaxOpenLoss { get; set; }

        public decimal MaxOpenWin { get; set; }

        public decimal MaxPL { get; set; }

        public decimal MaxWin { get; set; }

        public decimal MinPL { get; set; }

        public decimal MoneyInUse { get; set; }

        public decimal[] NegPctReturns { get; private set; }

        public decimal NetPL { get { return GrossPL - Commissions; } }

        public string NetPlNice { get { return V2S(NetPL); } }

        public decimal[] PctReturns { get; private set; }

        public decimal[] PortfolioNegPctReturns { get; private set; }

        public decimal[] PortfolioPctReturns { get; private set; }

        public decimal ProfitFactor { get; private set; }

        public long ResultsDateTime { get; set; }

        public string ResultsId { get { if (string.IsNullOrWhiteSpace(_resultid)) return ResultsDateTime + "/" + NetPlNice + "/" + SimParameters; return _resultid; } set { _resultid = value; } }

        public string RiskFreeRet { get { return RiskFreeRate.ToString("P2"); } }

        public decimal ROI { get { return Balance / InitialCapital - 1; } }

        public int RoundLosers { get; set; }

        public int RoundTurns { get; set; }

        public int RoundWinners { get; set; }

        public int SellLosers { get; set; }

        public decimal SellPL { get; set; }

        public int SellWins { get; set; }

        public int SharesTraded { get; set; }

        public decimal SharpeRatio { get; set; }

        public string SimParameters { get { return _simparam; } set { _simparam = value; } }

        public decimal SortinoRatio { get; set; }

        public int SymbolCount { get; set; }

        public string Symbols { get { return _syms; } set { _syms = value; } }

        public int Trades { get; set; }

        public int Winners { get; set; }

        public string WinSeqProbEffHyp { get { return V2S(Math.Min(100, (decimal)Math.Pow(1 / 2.0, ConsecWin) * (Trades - Flats - ConsecWin + 1) * 100)) + @" %"; } }

        public string WlRatio { get { return V2S(Losers == 0 ? 0 : Winners / (decimal)Losers); } }

        #endregion Public Properties

        #region Private Properties

        private decimal RiskFreeRate { get { return _rfr; } set { _rfr = value; } }

        #endregion Private Properties

        #region Private Methods

        private void OnPositionUpdate(Position pos, Trade fill, decimal pnL)
        {
            //Check if we may only process specific agent based trades
            if (_agentid > 0 && _agentid != fill.AgentId)
                return;

            //Process new results
            _fills.Add(fill);
            _traderesults.Add(TradeResult.Init(pos, fill, pnL));
            ProcessTrade(_traderesults.Last(), pos, pnL);
            ProcessGeneral();
        }

        private void ProcessGeneral()
        {
            try
            {
                PctReturns = _pctrets.ToArray();
                DollarReturns = _netreturns.ToArray();
                NegPctReturns = _negret.ToArray();
                PortfolioPctReturns = _portfPctreturns.ToArray();
                PortfolioNegPctReturns = _portfNegpctreturns.ToArray();

                if (Trades != 0)
                {
                    AvgPerTrade = Math.Round((_losepnl + _winpnl) / Trades, 2);
                    AvgLoser = Losers == 0 ? 0 : Math.Round(_losepnl / Losers, 2);
                    AvgWin = Winners == 0 ? 0 : Math.Round(_winpnl / Winners, 2);
                    MoneyInUse = Math.Round(Calc.Max(_miu.ToArray()), 2);
                    MaxPL = Math.Round(Calc.Max(_grossreturn.ToArray()), 2);
                    MinPL = Math.Round(Calc.Min(_grossreturn.ToArray()), 2);

                    //Check for max dd new value
                    var value = Math.Round(Calc.MaxDDPct(PctReturns), 5);
                    MaxDD = MaxDD < value ? MaxDD : value;
                    value = Math.Round(Calc.MaxDDPct(PortfolioPctReturns), 5);
                    MaxDDPortfolio = MaxDDPortfolio < value ? MaxDDPortfolio : value;

                    SymbolCount = ((PositionTracker)_positions).Count;
                    DaysTraded = _days.Count;
                    GrossPerDay = Math.Round(GrossPL / DaysTraded, 2);
                    GrossPerSymbol = Math.Round(GrossPL / SymbolCount, 2);

                    var losses = Math.Abs(_portfPctreturns.Where(x => x < 0).Sum());
                    if (losses > 0)
                        ProfitFactor = Math.Round(_portfPctreturns.Where(x => x >= 0).Sum() / losses, 3);
                }

                try
                {
                    var fret = Calc.Avg(PortfolioPctReturns);

                    if (PortfolioPctReturns.Length == 0)
                        SharpeRatio = 0;
                    else if (PortfolioPctReturns.Length == 1)
                        SharpeRatio = Math.Round(fret - RiskFreeRate, 3);
                    else
                        SharpeRatio = Math.Round(Calc.SharpeRatio(fret, Calc.StdDev(PortfolioPctReturns.Where(x => x != 0).ToArray()), RiskFreeRate), 3);
                }
                catch
                {
                    // ignored
                }

                try
                {
                    var fret = Calc.Avg(PortfolioPctReturns);
                    if (PortfolioPctReturns.Length == 0)
                        SortinoRatio = 0;
                    else if (PortfolioNegPctReturns.Length == 1)
                        SortinoRatio = (fret - RiskFreeRate) / PortfolioNegPctReturns[0];
                    else if (PortfolioNegPctReturns.Length == 0)
                        SortinoRatio = fret - RiskFreeRate;
                    else
                        SortinoRatio = Math.Round(Calc.SortinoRatio(fret, Calc.StdDev(PortfolioNegPctReturns), RiskFreeRate), 3);
                }
                catch
                {
                    // ignored
                }
            }
            catch
            {
                // ignored
            }
        }

        private void ProcessTrade(TradeResult tr, Position pos, decimal pospl)
        {
            //Compare to prevpos
            bool initial = false;
            if (_prevPos == null)
            {
                _prevPos = pos;
                initial = true;
            }

            if (_tradecount.ContainsKey(tr.Source.Symbol))
                _tradecount[tr.Source.Symbol]++;
            else
                _tradecount.Add(tr.Source.Symbol, 1);
            if (!_days.Contains(tr.Source.Xdate))
                _days.Add(tr.Source.Xdate);

            int usizebefore = _prevPos.UnsignedSize;
            var miubefore = _prevPos.IsFlat ? 0 : _prevPos.UnsignedSize * _prevPos.AvgPrice;

            //Use new position from here
            bool isroundturn = (usizebefore > 0) && pospl != 0 && !initial && (pos.Direction != _prevPos.Direction);

            // get comissions
            Commissions += tr.Commission;

            // calculate MIU and store on array
            var miu = pos.IsFlat ? 0 : pos.AvgPrice * pos.UnsignedSize;
            if (miu != 0)
                _miu.Add(miu);

            // if we closed something, update return
            decimal grosspl = pospl;
            decimal netpl = grosspl - tr.Commission;

            // count return
            _grossreturn.Add(grosspl);
            _netreturns.Add(netpl);
            // get pct return for portfolio
            decimal pctret;
            if (miubefore == 0)
                pctret = netpl / miu;
            else
                pctret = netpl / miubefore;
            _pctrets.Add(pctret);

            //adjust initial capital based balance
            decimal portfolioreturn = netpl / InitialCapital;
            _portfPctreturns.Add(portfolioreturn);

            // add to neg returns if negative
            if (portfolioreturn < 0)
                _portfNegpctreturns.Add(portfolioreturn);

            // adjust current balance
            Balance += netpl;

            // if it is below our zero, count it as negative return
            if (pctret < 0)
                _negret.Add(pctret);

            if (isroundturn)
            {
                RoundTurns++;
                if (pospl >= 0)
                    RoundWinners++;
                else if (pospl < 0)
                    RoundLosers++;
            }

            if (!Symbols.Contains(tr.Source.Symbol))
                Symbols += tr.Source.Symbol + ",";
            Trades++;
            SharesTraded += Math.Abs(tr.Source.Xsize / tr.Source.Security.LotSize);
            GrossPL += tr.ClosedPl;

            if ((tr.ClosedPl > 0) && !_exitscounted.Contains(tr.Source.Id))
            {
                if (tr.Source.Direction == Direction.Long)
                {
                    SellWins++;
                    SellPL += tr.ClosedPl;
                }
                else
                {
                    BuyWins++;
                    BuyPL += tr.ClosedPl;
                }
                if (tr.Source.Id != 0)
                    _exitscounted.Add(tr.Id);
                Winners++;
                _consecWinners++;
                _consecLosers = 0;
            }
            else if ((tr.ClosedPl < 0) && !_exitscounted.Contains(tr.Source.Id))
            {
                if (tr.Source.Direction == Direction.Long)
                {
                    SellLosers++;
                    SellPL += tr.ClosedPl;
                }
                else
                {
                    BuyLosers++;
                    BuyPL += tr.ClosedPl;
                }
                if (tr.Source.Id != 0)
                    _exitscounted.Add(tr.Id);
                Losers++;
                _consecLosers++;
                _consecWinners = 0;
            }
            if (tr.ClosedPl > 0)
                _winpnl += tr.ClosedPl;
            else if (tr.ClosedPl < 0)
                _losepnl += tr.ClosedPl;

            if (_consecWinners > ConsecWin) ConsecWin = _consecWinners;
            if (_consecLosers > ConsecLose) ConsecLose = _consecLosers;
            if ((tr.OpenSize == 0) && (tr.ClosedPl == 0)) Flats++;
            if (tr.ClosedPl > MaxWin) MaxWin = tr.ClosedPl;
            if (tr.ClosedPl < MaxLoss) MaxLoss = tr.ClosedPl;
            if (tr.OpenPl > MaxOpenWin) MaxOpenWin = tr.OpenPl;
            if (tr.OpenPl < MaxOpenLoss) MaxOpenLoss = tr.OpenPl;

            //set prev position
            _prevPos = pos;
        }

        private string V2S(decimal v)
        {
            return v.ToString("N2");
        }

        #endregion Private Methods
    }
}