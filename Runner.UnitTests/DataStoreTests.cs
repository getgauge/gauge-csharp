// Copyright 2015 ThoughtWorks, Inc.

// This file is part of Gauge-CSharp.

// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// Gauge-Ruby is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    [TestFixture]
    public class DataStoreTests
    {
        private DataStore _dataStore;
        [SetUp]
        public void Setup()
        {
            _dataStore = new DataStore();
            _dataStore.Initialize();
        }
        [Test]
        public void ShouldInitializeDataStore()
        {
            Assert.AreEqual(_dataStore.Count, 0);
        }

        [Test]
        public void ShouldInsertValueIntoDataStore()
        {
            _dataStore.Add("foo", 23);

            Assert.AreEqual(_dataStore.Count, 1);
            Assert.AreEqual(_dataStore.Get("foo"), 23);
        }

        [Test]
        public void ShouldInsertComplexTypeIntoDataStore()
        {
            _dataStore.Add("bar", new {Name = "Hello", Country = "India"});
            var value = _dataStore.Get("bar") as dynamic;

            Assert.AreEqual(value.Name, "Hello");
        }

        [Test]
        public void ShouldUpdateDataForGivenKey()
        {
            _dataStore.Add("foo", "bar");
            _dataStore.Add("foo", "rumpelstiltskin");

            var value = _dataStore.Get("foo");

            Assert.AreEqual(value, "rumpelstiltskin");
        }

        [Test]
        public void ShouldClearDataStore()
        {
            _dataStore.Add("fruit", "apple");
            _dataStore.Clear();

            Assert.AreEqual(_dataStore.Count, 0);
        }
    }
}