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
            var errorMessage = isValid ? string.Empty : string.Format("No implementation found for: {0}", stepToValidate);
            return GetStepValidateResponseMessage(isValid, request, errorMessage);
        }

        private static Message GetStepValidateResponseMessage(bool isValid, Message request, string errorMessage)
        {
            var stepValidateResponse = StepValidateResponse.CreateBuilder()
                                                    .SetErrorMessage(errorMessage)
                                                    .SetIsValid(isValid)
                                                    .Build();
            return Message.CreateBuilder()
                    .SetMessageId(request.MessageId)
                    .SetMessageType(Message.Types.MessageType.StepValidateResponse)
                    .SetStepValidateResponse(stepValidateResponse)
                    .Build();
        }
    }
}