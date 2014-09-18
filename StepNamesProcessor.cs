using System.Collections;
using System.Collections.Generic;
using main;

namespace gauge_csharp
{
    internal class StepNamesProcessor : IMessageProcessor
    {
        private readonly StepRegistry _stepRegistry;

        public StepNamesProcessor(StepRegistry stepRegistry)
        {
            _stepRegistry = stepRegistry;
        }

        public Message Process(Message request)
        {
            var allSteps = _stepRegistry.AllSteps();
            return GetStepNamesResponseMessage(allSteps,request);
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