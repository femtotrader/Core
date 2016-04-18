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

using System;

namespace Quantler.Interfaces
{
    /// <summary>
    /// generic interface that can be used with any type of tracker
    /// </summary>
    public interface GenericTracker
    {
        #region Public Properties

        /// <summary>
        /// get total number of labels/values
        /// </summary>
        int Count { get; }

        /// <summary>
        /// name of tracker
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// gets type of tracked values
        /// </summary>
        Type TrackedType { get; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// gets index associated with a given label, adding index if it doesn't exist (default
        /// value of index will be used)
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        int addindex(string txt);

        /// <summary>
        /// clears all tracked values and labels
        /// </summary>
        void Clear();

        /// <summary>
        /// display value of a tracked value for a given label
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        string Display(string txt);

        /// <summary>
        /// display tracked value for a given index
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        string Display(int idx);

        /// <summary>
        /// get index associated with a given label
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        int getindex(string txt);

        /// <summary>
        /// get label associated with an index
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        string getlabel(int idx);

        /// <summary>
        /// gets value of given index
        /// </summary>
        object Value(int idx);

        /// <summary>
        /// gets value of given label
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        object Value(string txt);

        /// <summary>
        /// attempts to get decimal value of index
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        decimal ValueDecimal(int idx);

        /// <summary>
        /// attempts to get decimal value of a label
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        decimal ValueDecimal(string txt);

        #endregion Public Methods
    }

    public interface GenericTrackerBool
    {
        #region Public Methods

        int addindex(string txt, bool v);

        bool getvalue(int idx);

        bool getvalue(string txt);

        void setvalue(int idx, bool v);

        #endregion Public Methods
    }

    public interface GenericTrackerDecimal
    {
        #region Public Methods

        int addindex(string txt, decimal v);

        decimal getvalue(int idx);

        decimal getvalue(string txt);

        void setvalue(int idx, decimal v);

        #endregion Public Methods
    }

    public interface GenericTrackerInt
    {
        #region Public Methods

        int addindex(string txt, int v);

        int getvalue(int idx);

        int getvalue(string txt);

        void setvalue(int idx, int v);

        #endregion Public Methods
    }

    public interface GenericTrackerLong
    {
        #region Public Methods

        int addindex(string txt, long v);

        long getvalue(int idx);

        long getvalue(string txt);

        void setvalue(int idx, long v);

        #endregion Public Methods
    }

    public interface GenericTrackerString
    {
        #region Public Methods

        int addindex(string txt, string s);

        string getvalue(int idx);

        string getvalue(string txt);

        void setvalue(int idx, string s);

        #endregion Public Methods
    }
}