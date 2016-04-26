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

using TicTacTec.TA.Library;

namespace Quantler.TALib
{
    /// <summary>
    /// Class containing TA-LIB helper functions
    /// </summary>
    public class TaLib
    {
        #region Public Methods

        /// <summary>
        /// Returns the ADX calculated value
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <param name="close"></param>
        /// <param name="period"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult Adx(double[] high, double[] low, double[] close, int period, int offset = 0)
        {
            int begin;
            int length;
            double[] output = new double[high.Length];

            Core.RetCode retCode = Core.Adx(0, high.Length - 1 - offset, high, low, close,
                period, out begin, out length, output);

            //Return the return object
            return new TaLibSingleResult(output, retCode, length);
        }

        /// <summary>
        /// Returns the Aroon calculated value
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <param name="period"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibAroonResult Aroon(double[] high, double[] low, int period, int offset = 0)
        {
            int begin;
            int length;
            double[] ups = new double[high.Length];
            double[] downs = new double[high.Length];

            Core.RetCode retCode = Core.Aroon(0, high.Length - 1 - offset, high, low,
                period, out begin, out length, downs, ups);

            //Return the return object
            return new TaLibAroonResult(ups, downs, retCode, length);
        }

        /// <summary>
        /// Returns the Aroon Oscillator calculated value
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <param name="period"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult AroonOsc(double[] high, double[] low, int period, int offset = 0)
        {
            int begin;
            int length;
            double[] result = new double[high.Length];

            Core.RetCode retCode = Core.AroonOsc(0, high.Length - 1 - offset, high, low,
                period, out begin, out length, result);

            //Return the return object
            return new TaLibSingleResult(result, retCode, length);
        }

        /// <summary>
        /// Returns the Bollinger Bands calculated value
        /// </summary>
        /// <param name="input"></param>
        /// <param name="sdUp"></param>
        /// <param name="sdLow"></param>
        /// <param name="period"></param>
        /// <param name="maType"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibBandResult Bbands(double[] input, double sdUp, double sdLow, int period, int maType = 0, int offset = 0)
        {
            int begin;
            int length;
            double[] upperBand = new double[input.Length];
            double[] middleBand = new double[input.Length];
            double[] lowerBand = new double[input.Length];

            Core.RetCode retCode = Core.Bbands(0, input.Length - 1 - offset, input, period, sdUp, sdLow, (Core.MAType)maType, out begin, out length, upperBand, middleBand, lowerBand);

            //Return the return object
            return new TaLibBandResult(upperBand, middleBand, lowerBand, retCode, length);
        }

        /// <summary>
        /// Returns the Balance Of Power calculated value
        /// </summary>
        /// <param name="open"></param>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <param name="close"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult Bop(double[] open, double[] high, double[] low, double[] close, int offset = 0)
        {
            int begin;
            int length;
            double[] result = new double[high.Length];

            Core.RetCode retCode = Core.Bop(0, high.Length - 1 - offset, open, high, low, close, out begin, out length, result);

            //Return the return object
            return new TaLibSingleResult(result, retCode, length);
        }

        /// <summary>
        /// Returns the Commodity Channel Index calculated value
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <param name="close"></param>
        /// <param name="period"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult Cci(double[] high, double[] low, double[] close, int period, int offset = 0)
        {
            int begin;
            int length;
            double[] result = new double[high.Length];

            Core.RetCode retCode = Core.Cci(0, high.Length - 1 - offset, high, low, close, period, out begin, out length, result);

            //Return the return object
            return new TaLibSingleResult(result, retCode, length);
        }

        /// <summary>
        /// Calculate the Chande Momentum Oscillator indicator based on the input values
        /// </summary>
        /// <param name="input"></param>
        /// <param name="period"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult ChandeMomOsc(double[] input, int period, int offset = 0)
        {
            double[] output = new double[input.Length];
            int begin;
            int length;

            Core.RetCode retCode = Core.Cmo(0, input.Length - 1 - offset, input,
                period, out begin, out length, output);

            //Return the return object
            return new TaLibSingleResult(output, retCode, length);
        }

