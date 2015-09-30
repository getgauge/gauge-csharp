// Copyright 2015 ThoughtWorks, Inc.

// This file is part of Gauge-CSharp.

// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

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
            var errorMessage = isValid ? string.Empty : string.Format("No implementation found for : {0}. Full Step Text :", stepToValidate);
            return GetStepValidateResponseMessage(isValid, request, StepValidateResponse.Types.ErrorType.STEP_IMPLEMENTATION_NOT_FOUND,  errorMessage);
        }

        private static Message GetStepValidateResponseMessage(bool isValid, Message request, StepValidateResponse.Types.ErrorType errorType, string errorMessage)
        {
            var stepValidateResponse = StepValidateResponse.CreateBuilder()
                                                    .SetErrorMessage(errorMessage)
                                                    .SetIsValid(isValid)
                                                    .SetErrorType(errorType)
                                                    .Build();
            return Message.CreateBuilder()
                    .SetMessageId(request.MessageId)
                    .SetMessageType(Message.Types.MessageType.StepValidateResponse)
                    .SetStepValidateResponse(stepValidateResponse)
                    .Build();
        }
    }
}