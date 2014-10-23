using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    [TestFixture]
    public class DataStoreTests
    {
        [SetUp]
        public void Setup()
        {
            DataStore.Initialize();
        }
        [Test]
        public void ShouldInitializeDataStore()
        {
            Assert.AreEqual(DataStore.Count, 0);
        }

        [Test]
        public void ShouldInsertValueIntoDataStore()
        {
            DataStore.Add("foo", 23);

            Assert.AreEqual(DataStore.Count, 1);
            Assert.AreEqual(DataStore.Get("foo"), 23);
        }

        [Test]
        public void ShouldInsertComplexTypeIntoDataStore()
        {
            DataStore.Add("bar", new {Name = "Hello", Country = "India"});
            var value = DataStore.Get("bar") as dynamic;

            Assert.AreEqual(value.Name, "Hello");
        }

        [Test]
        public void ShouldUpdateDataForGivenKey()
        {
            DataStore.Add("foo", "bar");
            DataStore.Add("foo", "rumpelstiltskin");

            var value = DataStore.Get("foo");

            Assert.AreEqual(value, "rumpelstiltskin");
        }

        [Test]
        public void ShouldClearDataStore()
        {
            DataStore.Add("fruit", "apple");
            DataStore.Clear();

            Assert.AreEqual(DataStore.Count, 0);
        }
    }
}