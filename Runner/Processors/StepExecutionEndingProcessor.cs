﻿// Copyright 2015 ThoughtWorks, Inc.
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

using System.Collections.Generic;
using System.Linq;
using Gauge.Messages;
using Google.Protobuf;

namespace Gauge.CSharp.Runner.Processors
{
    public class StepExecutionEndingProcessor : TaggedHooksFirstExecutionProcessor
    {
        public StepExecutionEndingProcessor(IMethodExecutor methodExecutor)
            : base(methodExecutor)
        {
        }

        protected override string HookType => "AfterStep";

        protected override ProtoExecutionResult ExecuteHooks(Message request)
        {
            var protoExecutionResult = base.ExecuteHooks(request);
            var allPendingMessages = MethodExecutor.GetAllPendingMessages().Where(m => m != null);
            var allPendingScreenshots = MethodExecutor.GetAllPendingScreenshots().Select(x => ByteString.CopyFrom(x));
            protoExecutionResult.Message.AddRange(allPendingMessages);
            protoExecutionResult.ScreenShot.AddRange(allPendingScreenshots);
            return protoExecutionResult;
        }

        protected override ExecutionInfo GetExecutionInfo(Message request)
        {
            return request.StepExecutionEndingRequest.CurrentExecutionInfo;
        }

        protected override List<string> GetApplicableTags(Message request)
        {
            return GetExecutionInfo(request).CurrentScenario.Tags
                .Union(GetExecutionInfo(request).CurrentSpec.Tags).ToList();
        }
    }
}