using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace gauge_csharp
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

        public IList<string> AllSteps()
        {
            var list = new List<string>();
            foreach (string stepText in _scannedSteps.Keys)
            {
                list.Add(stepText);
            }
            return list;

        }
    }
}