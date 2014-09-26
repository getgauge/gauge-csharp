using System.Collections.Generic;
using System.Reflection;
using main;

namespace Gauge.CSharp.Runner.Processors
{
    internal class SpecExecutionEndingProcessor : HookExecutionProcessor
    {
        public SpecExecutionEndingProcessor(HookRegistry hookRegistry) : base(hookRegistry)
        {
        }

        protected override IEnumerable<MethodInfo> GetHooks()
        {
            return Hooks.AfterSpecHooks;
        }

        protected override ExecutionInfo GetExecutionInfo(Message request)
        {
            return request.SpecExecutionEndingRequest.CurrentExecutionInfo;
        }
    }
}