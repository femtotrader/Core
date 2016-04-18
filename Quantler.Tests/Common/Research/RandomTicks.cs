#region Copyright

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

#endregion Copyright

using Quantler.Data.Ticks;
using Quantler.Interfaces;
using System;

namespace Quantler.Tests.Common.Research
{
    /// <summary>
    /// create an array of ticks that is a random walk from an initial set of ticks. walk varies
    /// between +MaxMoveCents and -MaxMoveCents. at present no quotes are generated, only trades.
    /// </summary>
    public class RandomTicks
    {
        #region Private Fields

        private Tick[][] _feed = new Tick[0][];

        private readonly decimal[] _iprice;

        private readonly int _maxmove = 3;

        private readonly string[] _syms;

        private readonly Random r;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// creates random ticks from a list of symbols, with randomized initial prices
        /// </summary>
        /// <param name="symbols"></param>
        public RandomTicks(string[] symbols)
            : this(symbols, RandomPrices(symbols.Length), (int)DateTime.Now.Ticks)
        {
        }

        /// <summary>
        /// creates random ticks for a list of symbols and starting prices. prices should be in same
        /// order for symbol they represent.
        /// </summary>
        /// <param name="symbols">list of symbols</param>
        /// <param name="startingprices">opening trade for each symbol</param>
        public RandomTicks(string[] symbols, decimal[] startingprices, int seed)
        {
            VolPerTrade = 100;
            // save symbol list
            _syms = symbols;
            // save initial prices
            _iprice = startingprices;
            // init generator
            r = new Random(seed);
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// obtain list of symbols
        /// </summary>
        public string[] Symbols { get { return _syms; } }

        /// <summary>
        /// obtain randomized ticks. each 1st dimension array corresponds to Symbol in same-position
        /// of this.Symbols[] Ticks are listed sequentially in the 2nd dimension.
        /// </summary>
        public Tick[][] Ticks { get { return _feed; } }

        /// <summary>
        /// volume to use on each tick
        /// </summary>
        public int VolPerTrade { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// generate random ticks for single symbol
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="Ticks"></param>
        /// <returns></returns>
        public static Tick[] GenerateSymbol(string sym, int Ticks)
        {
            RandomTicks rt = new RandomTicks(new string[] { sym });
            rt.Generate(Ticks);
            return rt.Ticks[0];
        }

        /// <summary>
        /// gets desired number of random [initial] prices.
        /// </summary>
        /// <param name="pricecount"></param>
        /// <returns></returns>
        public static decimal[] RandomPrices(int pricecount)
        {
            return RandomPrices(pricecount, (int)DateTime.Now.Ticks);
        }

        /// <summary>
        /// provides a group of random prices
        /// </summary>
        /// <param name="pricecout"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static decimal[] RandomPrices(int pricecount, int seed)
        {
            decimal[] p = new decimal[pricecount];
            Random r = new Random(seed);
            for (int i = 0; i < p.Length; i++)
                p[i] = (decimal)r.NextDouble() * 99 + 1;
            return p;
        }

        /// <summary>
        /// generate Ticks per symbol using a random walk from initial prices
        /// </summary>
        /// <param name="Ticks"></param>
        public void Generate(int Ticks)
        {
            Generate(Ticks, (int)DateTime.Now.Ticks);
        }

        public void Generate(int Ticks, int Seed)
        {
            _feed = new Tick[_syms.Length][];

            // for each symbol
            for (int i = 0; i < _syms.Length; i++)
            {
                // generate a list of requested ticks
                _feed[i] = new Tick[Ticks];
                for (int j = 0; j < Ticks; j++)
                {
                    // by taking the initial price and moving it some amount between min and max move
                    _iprice[i] += (decimal)r.Next(_maxmove * -1, _maxmove + 1) / 100;
                    // make sure it's still positive
                    if (_iprice[i] < 0) _iprice[i] = 0;
                    // then store this result as a tick and continue
                    _feed[i][j] = TickImpl.NewTrade(_syms[i], _iprice[i], VolPerTrade);
                }
            }
        }

        #endregion Public Methods
    }
}