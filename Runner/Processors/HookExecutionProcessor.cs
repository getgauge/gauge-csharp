using System.Collections.Generic;
using System.Reflection;
using main;

namespace Gauge.CSharp.Runner.Processors
{
    public abstract class HookExecutionProcessor : ExecutionProcessor, IMessageProcessor
    {
        protected abstract HashSet<MethodInfo> GetHooks();

        public Message Process(Message request)
        {
            var currentExecutionInfo = request.ExecutionStartingRequest.CurrentExecutionInfo;
            var hooks = GetHooks();
            var protoExecutionResult = new MethodExecutor().ExecuteHooks(hooks, currentExecutionInfo);
            return WrapInMessage(protoExecutionResult, request);
        }
    }
}