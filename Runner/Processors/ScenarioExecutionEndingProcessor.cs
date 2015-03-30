using System.Collections.Generic;
using System.Reflection;
using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public class ScenarioExecutionEndingProcessor : HookExecutionProcessor
    {
        public ScenarioExecutionEndingProcessor(IHookRegistry hookRegistry) : base(hookRegistry, new Sandbox())
        {
        }

        public ScenarioExecutionEndingProcessor(IHookRegistry hookRegistry, ISandbox sandbox)
            : base(hookRegistry, new MethodExecutor(sandbox), sandbox)
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