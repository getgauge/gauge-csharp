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

        protected abstract HashSet<HookMethod> GetHooks();

        [DebuggerHidden]
        public Message Process(Message request)
        {
            var applicableTags = GetApplicableTags(request);
            var applicableHooks = GetApplicableHooks(applicableTags, GetHooks());
            var protoExecutionResult = _methodExecutor.ExecuteHooks(applicableHooks, GetExecutionInfo(request));
            return WrapInMessage(protoExecutionResult, request);
        }

        private List<string> GetApplicableTags(Message request)
        {
            return GetExecutionInfo(request).CurrentScenario.TagsList.Union(GetExecutionInfo(request).CurrentSpec.TagsList).ToList();
        }

        public static IEnumerable<MethodInfo> GetApplicableHooks(List<string> applicableTags, IEnumerable<HookMethod> hooks)
        {
            var hookMethods = hooks as IList<HookMethod> ?? hooks.ToList();
            var alwaysExecuteHooks = hookMethods.Where(method => method.FilterTags == null || !method.FilterTags.Any() ).Select(method => method.Method);
            return applicableTags.Any() ? alwaysExecuteHooks.Union(GetFilteredHooks(applicableTags, hookMethods)) : alwaysExecuteHooks;
        }

        public static IEnumerable<MethodInfo> GetFilteredHooks(IEnumerable<string> applicableTags, IEnumerable<HookMethod> hooks)
        {
            var tagsList = applicableTags.ToList();
            return from hookMethod in hooks.ToList()
                where hookMethod.FilterTags != null
                where
                    hookMethod.TagAggregation == TagAggregation.Or && hookMethod.FilterTags.Intersect(tagsList).Any() ||
                    hookMethod.TagAggregation == TagAggregation.And && hookMethod.FilterTags.All(tagsList.Contains)
                select hookMethod.Method;
        }

        protected abstract ExecutionInfo GetExecutionInfo(Message request);
    }
}