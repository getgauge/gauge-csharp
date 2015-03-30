using System.Collections.Generic;
using System.Reflection;
using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public class ExecutionEndingProcessor : HookExecutionProcessor
    {
        public ExecutionEndingProcessor(IHookRegistry hookRegistry, IMethodExecutor methodExecutor) : base(hookRegistry, methodExecutor, new Sandbox())
        {
        }
        public ExecutionEndingProcessor(IHookRegistry hookRegistry, IMethodExecutor methodExecutor, ISandbox sandbox)
            : base(hookRegistry, methodExecutor, sandbox)
        {
        }

        protected override IEnumerable<MethodInfo> GetHooks()
        {
            return Hooks.AfterSuiteHooks;
        }

        protected override ExecutionInfo GetExecutionInfo(Message request)
        {
            return request.ExecutionEndingRequest.CurrentExecutionInfo;
        }
    }
}