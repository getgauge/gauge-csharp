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
using System.Diagnostics;
using System.Linq;
using Gauge.CSharp.Core;
using Gauge.CSharp.Runner.Strategy;
using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public abstract class HookExecutionProcessor : ExecutionProcessor, IMessageProcessor
    {
        private const string ClearStateFlag = "gauge_clear_state_level";
        protected const string SuiteLevel = "suite";
        protected const string SpecLevel = "spec";
        protected const string ScenarioLevel = "scenario";
        protected readonly IMethodExecutor MethodExecutor;

        protected HookExecutionProcessor(IMethodExecutor methodExecutor)
        {
            MethodExecutor = methodExecutor;
            Strategy = new HooksStrategy();
        }

        protected HooksStrategy Strategy { get; set; }

        protected abstract string HookType { get; }

        protected virtual string CacheClearLevel => null;

        [DebuggerHidden]
        public virtual Message Process(Message request)
        {
            var protoExecutionResult = ExecuteHooks(request);
            ClearCacheForConfiguredLevel();
            return WrapInMessage(protoExecutionResult, request);
        }

        protected abstract ExecutionInfo GetExecutionInfo(Message request);

        protected virtual ProtoExecutionResult ExecuteHooks(Message request)
        {
            var applicableTags = GetApplicableTags(request);
            var mapper = new ExecutionInfoMapper();
            var executionContext = mapper.ExecutionInfoFrom(GetExecutionInfo(request));
            return MethodExecutor.ExecuteHooks(HookType, Strategy, applicableTags, executionContext);        }

        private void ClearCacheForConfiguredLevel()
        {
            var flag = Utils.TryReadEnvValue(ClearStateFlag);
            if (!string.IsNullOrEmpty(flag) && flag.Trim().Equals(CacheClearLevel))
                MethodExecutor.ClearCache();
        }

        protected virtual List<string> GetApplicableTags(Message request)
        {
            return Enumerable.Empty<string>().ToList();
        }
    }
}