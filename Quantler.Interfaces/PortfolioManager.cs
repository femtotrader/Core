#region License
/*
Copyright (c) Quantler B.V., All rights reserved.

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
    public interface PortfolioManager : IPortfolio
    {
        #region Public Events

        /// <summary>
        /// Event is fired when a new order object is sent from the portfolio (one of its agents)
        /// </summary>
        event OrderSourceDelegate SendOrderEvent;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Factory which creates new orders based on the request of the trading agent
        /// </summary>
        OrderFactory OrderFactory { get; set; }

        /// <summary>
        /// All trading agents associated in this portoflio
        /// </summary>
        TradingAgentManager[] TradingAgents { get; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Add a new stream to the current portfolio
        /// </summary>
        /// <param name="stream"></param>
        void AddStream(DataStream stream);

        /// <summary>
        /// Notify this portfolio that an order has been filled (associated with the fill and the original order)
        /// </summary>
        /// <param name="fill"></param>
        /// <param name="order"></param>
        void GotFill(Trade fill, PendingOrder order);

        /// <summary>
        /// Notify this portfolio that an order has been received by the broker
        /// </summary>
        /// <param name="order"></param>
        void GotOrder(PendingOrder order);

        /// <summary>
        /// Notify this portfolio that an order has been cancelled by the broker
        /// </summary>
        /// <param name="order"></param>
        void GotOrderCancel(PendingOrder order);

        /// <summary>
        /// Notify this portfolio that an order has been updated by the broker
        /// </summary>
        /// <param name="order"></param>
        void GotOrderUpdate(PendingOrder order);

        /// <summary>
        /// Send new tick information this portfolio
        /// </summary>
        /// <param name="tick"></param>
        void GotTick(Tick tick);

        /// <summary>
        /// Initialize this portfolio
        /// </summary>
        void Initialize();

        /// <summary>
        /// Queue a new pending order, ready to be sent to the broker
        /// </summary>
        /// <param name="porder"></param>
        void QueueOrder(PendingOrder porder);

        /// <summary>
        /// Set default account to use by this portfolio
        /// </summary>
        /// <param name="account"></param>
        void SetAccount(IAccount account);

        /// <summary>
        /// Set position tracker implementation to use
        /// </summary>
        /// <param name="tracker"></param>
        void SetPositionTracker(IPositionTracker tracker);

        /// <summary>
        /// Set all pending orders currently known at the broker (update)
        /// </summary>
        /// <param name="pendingorders"></param>
        void SetPendingOrders(PendingOrder[] pendingorders);

        /// <summary>
        /// Attach another trading agent to the portfolio
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void InjectAgent<T>() where T : ITradingAgent;

        /// <summary>
        /// Remove Agent from this portfolio
        /// </summary>
        /// <param name="agentId"></param>
        /// <param name="flatten"></param>
        void RemoveAgent(int agentId, bool flatten = false);

        #endregion Public Methods
    }
}