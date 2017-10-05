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

using NUnit.Framework;

namespace Gauge.CSharp.Lib.UnitTests
{
    [TestFixture]
    public class DataStoreFactoryTests
    {
        [Test]
        public void ShouldBeAbleToStoreValuesToDatastoreWithoutInitializing()
        {
            var dataStore = DataStoreFactory.ScenarioDataStore;
            dataStore.Add("myKey", "myValue");
            Assert.AreEqual(dataStore.Get("myKey"), "myValue");
        }

        [Test]
        public void ShouldGetDataStoreForScenario()
        {
            var dataStore = DataStoreFactory.ScenarioDataStore;

            Assert.NotNull(dataStore);
            Assert.IsInstanceOf<DataStore>(dataStore);
        }

        [Test]
        public void ShouldGetDataStoreForSpec()
        {
            var dataStore = DataStoreFactory.SpecDataStore;

            Assert.NotNull(dataStore);
            Assert.IsInstanceOf<DataStore>(dataStore);
        }

        [Test]
        public void ShouldGetDataStoreForSuite()
        {
            var dataStore = DataStoreFactory.SuiteDataStore;

            Assert.NotNull(dataStore);
            Assert.IsInstanceOf<DataStore>(dataStore);
        }
    }
}