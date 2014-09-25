using System.Collections.Generic;
using System.Reflection;
using Gauge.CSharp.Runner.Processors;

namespace Gauge.CSharp.Runner
{
    internal class StepExecutionStartingProcessor : HookExecutionProcessor
    {
        protected override HashSet<MethodInfo> GetHooks()
        {
            return HookRegistry.BeforeStepHooks;
        }
    }
}