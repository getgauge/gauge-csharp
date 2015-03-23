using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Lib.Attribute;

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

        private static Sandbox Create()
        {
            var sandboxAppDomainSetup = new AppDomainSetup {ApplicationBase = Utils.GaugeBinDir};

            var permSet = new PermissionSet(PermissionState.None);
            permSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

            var newDomain = AppDomain.CreateDomain("Sandbox", null, sandboxAppDomainSetup, permSet);

            var handle = Activator.CreateInstanceFrom(
                newDomain, typeof(Sandbox).Assembly.ManifestModule.FullyQualifiedName,
                typeof(Sandbox).FullName
                );
            var sandbox = (Sandbox)handle.Unwrap();
            _scannedAssemblies = GetAllAssemblyFiles();
            return sandbox;
        }

        internal HookRegistry GetHookRegistry()
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

        internal IEnumerable<MethodInfo> GetStepMethods()
        {
            return GetAllMethodsForSpecAssemblies<Step>();
        }
        private static IEnumerable<MethodInfo> GetAllMethodsForSpecAssemblies<T>() where T : Attribute
        {
            return _scannedAssemblies.SelectMany(GetMethodsFromAssembly<T>);
        }

        private static IEnumerable<MethodInfo> GetMethodsFromAssembly<T>(Assembly assembly) where T : Attribute
        {
            var isGaugeAssembly = assembly.GetReferencedAssemblies().Select(name => name.Name).Contains("Gauge.CSharp.Lib");
            return isGaugeAssembly ? assembly.GetTypes().SelectMany(type => type.GetMethods().Where(info => info.GetCustomAttributes<T>().Any())) : Enumerable.Empty<MethodInfo>();
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