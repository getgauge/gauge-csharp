using System.Collections.Generic;
using System.Reflection;
using main;

namespace Gauge.CSharp.Runner.Processors
{
    public class ScenarioExecutionEndingProcessor : HookExecutionProcessor
    {
        public ScenarioExecutionEndingProcessor(IHookRegistry hookRegistry) : base(hookRegistry)
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