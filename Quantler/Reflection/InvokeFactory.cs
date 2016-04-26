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

namespace Quantler.Reflection
{
    /// <summary>
    /// Factory used for invoking collections of methods based on filters
    /// </summary>
    public class InvokeFactory
    {
        #region Public Methods

        /// <summary>
        /// Invoke all actions that return values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="TRt"></typeparam>
        /// <param name="actions"></param>
        /// <param name="data"></param>
        /// <param name="seconddata"></param>
        public void InvokeAll<T, T2, TRt>(List<InvokeLinkFunc<T, T2, TRt>> actions, T data, T2 seconddata)
        {
            foreach (var item in actions)
                item.Result = item.Action.Invoke(data, seconddata);
        }

        /// <summary>
        /// Invoke all actions that return values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TRt"></typeparam>
        /// <param name="actions"></param>
        /// <param name="data"></param>
        public void InvokeAll<T, TRt>(List<InvokeLinkFunc<T, TRt>> actions, T data)
        {
            foreach (var item in actions)
                item.Result = item.Action.Invoke(data);
        }

        /// <summary>
        /// Invoke method with 2 parameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="actions"></param>
        /// <param name="data"></param>
        /// <param name="seconddata"></param>
        public void InvokeAll<T, T2>(List<InvokeLinkVoid<T, T2>> actions, T data, T2 seconddata)
        {
            foreach (var item in actions)
                item.Action.Invoke(data, seconddata);
        }

        /// <summary>
        /// Invoke all methods for a given datastream and include a specific
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actions"></param>
        /// <param name="data"></param>
        /// <param name="stream"></param>
        /// <param name="filterInclude"></param>
        public void InvokeAll<T>(List<InvokeLinkVoid<T>> actions, T data, DataStream stream, params Type[] filterInclude)
        {
            ExecuteInvoke(actions, data, stream, filterInclude);
        }

        /// <summary>
        /// Invoke all methods for a given link including a list of types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actions"></param>
        /// <param name="data"></param>
        /// <param name="filterInclude"></param>
        public void InvokeAll<T>(List<InvokeLinkVoid<T>> actions, T data, params Type[] filterInclude)
        {
            ExecuteInvoke(actions, data, null, filterInclude);
        }

        /// <summary>
        /// Invoke all methods for a given link
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actions"></param>
        /// <param name="data"></param>
        public void InvokeAll<T>(List<InvokeLinkVoid<T>> actions, T data)
        {
            ExecuteInvoke(actions, data, null);
        }

        /// <summary>
        /// Invoke all methods that do not have parameters based on the filter.
        /// </summary>
        /// <param name="actions"></param>
        /// <param name="filterInclude"></param>
        public void InvokeAll(List<InvokeLinkVoid> actions, Type filterInclude)
        {
            var toinvoke = actions.AsEnumerable();

            if (filterInclude != null)
                toinvoke = toinvoke.Where(x => x.BaseType == filterInclude);

            foreach (var item in toinvoke)
                item.Action.Invoke();
        }

        /// <summary>
        /// Invoke all methods that do not have parameters
        /// </summary>
        /// <param name="actions"></param>
        public void InvokeAll(List<InvokeLinkVoid> actions)
        {
            InvokeAll(actions, null);
        }

        /// <summary>
        /// Invoke all methods for a list of links excluding certain types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actions"></param>
        /// <param name="data"></param>
        /// <param name="filterExclude"></param>
        public void InvokeAllExclude<T>(List<InvokeLinkVoid<T>> actions, T data, params Type[] filterExclude)
        {
            InvokeAll(actions.Where(x => !filterExclude.Contains(x.BaseType)).ToList(), data);
        }

        /// <summary>
        /// Invoke all methods for a list of links excluding certain types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actions"></param>
        /// <param name="data"></param>
        /// <param name="stream"></param>
        /// <param name="filterExclude"></param>
        public void InvokeAllExclude<T>(List<InvokeLinkVoid<T>> actions, T data, DataStream stream, params Type[] filterExclude)
        {
            ExecuteInvoke(actions.Where(x => !filterExclude.Contains(x.BaseType)).ToList(), data, stream, null);
        }

        /// <summary>
        /// Invoke all methods that do not have parameters excluding the type provided
        /// </summary>
        /// <param name="actions"></param>
        /// <param name="filterExclude"></param>
        public void InvokeAllExclude(List<InvokeLinkVoid> actions, Type filterExclude)
        {
            InvokeAll(actions.Where(x => x.BaseType != filterExclude).ToList(), null);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Execute the invoke link
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actions"></param>
        /// <param name="data"></param>
        /// <param name="stream"></param>
        /// <param name="filterInclude"></param>
        private void ExecuteInvoke<T>(List<InvokeLinkVoid<T>> actions, T data, DataStream stream, params Type[] filterInclude)
        {
            if (actions.Count == 0)
                return;

            var toinvoke = actions.Where(x => x.ParmType == typeof(T));

            if (stream != null)
                toinvoke = toinvoke.Where(x => x.DataStreams.Contains(stream));

            if (filterInclude != null && filterInclude.Length > 0)
                toinvoke = toinvoke.Where(x => filterInclude.Contains(x.BaseType));

            foreach (var item in toinvoke)
                item.Action.Invoke(data);
        }

        #endregion Private Methods
    }
}