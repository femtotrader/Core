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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantler.Indicators
{
    public class IndicatorDataSerie : DataSerie
    {
        #region Private Fields

        private int _maxHistory = 5;
        private readonly List<decimal> _values = new List<decimal>();

        #endregion Private Fields

        #region Public Properties

        public int Count
        {
            get { return _values.Count; }
        }

        public decimal CurrentValue
        {
            get { return Count > 0 ? _values[0] : 0; }
        }

        public bool IsFalling
        {
            get { return Count >= 2 && (this[0] < this[1]); }
        }

        public bool IsRising
        {
            get { return Count >= 2 && (this[0] > this[1]); }
        }

        public int MaxHistory
        {
            get { return _maxHistory; }
            set { _maxHistory = value; }
        }

        #endregion Public Properties

        #region Public Indexers

        public decimal this[int index]
        {
            get
            {
                index = Math.Abs(index);
                if (index > Count || index > _maxHistory)
                {
                    _maxHistory = index + 1;
                    return decimal.Zero;
                }

                //Check if the request value exists
                if (index < Count)
                    return _values[index];
                else
                    return 0;
            }
            set
            {
                _values.Insert(index, value);
                if (Count > MaxHistory)
                    _values.RemoveRange(MaxHistory, Count - MaxHistory);
            }
        }

        #endregion Public Indexers

        #region Public Methods

        public decimal Avg(int lookback)
        {
            return _values.Take(Math.Abs(lookback)).Average();
        }

        public bool CrossedAbove(decimal seriesB, int lookback = 3)
        {
            return CrossedAbove(new[] { seriesB }, lookback);
        }

        public bool CrossedAbove(DataSerie b, int lookback = 3)
        {
            lookback = Math.Abs(lookback);

            if (b.Count < lookback)
                return false;

            decimal[] valuesB = new decimal[lookback];

            for (int i = 0; i < lookback; i++)
                valuesB[i] = b[i];

            return CrossedAbove(valuesB, lookback);
        }

        public bool CrossedAbove(decimal[] seriesB, int lookback = 3)
        {
            decimal[] seriesA = _values.ToArray();
            lookback = Math.Abs(lookback);

            bool toreturn = false;

            //Lookback will start at this number so include 0 and you need to substract 1
            lookback--;

            //check lookbackperiod
            if (seriesA.Length <= lookback)
                lookback = seriesA.Length - 1;
            if (seriesB.Length <= lookback)
                lookback = seriesB.Length - 1;

            //Check if under
            bool under = false;
            for (int i = lookback; i > -1; i--)
            {
                if (seriesA[i] < seriesB[i] && !under) under = true;
                if (seriesA[i] > seriesB[i] && under) return true;
            }

            return toreturn;
        }

        public bool CrossedUnder(decimal seriesB, int lookback = 3)
        {
            return CrossedUnder(new[] { seriesB }, lookback);
        }

        public bool CrossedUnder(DataSerie b, int lookback = 3)
        {
            lookback = Math.Abs(lookback);

            if (b.Count < lookback)
                return false;

            decimal[] valuesB = new decimal[lookback];

            for (int i = 0; i < lookback; i++)
                valuesB[i] = b[i];

            return CrossedUnder(valuesB, lookback);
        }

        public bool CrossedUnder(decimal[] seriesB, int lookback = 3)
        {
            decimal[] seriesA = _values.ToArray();
            lookback = Math.Abs(lookback);

            bool toreturn = false;

            //Lookback will start at this number so include 0 and you need to substract 1
            lookback--;

            //check lookbackperiod
            if (seriesA.Length <= lookback)
                lookback = seriesA.Length - 1;
            if (seriesB.Length <= lookback)
                lookback = seriesB.Length - 1;

            //Check if above
            bool above = false;
            for (int i = lookback; i > -1; i--)
            {
                if (seriesA[i] > seriesB[i] && !above) above = true;
                if (seriesA[i] < seriesB[i] && above) return true;
            }

            return toreturn;
        }

        public decimal Max(int lookback)
        {
            return _values.Take(Math.Abs(lookback)).Max();
        }

        public decimal Min(int lookback)
        {
            return _values.Take(Math.Abs(lookback)).Min();
        }

        public decimal Sum(int lookback)
        {
            return _values.Take(Math.Abs(lookback)).Sum();
        }

        #endregion Public Methods
    }
}