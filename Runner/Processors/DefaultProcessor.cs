using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public class DefaultProcessor : IMessageProcessor
    {
        public Message Process(Message request)
        {
            return GetResponseMessage(request);
        }

        private static Message GetResponseMessage(Message request)
        {
            var executionStatusResponseBuilder = ExecutionStatusResponse.CreateBuilder();
            var executionStatusResponse =
                executionStatusResponseBuilder.SetExecutionResult(
                    ProtoExecutionResult.CreateBuilder().SetFailed(false).SetExecutionTime(0)).Build();
            return Message.CreateBuilder()
                .SetMessageId(request.MessageId)
                .SetMessageType(Message.Types.MessageType.ExecutionStatusResponse)
                .SetExecutionStatusResponse(executionStatusResponse)
                .Build();
        }
    }
}