using System.Collections.Generic;
using System.Reflection;
using main;

namespace Gauge.CSharp.Runner.Processors
{
    internal class ScenarioExecutionStartingProcessor : HookExecutionProcessor
    {
        public ScenarioExecutionStartingProcessor(HookRegistry hookRegistry) : base(hookRegistry)
        {
        }

        protected override IEnumerable<MethodInfo> GetHooks()
        {
            return Hooks.BeforeScenarioHooks;
        }

        protected override ExecutionInfo GetExecutionInfo(Message request)
        {
            return request.ScenarioExecutionStartingRequest.CurrentExecutionInfo;
        }
    }
}