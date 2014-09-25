using System.Collections.Generic;
using System.Reflection;

namespace Gauge.CSharp.Runner.Processors
{
    public class ExecutionEndingProcessor : HookExecutionProcessor
    {
        protected override HashSet<MethodInfo> GetHooks()
        {
            return HookRegistry.AfterSuiteHooks;
        }
    }
}