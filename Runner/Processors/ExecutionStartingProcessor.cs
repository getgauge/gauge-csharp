using System.Collections.Generic;
using System.Reflection;
using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public class ExecutionStartingProcessor : HookExecutionProcessor
    {
        public ExecutionStartingProcessor(IHookRegistry hookRegistry, IMethodExecutor methodExecutor) : base(hookRegistry, methodExecutor, new Sandbox())
        {
        }

        public ExecutionStartingProcessor(IHookRegistry hookRegistry, IMethodExecutor methodExecutor, ISandbox sandbox)
            : base(hookRegistry, methodExecutor, sandbox)
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