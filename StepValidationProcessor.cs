using System;
using System.Collections;
using main;

namespace gauge_csharp
{
    internal class StepValidationProcessor : IMessageProcessor
    {
        private readonly StepRegistry _stepMethodTable;

        public StepValidationProcessor(StepRegistry stepMethodTable)
        {
            _stepMethodTable = stepMethodTable;
        }

        public Message Process(Message request)
        {
            string stepToValidate = request.StepValidateRequest.StepText;
            bool isValid = _stepMethodTable.ContainsStep(stepToValidate);
            return GetStepValidateResponseMessage(isValid,request);
        }

        private static Message GetStepValidateResponseMessage(bool isValid, Message request)
        {
            StepValidateResponse stepValidateResponse = StepValidateResponse.CreateBuilder().SetIsValid(isValid).Build();
            return
                Message.CreateBuilder()
                    .SetMessageId(request.MessageId)
                    .SetMessageType(Message.Types.MessageType.StepValidateResponse)
                    .SetStepValidateResponse(stepValidateResponse)
                    .Build();
        }

    }
}