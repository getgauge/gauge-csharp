using Gauge.Messages;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    [TestFixture]
    public class DataStoreFactoryTests
    {
        [Test]
        public void ShouldGetDataStoreForSuite()
        {
            var dataStore = DataStoreFactory.GetDataStoreFor(Message.Types.MessageType.SuiteDataStoreInit);

            Assert.NotNull(dataStore);
            Assert.IsInstanceOf<DataStore>(dataStore);
        }

        [Test]
        public void ShouldGetDataStoreForSpec()
        {
            var dataStore = DataStoreFactory.GetDataStoreFor(Message.Types.MessageType.SpecDataStoreInit);

            Assert.NotNull(dataStore);
            Assert.IsInstanceOf<DataStore>(dataStore);
        }

        [Test]
        public void ShouldGetDataStoreForScenario()
        {
            var dataStore = DataStoreFactory.GetDataStoreFor(Message.Types.MessageType.ScenarioDataStoreInit);

            Assert.NotNull(dataStore);
            Assert.IsInstanceOf<DataStore>(dataStore);
        }
    }
}