using System.Collections.Generic;
using System.Reflection;
using main;

namespace Gauge.CSharp.Runner.Processors
{
    internal class StepExecutionEndingProcessor : HookExecutionProcessor
    {
        public StepExecutionEndingProcessor(HookRegistry hookRegistry) : base(hookRegistry)
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