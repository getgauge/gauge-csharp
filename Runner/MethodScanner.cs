using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Lib.Attribute;

namespace Gauge.CSharp.Runner
{
    public class MethodScanner : IMethodScanner
    {
        private readonly GaugeApiConnection _apiConnection;
        private static readonly IEnumerable<Assembly> ScannedAssemblies = GetAllAssemblyFiles();

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
            var stepMethods = GetAllMethodsForSpecAssemblies<Step>();
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
            var hookRegistry = new HookRegistry();
            hookRegistry.AddBeforeSuiteHooks(GetAllMethodsForSpecAssemblies<BeforeSuite>());
            hookRegistry.AddAfterSuiteHooks(GetAllMethodsForSpecAssemblies<AfterSuite>());
            hookRegistry.AddBeforeSpecHooks(GetAllMethodsForSpecAssemblies<BeforeSpec>());
            hookRegistry.AddAfterSpecHooks(GetAllMethodsForSpecAssemblies<AfterSpec>());
            hookRegistry.AddBeforeScenarioHooks(GetAllMethodsForSpecAssemblies<BeforeScenario>());
            hookRegistry.AddAfterScenarioHooks(GetAllMethodsForSpecAssemblies<AfterScenario>());
            hookRegistry.AddBeforeStepHooks(GetAllMethodsForSpecAssemblies<BeforeStep>());
            hookRegistry.AddAfterStepHooks(GetAllMethodsForSpecAssemblies<AfterStep>());
            return hookRegistry;
        }

        private IEnumerable<MethodInfo> GetAllMethodsForSpecAssemblies<T>() where T : Attribute
        {
            return ScannedAssemblies.SelectMany(GetMethodsFromAssembly<T>);
        }

        private static IEnumerable<MethodInfo> GetMethodsFromAssembly<T>(Assembly assembly) where T : Attribute
        {
            var isGaugeAssembly = assembly.GetReferencedAssemblies().Select(name => name.Name).Contains("Gauge.CSharp.Lib");
            return isGaugeAssembly ? assembly.GetTypes().SelectMany(type => type.GetMethods().Where(info => info.GetCustomAttributes<T>().Any())) : Enumerable.Empty<MethodInfo>();
        }

        private static IEnumerable<Assembly> GetAllAssemblyFiles()
        {
            return Directory.EnumerateFiles(Utils.GaugeBinDir, "*.dll", SearchOption.AllDirectories).Select(Assembly.LoadFrom);
        }
    }
}