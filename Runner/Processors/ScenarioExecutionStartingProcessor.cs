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

namespace Gauge.CSharp.Runner.Processors
{
    public class ScenarioExecutionStartingProcessor : UntaggedHooksFirstExecutionProcessor
    {
        private readonly ISandbox _sandbox;


        public ScenarioExecutionStartingProcessor(IHookRegistry hookRegistry, IMethodExecutor methodExecutor, ISandbox sandbox)
            : base(hookRegistry, methodExecutor)
        {
            _sandbox = sandbox;
        }

        public override Message Process(Message request)
        {
            _sandbox.StartExecutionScope("scenario");
            return base.Process(request);
        }

        protected override HashSet<HookMethod> GetHooks()
        {
            return Hooks.BeforeScenarioHooks;
        }

        protected override ExecutionInfo GetExecutionInfo(Message request)
        {
            return request.ScenarioExecutionStartingRequest.CurrentExecutionInfo;
        }

        protected override IEnumerable<string> GetApplicableTags(Message request)
        {
            return GetExecutionInfo(request).CurrentScenario.TagsList
                .Union(GetExecutionInfo(request).CurrentSpec.TagsList);
        }
    }
}