// Copyright 2015 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
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
        private readonly AssemblyScanner _assemblyScanner = new AssemblyScanner();

        public Assembly TargetLibAssembly { get; private set; }

        private static readonly Logger Logger = LogManager.GetLogger("Sandbox");

        private Type ScreenGrabberType { get; set; }

        [Obsolete("Sandbox is supposed to be a singleton class. Use Sandbox.Instance instead", true)]
        public Sandbox()
        {
            TargetLibAssembly = _assemblyScanner.GetTargetLibAssembly();
            LogConfiguration.Initialize();
        }

        [DebuggerStepperBoundary]
        [DebuggerHidden]
        public ExecutionResult ExecuteMethod(MethodInfo method, params object[] args)
        {
            var executionResult = new ExecutionResult {Success = true};
            var instance = ClassInstanceManager.Get(method.DeclaringType);
            try
            {
                method.Invoke(instance, args);
            }
            catch (TargetInvocationException ex)
            {
                var innerException = ex.InnerException;
                executionResult.ExceptionMessage = innerException.Message;
                executionResult.StackTrace = innerException.StackTrace;
                executionResult.Source= innerException.Source;
                executionResult.Success = false;
            }
            return executionResult;
        }

        public IHookRegistry GetHookRegistry()
        {
            var hookRegistry = new HookRegistry(this);
            hookRegistry.AddBeforeSuiteHooks(_assemblyScanner.GetMethods<BeforeSuite>());
            hookRegistry.AddAfterSuiteHooks(_assemblyScanner.GetMethods<AfterSuite>());
            hookRegistry.AddBeforeSpecHooks(_assemblyScanner.GetMethods<BeforeSpec>());
            hookRegistry.AddAfterSpecHooks(_assemblyScanner.GetMethods<AfterSpec>());
            hookRegistry.AddBeforeScenarioHooks(_assemblyScanner.GetMethods<BeforeScenario>());
            hookRegistry.AddAfterScenarioHooks(_assemblyScanner.GetMethods<AfterScenario>());
            hookRegistry.AddBeforeStepHooks(_assemblyScanner.GetMethods<BeforeStep>());
            hookRegistry.AddAfterStepHooks(_assemblyScanner.GetMethods<AfterStep>());
            return hookRegistry;
        }

        public List<MethodInfo> GetStepMethods()
        {
            return _assemblyScanner.GetMethods<Step>();
        }

        public List<string> GetAllStepTexts()
        {
            return GetStepMethods().SelectMany(GetStepTexts).ToList();
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

        internal void LoadAssemblyFiles()
        {
            try
            {
                EnumerateAndLoadAssemblies();
                SetAppConfigIfExists();
                ScanCustomScreenGrabber();
            }
            catch (ReflectionTypeLoadException ex)
            {
                Logger.Fatal(ex, "Unable to load one or more assemblies.");
                foreach (var loaderException in ex.LoaderExceptions)
                {
                    Logger.Error(loaderException);
                }
                throw;
            }
        }

        private void SetAppConfigIfExists()
        {            
            var targetAssembly = _assemblyScanner.AssembliesReferencingGaugeLib.FirstOrDefault();
            if (targetAssembly == null) return;

            var configFile = string.Format("{0}.config", targetAssembly.Location);
            if (File.Exists(configFile))
            {
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configFile);
            }
        }

        private void EnumerateAndLoadAssemblies()
        {
            foreach (var path in Directory.EnumerateFiles(Utils.GetGaugeBinDir(), "*.dll", SearchOption.TopDirectoryOnly))
                _assemblyScanner.TryAdd(path);
        }

        private void ScanCustomScreenGrabber()
        {
            ScreenGrabberType = _assemblyScanner.ScreengrabberTypes.FirstOrDefault();
            if (ScreenGrabberType != null)
            {
                Logger.Debug("Custom ScreenGrabber found : {0}", ScreenGrabberType.FullName);
            }
            else
            {
                Logger.Debug("No implementation of IScreenGrabber found.");
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
