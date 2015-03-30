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