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
using NUnit.Framework;

namespace Gauge.CSharp.Lib.UnitTests
{
    [TestFixture]
    public class DataStoreTests
    {
        [SetUp]
        public void Setup()
        {
            _dataStore = new DataStore();
            _dataStore.Initialize();
        }

        public class Fruit
        {
            public string Name { get; set; }
        }

        private DataStore _dataStore;

        [Test]
        public void ShouldClearDataStore()
        {
            _dataStore.Add("fruit", "apple");
            _dataStore.Clear();

            Assert.AreEqual(_dataStore.Count, 0);
        }

        [Test]
        public void ShouldGetNullWhenKeyDoesNotExist()
        {
            _dataStore.Add("fruit", "banana");
            var fruit = _dataStore.Get("banana");

            Assert.IsNull(fruit);
        }

        [Test]
        public void ShouldGetStrongTypedValue()
        {
            _dataStore.Add("banana", new Fruit {Name = "Banana"});
            var fruit = _dataStore.Get<Fruit>("banana");

            Assert.IsInstanceOf<Fruit>(fruit);
            Assert.AreEqual("Banana", fruit.Name);
        }

        [Test]
        public void ShouldInitializeDataStore()
        {
            Assert.AreEqual(_dataStore.Count, 0);
        }

        public class Sample
        {
            public string Name { get; set; }
            public string Country { get; set; }
        }

        [Test]
        public void ShouldInsertComplexTypeIntoDataStore()
        {

            _dataStore.Add("bar", new Sample {Name = "Hello", Country = "India"});
            var value = _dataStore.Get("bar") as Sample;

            Assert.AreEqual(value.Name, "Hello");
        }

        [Test]
        public void ShouldInsertValueIntoDataStore()
        {
            _dataStore.Add("foo", 23);

            Assert.AreEqual(_dataStore.Count, 1);
            Assert.AreEqual(_dataStore.Get("foo"), 23);
        }

        [Test]
        public void ShouldRaiseInvalidCastExceptionWhenAskingForInvalidCast()
        {
            _dataStore.Add("banana", new Fruit {Name = "Banana"});

            Assert.Throws<InvalidCastException>(() => { _dataStore.Get<string>("banana"); });
        }

        [Test]
        public void ShouldReturnNullWhenAskingForInvalidKeyWithStrongType()
        {
            _dataStore.Add("banana", new Fruit {Name = "Banana"});

            var fruit = _dataStore.Get<Fruit>("random");

            Assert.IsNull(fruit);
        }

        [Test]
        public void ShouldThrowArgumentNullExceptionWhenKeyIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => { _dataStore.Get(null); });
        }

        [Test]
        public void ShouldUpdateDataForGivenKey()
        {
            _dataStore.Add("foo", "bar");
            _dataStore.Add("foo", "rumpelstiltskin");

            var value = _dataStore.Get("foo");

            Assert.AreEqual(value, "rumpelstiltskin");
        }
    }
}