        /// <summary>
        /// DEMA (double exponential)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="period"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult Dema(double[] input, int period, int offset = 0)
        {
            return Ma(input, period, (int)Core.MAType.Dema, offset);
        }

        /// <summary>
        /// Calculate the Directional Movement Index based on the inserted values
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <param name="period"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibDmiResult Dmi(double[] high, double[] low, int period, int offset = 1)
        {
            double[] plusoutput = new double[high.Length];
            double[] minoutput = new double[high.Length];
            int begin;
            int length;

            Core.RetCode retCode = Core.MinusDM(0, high.Length - 1 - offset, high, low,
                period, out begin, out length, plusoutput);

            Core.MinusDM(0, high.Length - 1 - offset, high, low,
                period, out begin, out length, minoutput);

            return new TaLibDmiResult(plusoutput, minoutput, retCode, length);
        }

        /// <summary>
        /// Returns the Directional Movement Index calculated value
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <param name="close"></param>
        /// <param name="period"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult Dx(double[] high, double[] low, double[] close, int period, int offset = 0)
        {
            int begin;
            int length;
            double[] output = new double[high.Length];

            Core.RetCode retCode = Core.Dx(0, high.Length - 1 - offset, high, low, close,
                period, out begin, out length, output);

            //Return the return object
            return new TaLibSingleResult(output, retCode, length);
        }

        /// <summary>
        /// EMA (exponential)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="period"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult Ema(double[] input, int period, int offset = 0)
        {
            return Ma(input, period, (int)Core.MAType.Ema, offset);
        }

        /// <summary>
        /// KAMA (Kaufman adaptive)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="period"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult Kama(double[] input, int period, int offset = 0)
        {
            return Ma(input, period, (int)Core.MAType.Kama, offset);
        }

        /// <summary>
        /// Used for general Moving Average calculations
        /// </summary>
        /// <param name="input"></param>
        /// <param name="period"></param>
        /// <param name="maType"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult Ma(double[] input, int period, int maType, int offset = 0)
        {
            double[] output = new double[input.Length];
            int begin;
            int length;

            Core.RetCode retCode = Core.MovingAverage(0, input.Length - 1 - offset, input,
                period, (Core.MAType)maType, out begin, out length, output);

            //Return the return object
            return new TaLibSingleResult(output, retCode, length);
        }

        /// <summary>
        /// Returns the MACD indicator
        /// </summary>
        /// <param name="input"></param>
        /// <param name="fastperiod"></param>
        /// <param name="slowperiod"></param>
        /// <param name="signalperiod"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibMacdResult Macd(double[] input, int fastperiod, int slowperiod, int signalperiod, int offset = 1)
        {
            double[] macdline = new double[input.Length];
            double[] macdsignal = new double[input.Length];
            double[] macdhisto = new double[input.Length];
            int begin;
            int length;

            Core.RetCode retCode = Core.Macd(0, input.Length - 1 - offset, input,
                fastperiod, slowperiod, signalperiod, out begin, out length, macdline, macdsignal, macdhisto);

            return new TaLibMacdResult(macdline, macdsignal, macdhisto, retCode, length);
        }

        /// <summary>
        /// MAMA (Mesa adaptive)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="period"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult Mama(double[] input, int period, int offset = 0)
        {
            return Ma(input, period, (int)Core.MAType.Mama, offset);
        }

        /// <summary>
        /// Calculate the Momentum indicator based on the input values
        /// </summary>
        /// <param name="input"></param>
        /// <param name="period"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult Mom(double[] input, int period, int offset = 0)
        {
            double[] output = new double[input.Length];
            int begin;
            int length;

            Core.RetCode retCode = Core.Mom(0, input.Length - 1 - offset, input,
                period, out begin, out length, output);

            //Return the return object
            return new TaLibSingleResult(output, retCode, length);
        }

        /// <summary>
        /// Returns the Rate Of Change indicator based on the input values
        /// </summary>
        /// <param name="input"></param>
        /// <param name="period"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult Roc(double[] input, int period, int offset = 0)
        {
            double[] output = new double[input.Length];
            int begin;
            int length;

            Core.RetCode retCode = Core.Roc(0, input.Length - 1 - offset, input,
                period, out begin, out length, output);

            //Return the return object
            return new TaLibSingleResult(output, retCode, length);
        }

