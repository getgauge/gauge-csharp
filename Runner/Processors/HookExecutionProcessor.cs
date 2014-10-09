using System.Collections.Generic;
using System.Reflection;
using main;

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

        protected HookExecutionProcessor(IHookRegistry hookRegistry) : this(hookRegistry, new MethodExecutor())
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