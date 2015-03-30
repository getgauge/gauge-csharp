using System.Collections.Generic;
using System.Reflection;
using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public class StepExecutionStartingProcessor : HookExecutionProcessor
    {
        public StepExecutionStartingProcessor(IHookRegistry hookRegistry) : base(hookRegistry, new Sandbox())
        {
        }

        public StepExecutionStartingProcessor(IHookRegistry hookRegistry, ISandbox sandbox)
            : base(hookRegistry, new MethodExecutor(sandbox), sandbox)
        {
        }

        protected override IEnumerable<MethodInfo> GetHooks()
        {
            return Hooks.BeforeStepHooks;
        }

        protected override ExecutionInfo GetExecutionInfo(Message request)
        {
            return request.StepExecutionStartingRequest.CurrentExecutionInfo;
        }
    }
}