using System.Collections.Generic;
using System.Reflection;
using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public class StepExecutionEndingProcessor : HookExecutionProcessor
    {
        public StepExecutionEndingProcessor(IHookRegistry hookRegistry) : base(hookRegistry, new Sandbox())
        {
        }
        public StepExecutionEndingProcessor(IHookRegistry hookRegistry, ISandbox sandbox)
            : base(hookRegistry, new MethodExecutor(sandbox), sandbox)
        {
        }

        protected override IEnumerable<MethodInfo> GetHooks()
        {
            return Hooks.AfterStepHooks;
        }

        protected override ExecutionInfo GetExecutionInfo(Message request)
        {
            return request.StepExecutionEndingRequest.CurrentExecutionInfo;
        }
    }
}