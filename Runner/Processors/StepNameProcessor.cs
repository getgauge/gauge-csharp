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
    internal class StepNameProcessor : IMessageProcessor
    {
        private readonly IStepRegistry _stepRegistry;

        public StepNameProcessor(IStepRegistry stepRegistry)
        {
            _stepRegistry = stepRegistry;
        }

        public Message Process(Message request)
        {
            var parsedStepText = request.StepNameRequest.StepValue;
            var isValidStep = _stepRegistry.ContainsStep(parsedStepText);
            var stepText = _stepRegistry.GetStepText(parsedStepText);
            var hasAlias = _stepRegistry.HasAlias(stepText);

            var stepNameResponse = new StepNameResponse()
            {
                HasAlias = hasAlias,
                IsStepPresent = isValidStep,
                StepName = {stepText},
            };
            return new Message()
            {
                MessageId = request.MessageId,
                MessageType = Message.Types.MessageType.StepNameResponse,
                StepNameResponse = stepNameResponse,
            };
        }
    }
}