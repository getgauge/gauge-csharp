using Gauge.CSharp.Runner.Processors;
using Gauge.Messages;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    [TestFixture]
    public class DefaultProcessorTests
    {
        [Test]
        public void ShouldProcessMessage()
        {
            var request = Message.CreateBuilder()
                .SetMessageId(20)
                .SetMessageType(Message.Types.MessageType.ExecuteStep)
                .Build();
            
            var response = new DefaultProcessor().Process(request);
            var executionStatusResponse = response.ExecutionStatusResponse;
            
            Assert.AreEqual(response.MessageId, 20);
            Assert.AreEqual(response.MessageType, Message.Types.MessageType.ExecutionStatusResponse);
            Assert.AreEqual(executionStatusResponse.ExecutionResult.ExecutionTime, 0);
        }
    }
}