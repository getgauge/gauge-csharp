using System.Collections.Generic;
using System.Reflection;
using main;

namespace Gauge.CSharp.Runner.Processors
{
    public class ExecutionStartingProcessor : HookExecutionProcessor
    {
        public ExecutionStartingProcessor(HookRegistry hookRegistry) : base(hookRegistry)
        {
        }

        protected override IEnumerable<MethodInfo> GetHooks()
        {
            return Hooks.BeforeSuiteHooks;
        }

        protected override ExecutionInfo GetExecutionInfo(Message request)
        {
            return request.ExecutionStartingRequest.CurrentExecutionInfo;
        }
    }
}