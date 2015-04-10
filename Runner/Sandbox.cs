// Copyright 2015 ThoughtWorks, Inc.

// This file is part of Gauge-CSharp.

// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// Gauge-Ruby is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

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
    [Serializable]
    public class Sandbox : MarshalByRefObject, ISandbox
    {
        private static Sandbox _instance;
        private List<Assembly> ScannedAssemblies { get; set; }
        private static Assembly TargetLibAssembly { get; set; }

        private static readonly string GaugeLibAssembleName = typeof(Step).Assembly.GetName().Name;

        public static Sandbox Instance
        {
            get { return _instance ?? (_instance=Create()); }
        }

        public void ExecuteMethod(MethodInfo method, params object[] args)
        {
            var instance = ClassInstanceManager.Get(method.DeclaringType);
            method.Invoke(instance, args);
        }

        private static Sandbox Create(AppDomainSetup setup=null)
        {
            var sandboxAppDomainSetup = setup ?? new AppDomainSetup {ApplicationBase = Utils.GaugeBinDir};

            var permSet = new PermissionSet(PermissionState.Unrestricted);

            var sandboxDomain = AppDomain.CreateDomain("Sandbox", null, sandboxAppDomainSetup, permSet);
            AppDomain.CurrentDomain.AssemblyResolve+=CurrentDomain_AssemblyResolve;

            var sandbox = (Sandbox)sandboxDomain.CreateInstanceFromAndUnwrap(
                typeof(Sandbox).Assembly.ManifestModule.FullyQualifiedName,
                typeof (Sandbox).FullName);
            sandbox.LoadAssemblyFiles();
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

        internal List<MethodInfo> GetStepMethods()
        {
            return GetAllMethodsForSpecAssemblies(typeof(Step).FullName);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var shortAssemblyName = args.Name.Substring(0, args.Name.IndexOf(','));
            var fileName = Path.Combine(Utils.GaugeBinDir, shortAssemblyName + ".dll");
            if (File.Exists(fileName))
            {
                return Assembly.LoadFrom(fileName);
            }
            return Assembly.GetExecutingAssembly().FullName == args.Name ? Assembly.GetExecutingAssembly() : null;
        }

        private static List<MethodInfo> GetAllMethodsForSpecAssemblies(string type)
        {
            var targetType = TargetLibAssembly.GetType(type);
            return Instance.ScannedAssemblies.SelectMany(assembly => GetMethodsFromAssembly(targetType, assembly)).ToList();
        }

        private static List<MethodInfo> GetMethodsFromAssembly(Type type, Assembly assembly)
        {
            var isGaugeAssembly = assembly.GetReferencedAssemblies().Select(name => name.Name).Contains(GaugeLibAssembleName);
            return isGaugeAssembly ? assembly.GetTypes().SelectMany(t => t.GetMethods().Where(info => info.GetCustomAttributes(type).Any())).ToList() : new List<MethodInfo>();
        }
        private void LoadAssemblyFiles()
        {
            ScannedAssemblies=Directory.EnumerateFiles(Utils.GaugeBinDir, "*.dll", SearchOption.TopDirectoryOnly)
                .Select(Assembly.LoadFrom)
                .ToList();
            Console.WriteLine("Loaded {0} assemblies", ScannedAssemblies.Count());
            TargetLibAssembly = ScannedAssemblies.First(assembly => assembly.GetName().Name == GaugeLibAssembleName);
        }
    }
}
