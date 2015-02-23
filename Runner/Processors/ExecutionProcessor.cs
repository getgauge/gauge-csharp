using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public class ExecutionProcessor
    {
        protected static Message WrapInMessage(ProtoExecutionResult executionResult, Message request)
        {
            var executionStatusResponse = ExecutionStatusResponse.CreateBuilder()
                .SetExecutionResult(executionResult)
                .Build();
            return Message.CreateBuilder()
                .SetMessageId(request.MessageId)
                .SetMessageType(Message.Types.MessageType.ExecutionStatusResponse)
                .SetExecutionStatusResponse(executionStatusResponse)
                .Build();
        }
    }
}