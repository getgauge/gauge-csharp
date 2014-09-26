using System.Collections.Generic;
using System.Reflection;
using main;

namespace Gauge.CSharp.Runner.Processors
{
    internal class SpecExecutionStartingProcessor : HookExecutionProcessor
    {
        public SpecExecutionStartingProcessor(HookRegistry hookRegistry) : base(hookRegistry)
        {
        }

        protected override IEnumerable<MethodInfo> GetHooks()
        {
            return Hooks.BeforeSpecHooks;
        }

        protected override ExecutionInfo GetExecutionInfo(Message request)
        {
            return request.SpecExecutionStartingRequest.CurrentExecutionInfo;
        }
    }
}