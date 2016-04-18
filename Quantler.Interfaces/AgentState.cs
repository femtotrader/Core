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
    public enum AgentState
    {
        /// <summary>
        /// Enter Long, state
        /// </summary>
        EntryLong,

        /// <summary>
        /// Enter Short, state
        /// </summary>
        EntryShort,

        /// <summary>
        /// Exit Long, state
        /// </summary>
        ExitLong,

        /// <summary>
        /// Exit Short, state
        /// </summary>
        ExitShort,

        /// <summary>
        /// Flatten any current holdings, state
        /// </summary>
        Flatten,

        /// <summary>
        /// Currently no entry allowed, state
        /// </summary>
        NoEntry
    }
}