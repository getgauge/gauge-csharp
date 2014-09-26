using System.Collections.Generic;
using System.Reflection;
using main;

namespace Gauge.CSharp.Runner.Processors
{
    internal class ScenarioExecutionEndingProcessor : HookExecutionProcessor
    {
        public ScenarioExecutionEndingProcessor(HookRegistry hookRegistry) : base(hookRegistry)
        {
        }

        protected override IEnumerable<MethodInfo> GetHooks()
        {
            return Hooks.AfterScenarioHooks;
        }

        protected override ExecutionInfo GetExecutionInfo(Message request)
        {
            return request.ScenarioExecutionEndingRequest.CurrentExecutionInfo;
        }
    }
}