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

namespace Quantler.Reflection
{
    /// <summary>
    /// Invoke link with 2 parameters and a returntype
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="TRt"></typeparam>
    public class InvokeLinkFunc<T, T2, TRt>
    {
        #region Public Properties

        public Func<T, T2, TRt> Action { get; set; }
        public Type BaseType { get; set; }
        public DataStream[] DataStreams { get; set; }
        public Type ParmType { get; set; }
        public dynamic Result { get; set; }
        public Type ReturnType { get; set; }
        public Type SecondParmType { get; set; }

        #endregion Public Properties
    }

    /// <summary>
    /// Invoke link with 1 parameter and a returntype
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TRt"></typeparam>
    public class InvokeLinkFunc<T, TRt>
    {
        #region Public Properties

        public Func<T, TRt> Action { get; set; }

        #region Public Properties

        public Type BaseType { get; set; }
        public DataStream[] DataStreams { get; set; }
        public Type ParmType { get; set; }
        public dynamic Result { get; set; }
        public Type ReturnType { get; set; }

        #endregion Public Properties
    }

    /// <summary>
    /// Invoke link with 2 parameters
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class InvokeLinkVoid<T, T2>
    {
        public Action<T, T2> Action { get; set; }
        public Type BaseType { get; set; }
        public DataStream[] DataStreams { get; set; }
        public Type ParmType { get; set; }
        public dynamic Result { get; set; }
        public Type ReturnType { get; set; }

        #endregion Public Properties
    }

    /// <summary>
    /// Invoke link with 1 parameter, returns void
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InvokeLinkVoid<T>
    {
        #region Public Properties

        public Action<T> Action { get; set; }
        public Type BaseType { get; set; }
        public Type ParmType { get; set; }

        #endregion Public Properties

        public DataStream[] DataStreams { get; set; }
    }

    /// <summary>
    /// Invoke link with no parmaters, returns void
    /// </summary>
    public class InvokeLinkVoid
    {
        #region Public Properties

        public Action Action { get; set; }
        public Type BaseType { get; set; }
        public Type ParmType { get; set; }

        #endregion Public Properties

        public DataStream[] DataStreams { get; set; }
    }
}