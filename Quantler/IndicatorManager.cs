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

using Quantler.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Quantler
{
    /// <summary>
    /// Implementation of the indicator manager used by the default portfolio
    /// Here indicators are created and associated with the portfolio
    /// </summary>
    public class IndicatorManager : Interfaces.IndicatorManager
    {
        #region Private Fields

        /// <summary>
        /// Holder for the associated trading agent
        /// </summary>
        private readonly TradingAgentManager _agent;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Create default indicator manager for the assigned portfolio
        /// </summary>
        /// <param name="agent"></param>
        public IndicatorManager(TradingAgentManager agent)
        {
            Indicators = new List<Indicator>();
            _agent = agent;
        }

        #endregion Public Constructors

        #region Public Properties

        public ITradingAgent Agent
        {
            get { return _agent; }
        }

        /// <summary>
        /// Get all indicators current under management
        /// </summary>
        public List<Indicator> Indicators { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Check if two indicators are identical in properties and type
        /// </summary>
        /// <param name="self"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static bool PublicInstancePropertiesEqual(Indicator self, Indicator to)
        {
            //TODO: implement disruptor pattern 2.0 to allow for proper event management, currently this is not possible

            //Checks the following properties of two indicator objects to see if these are equal
            bool timeframe = self.BarSize == to.BarSize;
            bool period = self.Period == to.Period;
            bool stream = self.DataStreams[0].Security.Name == to.DataStreams[0].Security.Name;
            bool indicatortype = self.GetType() == to.GetType();
            bool compute = self.Compute != null && to.Compute != null ? self.Compute.GetHashCode() == to.Compute.GetHashCode() : self.Compute == null && to.Compute == null;

            return timeframe &&
                   period &&
                   stream &&
                   indicatortype &&
                   compute;
        }

        /// <summary>
        /// Subscribe a new indicator to this indicator manager
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indicator"></param>
        /// <returns></returns>
        public T Subscribe<T>(T indicator)
        {
            var found = Indicators.Where(x => x is T);
            var baseindicator = (Indicator)indicator;

            foreach (var item in found)
            {
                if (PublicInstancePropertiesEqual(item, baseindicator))
                    return (T)item;
            }

            //Check portfolio data streams and barsizes
            foreach (var ds in baseindicator.DataStreams)
                ds.AddInterval((int)baseindicator.BarSize.TotalSeconds);

            //Check for events to handle
            _agent.AddEvent(indicator);

            //Add this indicator
            Indicators.Add(baseindicator);
            return indicator;
        }

        #endregion Public Methods
    }
}