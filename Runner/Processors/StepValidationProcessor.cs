using System;
using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public class StepValidationProcessor : IMessageProcessor
    {
        private readonly IStepRegistry _stepMethodTable;

        public StepValidationProcessor(IStepRegistry stepMethodTable)
        {
            _stepMethodTable = stepMethodTable;
        }

        public Message Process(Message request)
        {
            var stepToValidate = request.StepValidateRequest.StepText;
            var isValid = _stepMethodTable.ContainsStep(stepToValidate);
            return GetStepValidateResponseMessage(isValid, request);
        }

        private static Message GetStepValidateResponseMessage(bool isValid, Message request)
        {
            var stepValidateResponse = StepValidateResponse.CreateBuilder().SetIsValid(isValid).Build();
            return
                Message.CreateBuilder()
                    .SetMessageId(request.MessageId)
                    .SetMessageType(Message.Types.MessageType.StepValidateResponse)
                    .SetStepValidateResponse(stepValidateResponse)
                    .Build();
        }
    }
}