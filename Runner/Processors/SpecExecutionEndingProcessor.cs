using System.Collections.Generic;
using System.Reflection;
using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public class SpecExecutionEndingProcessor : HookExecutionProcessor
    {
        public SpecExecutionEndingProcessor(IHookRegistry hookRegistry) : base(hookRegistry, new Sandbox())
        {
        }

        public SpecExecutionEndingProcessor(IHookRegistry hookRegistry, ISandbox sandbox)
            : base(hookRegistry, new MethodExecutor(sandbox), sandbox)
        {
        }

        protected override IEnumerable<MethodInfo> GetHooks()
        {
            return Hooks.AfterSpecHooks;
        }

        protected override ExecutionInfo GetExecutionInfo(Message request)
        {
            return request.SpecExecutionEndingRequest.CurrentExecutionInfo;
        }
    }
}