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

namespace Quantler.Interfaces
{
    /// <summary>
    /// historical simulator
    /// </summary>
    public interface HistSim
    {
        #region Public Events

        event TickDelegate GotTick;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Gets next tick in the simulation
        /// </summary>
        long NextTickTime { get; }

        /// <summary>
        /// Total ticks available for processing, based on provided filter or tick files.
        /// </summary>
        int TicksPresent { get; }

        /// <summary>
        /// Ticks processed in this simulation run.
        /// </summary>
        int TicksProcessed { get; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// start simulation and run to specified date/time
        /// </summary>
        /// <param name="ftime"></param>
        void PlayTo(long ftime);

        /// <summary>
        /// reset simulation
        /// </summary>
        void Reset();

        /// <summary>
        /// stop simulation
        /// </summary>
        void Stop();

        #endregion Public Methods
    }
}