using main;

namespace Gauge.CSharp.Runner.Processors
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
            var stepToValidate = request.StepValidateRequest.StepText;
            var isValid = _stepMethodTable.ContainsStep(stepToValidate);
            return GetStepValidateResponseMessage(isValid,request);
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