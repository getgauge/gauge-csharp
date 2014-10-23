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