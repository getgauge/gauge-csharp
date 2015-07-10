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

using System.Collections.Generic;
using System.Linq;
using Gauge.Messages;
using Google.ProtocolBuffers;

namespace Gauge.CSharp.Runner.Communication
{
    public class GaugeApiConnection : AbstractGaugeConnection
    {
        public GaugeApiConnection(ITcpClientWrapper clientWrapper) : base(clientWrapper)
        {
        }

        public IEnumerable<string> GetStepValues(IEnumerable<string> stepTexts, bool hasInlineTable)
        {
            foreach (var stepText in stepTexts)
            {
                var stepValueRequest = GetStepValueRequest.CreateBuilder()
                    .SetStepText(stepText)
                    .SetHasInlineTable(hasInlineTable)
                    .Build();
                var stepValueRequestMessage = APIMessage.CreateBuilder()
                    .SetMessageId(GenerateMessageId())
                    .SetMessageType(APIMessage.Types.APIMessageType.GetStepValueRequest)
                    .SetStepValueRequest(stepValueRequest)
                    .Build();
                var apiMessage = WriteAndReadApiMessage(stepValueRequestMessage);
                yield return apiMessage.StepValueResponse.StepValue.StepValue;
            }
        }

        public APIMessage WriteAndReadApiMessage(IMessageLite stepValueRequestMessage)
        {
            lock (TcpClientWrapper)
            {
                WriteMessage(stepValueRequestMessage);
                return ReadMessage();
            }
        }

        private APIMessage ReadMessage()
        {
            var responseBytes = ReadBytes();
            return APIMessage.ParseFrom(responseBytes.ToArray());
        }
    }
}