// Copyright 2015 ThoughtWorks, Inc.

// This file is part of Gauge-CSharp.

// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// Gauge-Ruby is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Reflection;
using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public abstract class HookExecutionProcessor : ExecutionProcessor, IMessageProcessor
    {
        private readonly IMethodExecutor _methodExecutor;
        private readonly ISandbox _sandbox;
        protected IHookRegistry Hooks { get; private set; }

        protected HookExecutionProcessor(IHookRegistry hookRegistry, IMethodExecutor methodExecutor, ISandbox sandbox)
        {
            _methodExecutor = methodExecutor;
            _sandbox = sandbox;
            Hooks = hookRegistry;
        }

        protected HookExecutionProcessor(IHookRegistry hookRegistry, ISandbox sandbox)
            : this(hookRegistry, new MethodExecutor(), sandbox)
        {
        }


        protected abstract IEnumerable<MethodInfo> GetHooks();

        public Message Process(Message request)
        {
            var currentExecutionInfo = GetExecutionInfo(request);
            var hooks = GetHooks();
            var protoExecutionResult = _methodExecutor.ExecuteHooks(hooks, currentExecutionInfo);
            return WrapInMessage(protoExecutionResult, request);
        }

        protected abstract ExecutionInfo GetExecutionInfo(Message request);
    }
}