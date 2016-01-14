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

using System.Collections.Generic;
using System.Linq;
using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public class StepExecutionStartingProcessor : UntaggedHooksFirstExecutionProcessor
    {
        public StepExecutionStartingProcessor(IHookRegistry hookRegistry, IMethodExecutor methodExecutor)
            : base(hookRegistry, methodExecutor)
        {
        }

        protected override HashSet<HookMethod> GetHooks()
        {
            return Hooks.BeforeStepHooks;
        }

        protected override ExecutionInfo GetExecutionInfo(Message request)
        {
            return request.StepExecutionStartingRequest.CurrentExecutionInfo;
        }

        protected override ProtoExecutionResult.Builder ExecuteHooks(Message request)
        {
            // Just need to clear the messages, but Gauge.CSharp.Lib v0.5.2 does not have MessageCollector.Clear()
            MethodExecutor.GetAllPendingMessages();
            return base.ExecuteHooks(request);
        }

        protected override IEnumerable<string> GetApplicableTags(Message request)
        {
            return GetExecutionInfo(request).CurrentScenario.TagsList
                .Union(GetExecutionInfo(request).CurrentSpec.TagsList);
        }
    }
}