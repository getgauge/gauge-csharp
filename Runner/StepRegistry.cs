using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gauge.CSharp.Runner
{
    public class StepRegistry : IStepRegistry
    {
        private readonly Dictionary<string, MethodInfo> _scannedSteps;

        public StepRegistry(IEnumerable<KeyValuePair<string, MethodInfo>> scannedSteps)
        {
            _scannedSteps = scannedSteps.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public bool ContainsStep(string parsedStepText)
        {
            return _scannedSteps.ContainsKey(parsedStepText);
        }

        public MethodInfo MethodFor(string parsedStepText)
        {
            return _scannedSteps[parsedStepText];
        }

        public IEnumerable<string> AllSteps()
        {
            return _scannedSteps.Keys;
        }
    }
}