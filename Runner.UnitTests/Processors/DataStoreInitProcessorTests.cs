using Gauge.CSharp.Lib;
using Gauge.CSharp.Runner.Processors;
using Gauge.Messages;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    [TestFixture]
    public class DataStoreInitProcessorTests
    {
        [Test]
        public void ShouldGetScenarioDataStoreType()
        {
            var dataStoreType = DataStoreInitProcessor.GetDataStoreType(Message.Types.MessageType.ScenarioDataStoreInit);
            Assert.AreEqual(DataStoreType.Scenario, dataStoreType);
        }

        [Test]
        public void ShouldGetSpecDataStoreType()
        {
            var dataStoreType = DataStoreInitProcessor.GetDataStoreType(Message.Types.MessageType.SpecDataStoreInit);
            Assert.AreEqual(DataStoreType.Specification, dataStoreType);
        }

        [Test]
        public void ShouldGetSuiteDataStoreType()
        {
            var dataStoreType = DataStoreInitProcessor.GetDataStoreType(Message.Types.MessageType.SuiteDataStoreInit);
            Assert.AreEqual(DataStoreType.Suite, dataStoreType);
        }
    }
}
