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

using System.Collections.Generic;

namespace Gauge.CSharp.Lib
{
    /// <summary>
    ///     Holds various DataStores, that have lifetime defined as per their scope.
    ///     Ex: ScenarioDataStore has its scope defined to a particular scenario.
    /// </summary>
    public class DataStoreFactory
    {
        private static readonly Dictionary<DataStoreType, DataStore> DataStores =
            new Dictionary<DataStoreType, DataStore>
            {
                {DataStoreType.Suite, new DataStore()},
                {DataStoreType.Spec, new DataStore()},
                {DataStoreType.Scenario, new DataStore()}
            };

        /// <summary>
        ///     Access the Suite level DataStore.
        /// </summary>
        public static DataStore SuiteDataStore => DataStores[DataStoreType.Suite];

        /// <summary>
        ///     Access the Specification level DataStore.
        /// </summary>
        public static DataStore SpecDataStore => DataStores[DataStoreType.Spec];

        /// <summary>
        ///     Access the Scenario level DataStore.
        /// </summary>
        public static DataStore ScenarioDataStore => DataStores[DataStoreType.Scenario];

        /// <summary>
        ///     <remarks>
        ///         FOR GAUGE INTERNAL USE ONLY.
        ///     </remarks>
        ///     Initializes the Suite level DataStore.
        /// </summary>
        public static void InitializeSuiteDataStore()
        {
            SuiteDataStore.Initialize();
        }

        /// <summary>
        ///     <remarks>
        ///         FOR GAUGE INTERNAL USE ONLY.
        ///     </remarks>
        ///     Initializes the Spec level DataStore.
        /// </summary>
        public static void InitializeSpecDataStore()
        {
            SpecDataStore.Initialize();
        }

        /// <summary>
        ///     <remarks>
        ///         FOR GAUGE INTERNAL USE ONLY.
        ///     </remarks>
        ///     Initializes the Scenario level DataStore.
        /// </summary>
        public static void InitializeScenarioDataStore()
        {
            ScenarioDataStore.Initialize();
        }
    }
}