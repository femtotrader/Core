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
    public interface TradingAgentManager : ITradingAgent
    {
        #region Public Properties

        /// <summary>
        /// Indicator manager, currently subscribed indicators
        /// </summary>
        IndicatorManager IndicatorManagement { get; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Add another datastream for this agent and the portfolio
        /// </summary>
        /// <param name="sample"></param>
        void AddDataStream(DataStream sample);

        /// <summary>
        /// Add an event to be executed on specific data (ontick, onbar etc)
        /// </summary>
        /// <param name="Event"></param>
        void AddEvent(object Event);

        /// <summary>
        /// Add template to the current agent
        /// </summary>
        /// <param name="template"></param>
        void AddTemplate(ITemplate template);

        /// <summary>
        /// Set the default stream on which this agent listens
        /// </summary>
        /// <param name="stream"></param>
        void SetDefaultStream(DataStream stream);

        /// <summary>
        /// Set portfolio used for this agent
        /// </summary>
        /// <param name="portfolio"></param>
        void SetPortfolio(PortfolioManager portfolio);

        /// <summary>
        /// Set position tracker used for this agent
        /// </summary>
        /// <param name="postracker"></param>
        void SetPositionsTracker(IPositionTracker postracker);

        #endregion Public Methods
    }
}