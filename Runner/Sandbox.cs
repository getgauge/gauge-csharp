using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Communication;

namespace Gauge.CSharp.Runner
{
    public class Sandbox : MarshalByRefObject, ISandbox
    {
        private static Sandbox _instance;
        public static Sandbox Instance
        {
            get { return _instance ?? (_instance=Create()); }
        }

        private static IEnumerable<Assembly> _scannedAssemblies;
        private static Assembly _targetLibAssembly;

        private static Sandbox Create(AppDomainSetup setup=null)
        {
            var sandboxAppDomainSetup = setup ?? new AppDomainSetup {ApplicationBase = Utils.GaugeBinDir};

            var permSet = new PermissionSet(PermissionState.Unrestricted);

            var newDomain = AppDomain.CreateDomain("Sandbox", null, sandboxAppDomainSetup, permSet);
            
            var handle = Activator.CreateInstanceFrom(
                newDomain, typeof(Sandbox).Assembly.ManifestModule.FullyQualifiedName,
                typeof(Sandbox).FullName
                );
            var sandbox = (Sandbox)handle.Unwrap();
            _scannedAssemblies = GetAllAssemblyFiles();
            _targetLibAssembly = _scannedAssemblies.First(assembly => assembly.GetName().Name=="Gauge.CSharp.Lib");
            return sandbox;
        }

        internal HookRegistry GetHookRegistry()
        {
            var hookRegistry = new HookRegistry();
            hookRegistry.AddBeforeSuiteHooks(GetAllMethodsForSpecAssemblies(typeof(BeforeSuite).ToString()));
            hookRegistry.AddAfterSuiteHooks(GetAllMethodsForSpecAssemblies(typeof(AfterSuite).ToString()));
            hookRegistry.AddBeforeSpecHooks(GetAllMethodsForSpecAssemblies(typeof(BeforeSpec).ToString()));
            hookRegistry.AddAfterSpecHooks(GetAllMethodsForSpecAssemblies(typeof(AfterSpec).ToString()));
            hookRegistry.AddBeforeScenarioHooks(GetAllMethodsForSpecAssemblies(typeof(BeforeScenario).ToString()));
            hookRegistry.AddAfterScenarioHooks(GetAllMethodsForSpecAssemblies(typeof(AfterScenario).ToString()));
            hookRegistry.AddBeforeStepHooks(GetAllMethodsForSpecAssemblies(typeof(BeforeStep).ToString()));
            hookRegistry.AddAfterStepHooks(GetAllMethodsForSpecAssemblies(typeof(AfterStep).ToString()));
            return hookRegistry;
        }

        internal IEnumerable<MethodInfo> GetStepMethods()
        {
            return GetAllMethodsForSpecAssemblies(typeof(Step).ToString());
        }
        private static IEnumerable<MethodInfo> GetAllMethodsForSpecAssemblies(string type)
        {
            var targetType = _targetLibAssembly.GetType(type);
            return _scannedAssemblies.SelectMany(assembly => GetMethodsFromAssembly(targetType, assembly));
        }

        private static IEnumerable<MethodInfo> GetMethodsFromAssembly(Type type, Assembly assembly)
        {
            var isGaugeAssembly = assembly.GetReferencedAssemblies().Select(name => name.Name).Contains("Gauge.CSharp.Lib");
            return isGaugeAssembly ? assembly.GetTypes().SelectMany(t => t.GetMethods().Where(info => info.GetCustomAttributes(type).Any())) : Enumerable.Empty<MethodInfo>();
        }
        private static IEnumerable<Assembly> GetAllAssemblyFiles()
        {
            return Directory.EnumerateFiles(Utils.GaugeBinDir, "*.dll", SearchOption.TopDirectoryOnly).Select(Assembly.LoadFrom);
        }

        public void ExecuteMethod(MethodInfo method, params object[] args)
        {
            var instance = ClassInstanceManager.Get(method.DeclaringType);
            method.Invoke(instance, args);
        }
    }
}