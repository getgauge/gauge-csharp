// Copyright 2015 ThoughtWorks, Inc.

// This file is part of Gauge-CSharp.

// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Communication;

namespace Gauge.CSharp.Runner
{
    [Serializable]
    public class Sandbox : MarshalByRefObject, ISandbox
    {
        private List<Assembly> ScannedAssemblies { get; set; }
        public Assembly TargetLibAssembly { get; set; }
        private static readonly string GaugeLibAssembleName = typeof(Step).Assembly.GetName().Name;
        private Type ScreenGrabberType { get; set; }

        [Obsolete("Sandbox is supposed to be a singleton class. Use Sandbox.Instance instead", true)]
        public Sandbox()
        {
        }

        [DebuggerStepperBoundary]
        [DebuggerHidden]
        public void ExecuteMethod(MethodInfo method, params object[] args)
        {
            var instance = ClassInstanceManager.Get(method.DeclaringType);
            method.Invoke(instance, args);
        }

        public HookRegistry GetHookRegistry()
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

        public List<MethodInfo> GetStepMethods()
        {
            return GetAllMethodsForSpecAssemblies(typeof(Step).FullName);
        }

        public void InitializeDataStore(string dataStoreType)
        {
            var remoteDataStoreType = TargetLibAssembly.GetType(typeof(DataStoreFactory).ToString());
            var dataStoreGetter = remoteDataStoreType.GetMethod(string.Format("Initialize{0}DataStore", dataStoreType));
            if (dataStoreGetter != null)
                dataStoreGetter.Invoke(null, null);
        }

        public bool TryScreenCapture(out byte[] screenShotBytes)
        {
            if (ScreenGrabberType != null)
            {
                var screenCaptureMethod = ScreenGrabberType.GetMethod("TakeScreenShot");
                var instance = Activator.CreateInstance(ScreenGrabberType);
                if (instance != null)
                {
                    screenShotBytes = screenCaptureMethod.Invoke(instance, null) as byte[];
                    return true;
                }
            }
            screenShotBytes = null;
            return false;
        }

        private List<MethodInfo> GetAllMethodsForSpecAssemblies(string type)
        {
            var targetType = TargetLibAssembly.GetType(type);
            return ScannedAssemblies.SelectMany(assembly => GetMethodsFromAssembly(targetType, assembly)).ToList();
        }

        private static List<MethodInfo> GetMethodsFromAssembly(Type type, Assembly assembly)
        {
            var isGaugeAssembly = assembly.GetReferencedAssemblies().Select(name => name.Name).Contains(GaugeLibAssembleName);
            return isGaugeAssembly
                ? assembly.GetTypes()
                    .SelectMany(t => t.GetMethods().Where(info => info.GetCustomAttributes(type).Any()))
                    .ToList()
                : new List<MethodInfo>();
        }

        internal void LoadAssemblyFiles()
        {
            ScannedAssemblies=Directory.EnumerateFiles(Utils.GaugeBinDir, "*.dll", SearchOption.TopDirectoryOnly)
                .Select(Assembly.LoadFrom)
                .ToList();
            TargetLibAssembly = ScannedAssemblies.First(assembly => assembly.GetName().Name == GaugeLibAssembleName);
            
            ScreenGrabberType = ScannedAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => type.GetInterfaces().Any(t => t.FullName == typeof(IScreenGrabber).FullName));
        }
    }
}
