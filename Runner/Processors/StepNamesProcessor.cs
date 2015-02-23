using System.Collections.Generic;
using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public class StepNamesProcessor : IMessageProcessor
    {
        private readonly IStepRegistry _stepRegistry;

        public StepNamesProcessor(IStepRegistry stepRegistry)
        {
            _stepRegistry = stepRegistry;
        }

        public Message Process(Message request)
        {
            var allSteps = _stepRegistry.AllSteps();
            return GetStepNamesResponseMessage(allSteps, request);
        }

        private static Message GetStepNamesResponseMessage(IEnumerable<string> allSteps, Message request)
        {
            var stepNamesResponse = StepNamesResponse.CreateBuilder().AddRangeSteps(allSteps).Build();
            return Message.CreateBuilder()
                .SetMessageId(request.MessageId)
                .SetMessageType(Message.Types.MessageType.StepNamesResponse)
                .SetStepNamesResponse(stepNamesResponse).Build();
        }
    }
}