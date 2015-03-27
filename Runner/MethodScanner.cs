using System.Collections.Generic;
using System.Reflection;
using System.Security.Policy;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Lib.Attribute;

namespace Gauge.CSharp.Runner
{
    public class MethodScanner : IMethodScanner
    {
        private readonly GaugeApiConnection _apiConnection;

        private readonly Sandbox _sandbox = Sandbox.Instance;
        public MethodScanner(GaugeApiConnection apiConnection)
        {
            _apiConnection = apiConnection;
        }

        public IStepRegistry GetStepRegistry()
        {
            return new StepRegistry(GetStepMethods());
        }

        private IEnumerable<KeyValuePair<string, MethodInfo>> GetStepMethods()
        {
            var stepMethods = _sandbox.GetStepMethods();
            foreach (var stepMethod in stepMethods)
            {
                var stepValues = _apiConnection.GetStepValue(stepMethod.GetCustomAttribute<Step>().Names, false);
                foreach (var stepValue in stepValues)
                {
                    yield return new KeyValuePair<string, MethodInfo>(stepValue, stepMethod);
                }
            }
        }

        public IHookRegistry GetHookRegistry()
        {
            return _sandbox.GetHookRegistry();
        }

    }
}