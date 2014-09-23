using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gauge.CSharp.Runner
{
    public class StepRegistry
    {
        private readonly Hashtable _scannedSteps;

        public StepRegistry(Hashtable scannedSteps)
        {
            _scannedSteps = scannedSteps;
        }

        public bool ContainsStep(string parsedStepText)
        {
            return _scannedSteps.ContainsKey(parsedStepText);
        }

        public MethodInfo MethodFor(string parsedStepText)
        {
            return (MethodInfo) _scannedSteps[parsedStepText];
        }

        public IEnumerable<string> AllSteps()
        {
            return _scannedSteps.Keys.Cast<string>();
        }
    }
}