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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Converters;
using Gauge.CSharp.Runner.InstanceManagement;
using Gauge.CSharp.Runner.Wrappers;
using NLog;

namespace Gauge.CSharp.Runner
{
    [Serializable]
    public class Sandbox : MarshalByRefObject, ISandbox
    {
        private readonly IAssemblyLoader _assemblyLoader;

		private readonly Assembly _libAssembly;

        private static readonly Logger Logger = LogManager.GetLogger("Sandbox");

        private Type ScreenGrabberType { get; set; }

        private dynamic _classInstanceManager;


        public Sandbox(IAssemblyLocater locater)
        {
            LogConfiguration.Initialize();
            var assemblies = locater.GetAllAssemblies();
            _assemblyLoader = new AssemblyLoader(assemblies);
			_libAssembly = _assemblyLoader.GetTargetLibAssembly();
            SetAppConfigIfExists();
            ScanCustomScreenGrabber();
            LoadClassInstanceManager();
        }

        public Sandbox() : this(new AssemblyLocater(new DirectoryWrapper(), new FileWrapper()))
        {
        }

        [DebuggerStepperBoundary]
        [DebuggerHidden]
        public ExecutionResult ExecuteMethod(MethodInfo method, params object[] args)
        {
            var executionResult = new ExecutionResult {Success = true};
            try
            {
                var parameters = args.Select(o => o is TableDonkey ? ToTable((TableDonkey)o) : o).ToArray();
                Type typeToLoad = method.DeclaringType;
                var instance = _classInstanceManager.Get(typeToLoad);
                if (instance == null)
                {
                    string error = "Could not instance type: " + typeToLoad;
                    Logger.Error(error);
                    throw new Exception(error);
                }
                Logger.Info(instance.GetType().FullName);
                method.Invoke(instance, parameters);
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException;
                executionResult.ExceptionMessage = innerException.Message;
                executionResult.StackTrace = innerException.StackTrace;
                executionResult.Source= innerException.Source;
                executionResult.Success = false;
            }

            return executionResult;
        }

        private object ToTable(TableDonkey donkey)
        {
            var table = _libAssembly.CreateInstance(typeof (Table).FullName, true, BindingFlags.CreateInstance,null,
                new object[] {donkey.Headers}, CultureInfo.CurrentCulture, null);
            Logger.Debug("Got Table from {0} at {1}", table.GetType().Assembly.FullName, table.GetType().Assembly.CodeBase);
            foreach (var row in donkey.Rows)
            {
                table.GetType().InvokeMember("AddRow", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod,
                        null, table, new object[] {row});
            }
            return table;
        }


        public Type GetTargetType(string typeFullName)
        {
            return _libAssembly.GetType(typeFullName);
        }

        public string TargetLibAssemblyVersion {
			get{ return FileVersionInfo.GetVersionInfo(_libAssembly.Location).ProductVersion; } 
		}

        private Assembly TargetLibAssembly {
			get{ return _libAssembly; } 
		}

        public IHookRegistry GetHookRegistry()
        {
            return new HookRegistry(_assemblyLoader);
        }

        public List<MethodInfo> GetStepMethods()
        {
            return _assemblyLoader.GetMethods(typeof(Step));
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
			var fullStepName = typeof(Step).FullName;
			dynamic step = stepMethod.GetCustomAttributes ().FirstOrDefault (
				a => a.GetType ().FullName.Equals (fullStepName));
            return step.Names;
        }


        public void LoadClassInstanceManager()
        {
            var instanceManagerType = _assemblyLoader.ClassInstanceManagerTypes.FirstOrDefault();

            _classInstanceManager = instanceManagerType != null
                ? Activator.CreateInstance(instanceManagerType)
                : new DefaultClassInstanceManager();

            Logger.Info("Loaded Instance Manager of Type:" + _classInstanceManager.GetType().FullName);

            _classInstanceManager.Initialize(_assemblyLoader.AssembliesReferencingGaugeLib);
        }


        public bool TryScreenCapture(out byte[] screenShotBytes)
        {
            try
            {
                var screenCaptureMethod = ScreenGrabberType.GetMethod("TakeScreenShot");
                var instance = Activator.CreateInstance(ScreenGrabberType);
                if (instance != null)
                {
                    screenShotBytes = screenCaptureMethod.Invoke(instance, null) as byte[];
                    return true;
                }
            }
            catch
            {
                //do nothing, return
            }

            screenShotBytes = null;
            return false;
        }

        public void ClearObjectCache()
        {
            _classInstanceManager.ClearCache();
        }

        public IEnumerable<string> GetAllPendingMessages()
        {
            var targetMessageCollectorType = TargetLibAssembly.GetType(typeof(MessageCollector).ToString());
            var targetMethod = targetMessageCollectorType.GetMethod("GetAllPendingMessages", BindingFlags.Static | BindingFlags.Public);
            return targetMethod.Invoke(null, null) as IEnumerable<string>;
        }

        public void StartExecutionScope(string tag)
        {
            _classInstanceManager.StartScope(tag);
        }

        public void CloseExectionScope()
        {
            _classInstanceManager.CloseScope();
        }


        private void SetAppConfigIfExists()
        {            
            var targetAssembly = _assemblyLoader.AssembliesReferencingGaugeLib.FirstOrDefault();
            if (targetAssembly == null) return;

            var configFile = string.Format("{0}.config", targetAssembly.Location);
            if (File.Exists(configFile))
            {
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configFile);
            }
        }

        private void ScanCustomScreenGrabber()
        {
            ScreenGrabberType = _assemblyLoader.ScreengrabberTypes.FirstOrDefault();
            if (ScreenGrabberType != null)
            {
                Logger.Debug("Custom ScreenGrabber found : {0}", ScreenGrabberType.FullName);
            }
            else
            {
                Logger.Debug("No implementation of IScreenGrabber found. Using DefaultScreenGrabber");
                ScreenGrabberType = typeof (DefaultScreenGrabber);
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
