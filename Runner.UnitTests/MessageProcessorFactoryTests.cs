using Gauge.CSharp.Runner.Processors;
using Gauge.Messages;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    [TestFixture]
    public class MessageProcessorFactoryTests
    {
        private MessageProcessorFactory _messageProcessorFactory;

        [SetUp]
        public void Setup()
        {
            var mockMethodScanner = new Mock<IMethodScanner>();
            var mockStepRegistry = new Mock<IStepRegistry>();
            var mockHookRegistry = new Mock<IHookRegistry>();
            mockMethodScanner.Setup(x => x.GetHookRegistry()).Returns(mockHookRegistry.Object);
            mockMethodScanner.Setup(x => x.GetStepRegistry()).Returns(mockStepRegistry.Object);
            _messageProcessorFactory = new MessageProcessorFactory(mockMethodScanner.Object);
        }

        [Test]
        public void ShouldGetProcessorForExecutionStarting()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.ExecutionStarting);

            Assert.AreEqual(messageProcessor.GetType(), typeof(ExecutionStartingProcessor));
        }

        [Test]
        public void ShouldGetProcessorForExecutionEnding()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.ExecutionEnding);

            Assert.AreEqual(messageProcessor.GetType(), typeof(ExecutionEndingProcessor));
        }

        [Test]
        public void ShouldGetProcessorForStepExecutionStarting()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.StepExecutionStarting);

            Assert.AreEqual(messageProcessor.GetType(), typeof(StepExecutionStartingProcessor));
        }

        [Test]
        public void ShouldGetProcessorForStepExecutionEnding()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.StepExecutionEnding);

            Assert.AreEqual(messageProcessor.GetType(), typeof(StepExecutionEndingProcessor));
        }

        [Test]
        public void ShouldGetProcessorForSpecExecutionStarting()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.SpecExecutionStarting);

            Assert.AreEqual(messageProcessor.GetType(), typeof(SpecExecutionStartingProcessor));
        }

        [Test]
        public void ShouldGetProcessorForSpecExecutionEnding()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.SpecExecutionEnding);

            Assert.AreEqual(messageProcessor.GetType(), typeof(SpecExecutionEndingProcessor));
        }

        [Test]
        public void ShouldGetProcessorForScenarioExecutionStarting()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.ScenarioExecutionStarting);

            Assert.AreEqual(messageProcessor.GetType(), typeof(ScenarioExecutionStartingProcessor));
        }

        [Test]
        public void ShouldGetProcessorForScenarioExecutionEnding()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.ScenarioExecutionEnding);

            Assert.AreEqual(messageProcessor.GetType(), typeof(ScenarioExecutionEndingProcessor));
        }

        [Test]
        public void ShouldGetProcessorForExecuteStep()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.ExecuteStep);

            Assert.AreEqual(messageProcessor.GetType(), typeof(ExecuteStepProcessor));
        }

        [Test]
        public void ShouldGetProcessorForKillProcessRequest()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.KillProcessRequest);

            Assert.AreEqual(messageProcessor.GetType(), typeof(KillProcessProcessor));
        }

        [Test]
        public void ShouldGetProcessorForStepNamesRequest()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.StepNamesRequest);

            Assert.AreEqual(messageProcessor.GetType(), typeof(StepNamesProcessor));
        }

        [Test]
        public void ShouldGetProcessorForStepValidateRequest()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.StepValidateRequest);

            Assert.AreEqual(messageProcessor.GetType(), typeof(StepValidationProcessor));
        }

        [Test]
        public void ShouldGetProcessorForSpecDataStoreInitRequest()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.SpecDataStoreInit);

            Assert.AreEqual(messageProcessor.GetType(), typeof(DataStoreInitProcessor));
        }

        [Test]
        public void ShouldGetProcessorForSuiteDataStoreInitRequest()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.SuiteDataStoreInit);

            Assert.AreEqual(messageProcessor.GetType(), typeof(DataStoreInitProcessor));
        }

        [Test]
        public void ShouldGetProcessorForScenarioDataStoreInitRequest()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.ScenarioDataStoreInit);

            Assert.AreEqual(messageProcessor.GetType(), typeof(DataStoreInitProcessor));
        }
    }
}