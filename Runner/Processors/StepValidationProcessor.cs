// Copyright 2015 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using Gauge.CSharp.Runner.Models;
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
            var isValid = true;
            var errorMessage = "";
            var errorType = StepValidateResponse.Types.ErrorType.StepImplementationNotFound;
            if (!_stepMethodTable.ContainsStep(stepToValidate))
            {
                isValid = false;
                errorMessage = string.Format("No implementation found for : {0}. Full Step Text :", stepToValidate);
            }
            else if (_stepMethodTable.HasMultipleImplementations(stepToValidate))
            {
                isValid = false;
                errorType = StepValidateResponse.Types.ErrorType.DuplicateStepImplementation;
                errorMessage = string.Format("Multiple step implementations found for : {0}", stepToValidate);
            }
            return GetStepValidateResponseMessage(isValid, request, errorType,  errorMessage);
        }

        private static Message GetStepValidateResponseMessage(bool isValid, Message request, StepValidateResponse.Types.ErrorType errorType, string errorMessage)
        {
            var stepValidateResponse = new StepValidateResponse()
            {
                ErrorMessage = errorMessage,
                IsValid = isValid,
                ErrorType = errorType,
            };
            return new Message()
            {
                MessageId = request.MessageId,
                MessageType = Message.Types.MessageType.StepValidateResponse,
                StepValidateResponse = stepValidateResponse
            };
        }
    }
}