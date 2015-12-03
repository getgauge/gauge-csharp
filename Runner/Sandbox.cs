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
using Gauge.CSharp.Core;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Lib.Attribute;
using NLog;

namespace Gauge.CSharp.Runner
{
    [Serializable]
    public class Sandbox : MarshalByRefObject, ISandbox
    {
        private List<Assembly> ScannedAssemblies { get; set; }
        public Assembly TargetLibAssembly { get; set; }

        private static readonly string GaugeLibAssembleName = typeof(Step).Assembly.GetName().Name;
        private static readonly Logger logger = LogManager.GetLogger("Sandbox");

        private Type ScreenGrabberType { get; set; }

        [Obsolete("Sandbox is supposed to be a singleton class. Use Sandbox.Instance instead", true)]
        public Sandbox()
        {
            LogConfiguration.Initialize();
        }

        [DebuggerStepperBoundary]
        [DebuggerHidden]
        public void ExecuteMethod(MethodInfo method, params object[] args)
        {
            var instance = ClassInstanceManager.Get(method.DeclaringType);
            method.Invoke(instance, args);
        }

        public IHookRegistry GetHookRegistry()
        {
            var hookRegistry = new HookRegistry(this);
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

        public List<string> GetAllStepTexts()
        {
            return GetAllMethodsForSpecAssemblies(typeof(Step).FullName).SelectMany(GetStepTexts).ToList();
        }

        public void InitializeDataStore(string dataStoreType)
        {
            var remoteDataStoreType = TargetLibAssembly.GetType(typeof(DataStoreFactory).ToString());
            var dataStoreGetter = remoteDataStoreType.GetMethod(string.Format("Initialize{0}DataStore", dataStoreType));
            if (dataStoreGetter != null)
                dataStoreGetter.Invoke(null, null);
        }

        public IEnumerable<string> GetStepTexts(MethodInfo stepMethod)
        {
            var targetStepType = TargetLibAssembly.GetType(typeof(Step).ToString());
            dynamic step = stepMethod.GetCustomAttribute(targetStepType);
            return step.Names;
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

        public void ClearObjectCache()
        {
            ClassInstanceManager.ClearCache();
        }

        public IEnumerable<string> GetAllPendingMessages()
        {
            var targetMessageCollectorType = TargetLibAssembly.GetType(typeof(MessageCollector).ToString());
            var targetMethod = targetMessageCollectorType.GetMethod("GetAllPendingMessages", BindingFlags.Static | BindingFlags.Public);
            return targetMethod.Invoke(null, null) as IEnumerable<string>;
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
            try
            {
                ScannedAssemblies = Directory.EnumerateFiles(Utils.GetGaugeBinDir(), "*.dll", SearchOption.TopDirectoryOnly)
                    .Select(s =>
                        {
                            logger.Debug("Loading assembly from : {0}", s);
                            return Assembly.LoadFrom(s);
                        })
                    .ToList();
                TargetLibAssembly = ScannedAssemblies.First(assembly => assembly.GetName().Name == GaugeLibAssembleName);
                logger.Debug("Target Lib loaded : {0}, from {1}", TargetLibAssembly.FullName, TargetLibAssembly.Location);

                ScreenGrabberType = ScannedAssemblies
                    .SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(type => type.GetInterfaces().Any(t => t.FullName == typeof(IScreenGrabber).FullName));
                if (ScreenGrabberType!=null)
                {
                    logger.Debug("Custom ScreenGrabber found : {0}", ScreenGrabberType.FullName);
                }
                else
                {
                    logger.Debug("No implementation of IScreenGrabber found.");
                }

            }
            catch (ReflectionTypeLoadException ex)
            {
                logger.Fatal(ex, "Unable to load one or more assemblies.");
                foreach (var loaderException in ex.LoaderExceptions)
                {
                    logger.Error(loaderException);
                }
                throw;
            }
        }
    }
}
