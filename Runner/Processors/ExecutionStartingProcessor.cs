using System.Collections.Generic;
using System.Reflection;
using main;

namespace Gauge.CSharp.Runner.Processors
{
    public class ExecutionStartingProcessor : HookExecutionProcessor
    {
        protected override HashSet<MethodInfo> GetHooks()
        {
            return HookRegistry.BeforeSuiteHooks;
        }
    }
}