using Gauge.CSharp.Runner.Processors;
using Gauge.Messages;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    [TestFixture]
    public class ExecuteStepProcessorTests
    {
        public void Foo(string param)
        {
        }

        [Test]
        public void ShouldReportMissingStep()
        {
            const string parsedStepText = "foo";
            var request = Message.CreateBuilder()
                .SetMessageType(Message.Types.MessageType.ExecuteStep)
                .SetExecuteStepRequest(
                    ExecuteStepRequest.CreateBuilder()
                        .SetActualStepText(parsedStepText)
                        .SetParsedStepText(parsedStepText)
                        .Build())
                .SetMessageId(20)
                .Build();
            var mockStepRegistry = new Mock<IStepRegistry>();
            mockStepRegistry.Setup(x => x.ContainsStep(parsedStepText)).Returns(false);

            var response = new ExecuteStepProcessor(mockStepRegistry.Object).Process(request);

            Assert.True(response.ExecutionStatusResponse.ExecutionResult.Failed);
            Assert.AreEqual(response.ExecutionStatusResponse.ExecutionResult.ErrorMessage,
                "Step Implementation not found");
        }

        [Test]
        public void ShouldReportArgumentMismatch()
        {
            const string parsedStepText = "foo";
            var request = Message.CreateBuilder()
                .SetMessageType(Message.Types.MessageType.ExecuteStep)
                .SetExecuteStepRequest(
                    ExecuteStepRequest.CreateBuilder()
                        .SetActualStepText(parsedStepText)
                        .SetParsedStepText(parsedStepText)
                        .Build())
                .SetMessageId(20)
                .Build();
            var mockStepRegistry = new Mock<IStepRegistry>();
            mockStepRegistry.Setup(x => x.ContainsStep(parsedStepText)).Returns(true);
            mockStepRegistry.Setup(x => x.MethodFor(parsedStepText)).Returns(GetType().GetMethod("Foo"));

            var response = new ExecuteStepProcessor(mockStepRegistry.Object).Process(request);
            
            Assert.True(response.ExecutionStatusResponse.ExecutionResult.Failed);
            Assert.AreEqual(response.ExecutionStatusResponse.ExecutionResult.ErrorMessage,
                "Argument length mismatch for foo. Actual Count: 0, Expected Count: 1");
        }

        [Test]
        public void ShouldProcessExecuteStepRequest()
        {
            const string parsedStepText = "foo";
            var request = Message.CreateBuilder()
                .SetMessageType(Message.Types.MessageType.ExecuteStep)
                .SetExecuteStepRequest(
                    ExecuteStepRequest.CreateBuilder()
                        .SetActualStepText(parsedStepText)
                        .SetParsedStepText(parsedStepText)
                        .AddParameters(
                            Parameter.CreateBuilder()
                                .SetParameterType(Parameter.Types.ParameterType.Static)
                                .SetName("foo")
                                .SetValue("bar")
                                .Build())
                        .Build())
                .SetMessageId(20)
                .Build();
            var mockStepRegistry = new Mock<IStepRegistry>();
            mockStepRegistry.Setup(x => x.ContainsStep(parsedStepText)).Returns(true);
            mockStepRegistry.Setup(x => x.MethodFor(parsedStepText)).Returns(GetType().GetMethod("Foo"));

            var response = new ExecuteStepProcessor(mockStepRegistry.Object).Process(request);

            Assert.False(response.ExecutionStatusResponse.ExecutionResult.Failed);
        }
    }
}