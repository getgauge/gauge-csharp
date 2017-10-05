// Copyright 2015 ThoughtWorks, Inc.

// This file is part of Gauge-CSharp.

// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace Gauge.CSharp.Lib
{
    /// <summary>
    ///     A key-value store that holds any object data.
    /// </summary>
    [Serializable]
    public class DataStore
    {
        public DataStore()
        {
            Initialize();
        }

        private Dictionary<object, object> Dictionary { get; set; }

        /// <summary>
        ///     Gets the number of entries in the datastore.
        /// </summary>
        public int Count => Dictionary.Count;

        /// <summary>
        ///     Initializes a datastore, with a new dictionary.
        /// </summary>
        public void Initialize()
        {
            Dictionary = new Dictionary<object, object>();
        }

        /// <summary>
        ///     Gets the value that is stored against a given key.
        /// </summary>
        /// <param name="key">key for lookup</param>
        /// <returns>value as object, if exists, null when key does not exist.</returns>
        public object Get(string key)
        {
            object outVal;
            var valueExists = Dictionary.TryGetValue(key, out outVal);
            return valueExists ? outVal : null;
        }

        /// <summary>
        ///     Returns the value of the object cast as Type provided. Raises an exception when the key is not present.
        /// </summary>
        /// <typeparam name="T">The type to cast the return value</typeparam>
        /// <param name="key">key for lookup</param>
        /// <returns>value as T, if exists, null when key does not exist.</returns>
        public T Get<T>(string key)
        {
            return (T) Get(key);
        }

        /// <summary>
        ///     Adds a value to the datastore against given key.
        /// </summary>
        /// <param name="key">Key to store the value against</param>
        /// <param name="value">Value to store</param>
        public void Add(string key, object value)
        {
            Dictionary[key] = value;
        }

        /// <summary>
        ///     Clears the datastore
        /// </summary>
        public void Clear()
        {
            Dictionary.Clear();
        }
    }
}