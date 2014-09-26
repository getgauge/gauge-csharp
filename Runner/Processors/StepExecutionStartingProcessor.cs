using System.Collections.Generic;
using System.Reflection;
using main;

namespace Gauge.CSharp.Runner.Processors
{
    internal class StepExecutionStartingProcessor : HookExecutionProcessor
    {
        public StepExecutionStartingProcessor(HookRegistry hookRegistry) : base(hookRegistry)
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