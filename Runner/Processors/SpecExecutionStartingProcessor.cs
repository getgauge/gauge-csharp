using System.Collections.Generic;
using System.Reflection;
using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public class SpecExecutionStartingProcessor : HookExecutionProcessor
    {
        public SpecExecutionStartingProcessor(IHookRegistry hookRegistry) : base(hookRegistry)
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