        /// <summary>
        /// Calculate the RSI based on the input data
        /// </summary>
        /// <param name="input"></param>
        /// <param name="period"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult Rsi(double[] input, int period, int offset = 0)
        {
            double[] output = new double[input.Length];
            int begin;
            int length;

            Core.RetCode retCode = Core.Rsi(0, input.Length - 1 - offset, input,
                period, out begin, out length, output);

            //Return the return object
            return new TaLibSingleResult(output, retCode, length);
        }

        /// <summary>
        /// Returns the Parabolic SAR calculated value
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <param name="acc"></param>
        /// <param name="max"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult Sar(double[] high, double[] low, double acc, double max, int offset = 0)
        {
            int begin;
            int length;
            double[] output = new double[high.Length];

            Core.RetCode retCode = Core.Sar(0, high.Length - 1 - offset, high, low, acc, max, out begin, out length, output);

            //Return the return object
            return new TaLibSingleResult(output, retCode, length);
        }

        /// <summary>
        /// SMA (simple)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="period"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult Sma(double[] input, int period, int offset = 0)
        {
            return Ma(input, period, (int)Core.MAType.Sma, offset);
        }

        /// <summary>
        /// Returns the Stochastic Oscillator indicator
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <param name="close"></param>
        /// <param name="fastperiod"></param>
        /// <param name="slowperiod"></param>
        /// <param name="slowDperiod"></param>
        /// <param name="matype"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibStochResult Stoch(double[] high, double[] low, double[] close,
            int fastperiod, int slowperiod, int slowDperiod, Core.MAType matype, int offset = 1)
        {
            double[] outputK = new double[high.Length];
            double[] outputD = new double[high.Length];

            int begin;
            int length;

            Core.RetCode retCode = Core.Stoch(0, high.Length - 1 - offset, high, low, close,
                fastperiod, slowperiod, matype, slowDperiod, matype, out begin, out length, outputK, outputD);

            return new TaLibStochResult(outputK, outputD, retCode, length);
        }

        /// <summary>
        /// TEMA (triple exponential)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="period"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult Tema(double[] input, int period, int offset = 0)
        {
            return Ma(input, period, (int)Core.MAType.Tema, offset);
        }

        /// <summary>
        /// T3 (triple exponential T3)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="period"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult Three(double[] input, int period, int offset = 0)
        {
            return Ma(input, period, (int)Core.MAType.T3, offset);
        }

        /// <summary>
        /// TRIMA (triangular)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="period"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult Trima(double[] input, int period, int offset = 0)
        {
            return Ma(input, period, (int)Core.MAType.Trima, offset);
        }

        /// <summary>
        /// Returns the True Range indicator
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <param name="close"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult TrueRange(double[] high, double[] low, double[] close, int offset = 0)
        {
            double[] output = new double[high.Length];
            int begin;
            int length;

            Core.RetCode retCode = Core.TrueRange(0, high.Length - 1 - offset, high, low, close, out begin, out length, output);

            //Return the return object
            return new TaLibSingleResult(output, retCode, length);
        }

        /// <summary>
        /// Calculate the WilliamsR
        /// </summary>
        /// <param name="high">High Bars</param>
        /// <param name="low">Low Bars</param>
        /// <param name="close">Close Bars</param>
        /// <param name="period">Period back</param>
        /// <param name="offset">Offset</param>
        /// <returns></returns>
        public TaLibSingleResult WilliamsR(double[] high, double[] low, double[] close,
    int period, int offset = 0)
        {
            double[] output = new double[high.Length];
            int begin;
            int length;

            Core.RetCode retCode = Core.WillR(0, high.Length - 1 - offset, high, low, close,
                period, out begin, out length, output);

            //Return the return object
            return new TaLibSingleResult(output, retCode, length);
        }

        /// <summary>
        /// WMA (weighted)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="period"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public TaLibSingleResult Wma(double[] input, int period, int offset = 0)
        {
            return Ma(input, period, (int)Core.MAType.Wma, offset);
        }

        #endregion Public Methods
    }
}