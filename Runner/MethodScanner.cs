using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Lib.Attribute;

namespace Gauge.CSharp.Runner
{
    internal class MethodScanner
    {
        private readonly GaugeApiConnection _apiConnection;
        private readonly HookRegistry _hookRegistry;

        public MethodScanner(GaugeApiConnection apiConnection)
        {
            _apiConnection = apiConnection;
            _hookRegistry = new HookRegistry();
        }

        public StepRegistry GetStepRegistry()
        {
            return new StepRegistry(GetStepMethods());
        }

        private IEnumerable<KeyValuePair<string, MethodInfo>> GetStepMethods()
        {
            var stepMethods = GetAllMethodsForSpecAssemblies().Where(info => info.GetCustomAttributes().OfType<Step>().Any());
            foreach (var stepMethod in stepMethods)
            {
                var stepValues = _apiConnection.GetStepValue(stepMethod.GetCustomAttribute<Step>().Names, false);
                foreach (var stepValue in stepValues)
                {
                    yield return new KeyValuePair<string, MethodInfo>(stepValue, stepMethod);
                }
            }
        }

        public HookRegistry GetHookRegistry()
        {
            var allMethods = GetAllMethodsForSpecAssemblies();
            HookRegistry.AddBeforeSuiteHooks(allMethods.Where(info => info.GetCustomAttributes().OfType<BeforeSuite>().Any()));
            HookRegistry.AddAfterSuiteHooks(allMethods.Where(info => info.GetCustomAttributes().OfType<AfterSuite>().Any()));
            HookRegistry.AddBeforeSpecHooks(allMethods.Where(info => info.GetCustomAttributes().OfType<BeforeSpec>().Any()));
            HookRegistry.AddAfterSpecHooks(allMethods.Where(info => info.GetCustomAttributes().OfType<AfterSpec>().Any()));
            HookRegistry.AddBeforeScenarioHooks(allMethods.Where(info => info.GetCustomAttributes().OfType<BeforeScenario>().Any()));
            HookRegistry.AddAfterScenarioHooks(allMethods.Where(info => info.GetCustomAttributes().OfType<AfterScenario>().Any()));
            HookRegistry.AddBeforeStepHooks(allMethods.Where(info => info.GetCustomAttributes().OfType<BeforeStep>().Any()));
            HookRegistry.AddAfterStepHooks(allMethods.Where(info => info.GetCustomAttributes().OfType<AfterStep>().Any()));
            return _hookRegistry;
        }

        private IEnumerable<MethodInfo> GetAllMethodsForSpecAssemblies()
        {
            var enumerateFiles = GetAllAssemblyFiles();
            return enumerateFiles.SelectMany(GetMethodsFromAssembly);
        }

        private IEnumerable<MethodInfo> GetMethodsFromAssembly(string specAssembly)
        {
            var assembly = Assembly.LoadFile(specAssembly);
            return assembly.GetTypes().SelectMany(type => type.GetMethods());
        }

        private static IEnumerable<string> GetAllAssemblyFiles()
        {
            return Directory.EnumerateFiles(Utils.GaugeProjectRoot, "*.dll", SearchOption.AllDirectories);
        }
    }
}