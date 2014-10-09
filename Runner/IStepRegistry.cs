using System.Collections.Generic;
using System.Reflection;

namespace Gauge.CSharp.Runner
{
    public interface IStepRegistry
    {
        bool ContainsStep(string parsedStepText);
        MethodInfo MethodFor(string parsedStepText);
        IEnumerable<string> AllSteps();
    }
}