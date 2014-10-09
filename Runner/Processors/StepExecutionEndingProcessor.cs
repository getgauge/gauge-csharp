using System.Collections.Generic;
using System.Reflection;
using main;

namespace Gauge.CSharp.Runner.Processors
{
    public class StepExecutionEndingProcessor : HookExecutionProcessor
    {
        public StepExecutionEndingProcessor(IHookRegistry hookRegistry) : base(hookRegistry)
        {
        }

        protected override IEnumerable<MethodInfo> GetHooks()
        {
            return Hooks.BeforeStepHooks;
        }

        protected override ExecutionInfo GetExecutionInfo(Message request)
        {
            return request.StepExecutionEndingRequest.CurrentExecutionInfo;
        }
    }
}