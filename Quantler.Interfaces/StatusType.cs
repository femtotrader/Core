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
    public enum StatusType
    {
        ORDER_NOT_FOUND = -112,
        FEATURE_NOT_IMPLEMENTED = -108,
        CLIENTNOTREGISTERED = -107,
        EMPTY_ORDER = -106,
        UNKNOWN_MESSAGE = -105,
        UNKNOWN_SYMBOL = -104,
        BROKERSERVER_NOT_FOUND = -103,
        DUPLICATE_ORDERID = -101,
        SYMBOL_NOT_LOADED = -100,
        PROHIBITED_BY_FIFO = 150,
        HEDGING_PROHIBITED = 149,
        TOO_MANY_ORDERS = 148,
        EXPIRATION_DENIED = 147,
        CONTEXT_BUSY = 146,
        MODIFY_DENIED = 145,
        LONG_ONLY = 140,
        ORDER_LOCKED = 139,
        REQUOTE = 138,
        BROKER_BUSY = 137,
        OFF_QUOTES = 136,
        PRICE_CHANGED = 135,
        INSUFFICIENT_CAPITAL = 134,
        TRADING_DISABLED = 133,
        MARKET_CLOSED = 132,
        ORDER_INVALID_VOLUME = 131,
        ORDER_INVALID_STOP = 130,
        ORDER_INVALID_PRICE = 129,
        TRADE_TIMEOUT = 128,
        INVALID_ACCOUNT = 65,
        ACCOUNT_DISABLED = 64,
        MALFUNCTIONING_TRADE = 9,
        TOO_MANY_REQUESTS = 8,
        NOT_ENOUGH_RIGHTS = 7,
        CONNECTION_LOST = 6,
        OLD_VERSION = 5,
        SERVER_BUSY = 4,
        INVALID_TRADE_PARAMETERS = 3,
        COMMON = 2,
        UNKNOWN_ERROR = 1,
        OK = 0,
        ORDER_FILLED = -1,
        ORDER_PARTIALFILL = -2
    }
}