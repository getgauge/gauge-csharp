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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Gauge.CSharp.Lib.Attribute;
using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public abstract class HookExecutionProcessor : ExecutionProcessor, IMessageProcessor
    {
        private readonly IMethodExecutor _methodExecutor;
        protected IHookRegistry Hooks { get; private set; }

        protected HookExecutionProcessor(IHookRegistry hookRegistry, IMethodExecutor methodExecutor)
        {
            _methodExecutor = methodExecutor;
            Hooks = hookRegistry;
        }

        protected HookExecutionProcessor(IHookRegistry hookRegistry)
            : this(hookRegistry, new MethodExecutor())
        {
        }


        protected abstract HashSet<HookMethod> GetHooks();

        [DebuggerHidden]
        public Message Process(Message request)
        {
            var currentExecutionInfo = GetExecutionInfo(request);
            var applicableTags = currentExecutionInfo.CurrentScenario.TagsList.Union(currentExecutionInfo.CurrentSpec.TagsList).ToList();
            var hooks = GetHooks();
            List<MethodInfo> applicableHooks;
            if (applicableTags.Any())
            {
                applicableHooks = new List<MethodInfo>();
                foreach (var hookMethod in hooks)
                {
                    if (hookMethod.TagAggregation==TagAggregation.Or)
                    {
                        if (hookMethod.FilterTags.Intersect(applicableTags).Any())
                        {
                            applicableHooks.Add(hookMethod.Method);
                        }
                    }
                    else
                    {
                        if (hookMethod.FilterTags.Intersect(applicableTags).Count().Equals(applicableTags.Count))
                        {
                            applicableHooks.Add(hookMethod.Method);
                        }
                    }
                }                                 
            }
            var protoExecutionResult = _methodExecutor.ExecuteHooks(applicableHooks, currentExecutionInfo);
            return WrapInMessage(protoExecutionResult, request);
        }

        protected abstract ExecutionInfo GetExecutionInfo(Message request);
    }
}