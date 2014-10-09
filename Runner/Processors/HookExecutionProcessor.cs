using System.Collections.Generic;
using System.Reflection;
using main;

namespace Gauge.CSharp.Runner.Processors
{
    public abstract class HookExecutionProcessor : ExecutionProcessor, IMessageProcessor
    {
        protected IHookRegistry Hooks { get; private set; }

        protected HookExecutionProcessor(IHookRegistry hookRegistry)
        {
            Hooks = hookRegistry;
        }

        protected abstract IEnumerable<MethodInfo> GetHooks();

        public Message Process(Message request)
        {
            var currentExecutionInfo = GetExecutionInfo(request);
            var hooks = GetHooks();
            var protoExecutionResult = new MethodExecutor().ExecuteHooks(hooks, currentExecutionInfo);
            return WrapInMessage(protoExecutionResult, request);
        }

        protected abstract ExecutionInfo GetExecutionInfo(Message request);
    }
}