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
using System.Runtime.Serialization.Json;
using System.Text;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Runner.Converters;
using Gauge.CSharp.Runner.Models;
using Gauge.CSharp.Runner.Strategy;
using Gauge.CSharp.Runner.Wrappers;

//using NLog;

namespace Gauge.CSharp.Runner
{
    [Serializable]
    public class Sandbox : MarshalByRefObject, ISandbox
    {
        private readonly IAssemblyLoader _assemblyLoader;

        private readonly Assembly _libAssembly;

//        private static readonly Logger Logger = LogManager.GetLogger("Sandbox");

        private Type ScreenGrabberType { get; set; }

        private dynamic _classInstanceManager;

        private readonly IHookRegistry _hookRegistry;
        private readonly IFileWrapper _fileWrapper;

        private IDictionary<string, MethodInfo> MethodMap { get; set; }

        public Sandbox(IAssemblyLoader assemblyLoader, IHookRegistry hookRegistry, IFileWrapper fileWrapper)
        {
//            LogConfiguration.Initialize();
            _assemblyLoader = assemblyLoader;
            _hookRegistry = hookRegistry;
            _fileWrapper = fileWrapper;
            _libAssembly = _assemblyLoader.GetTargetLibAssembly();
            SetAppConfigIfExists();
            ScanCustomScreenGrabber();
            LoadClassInstanceManager();
        }

        public Sandbox() : this(new AssemblyLoader(), new HookRegistry(new AssemblyLoader()), new FileWrapper())
        {
        }

        [DebuggerStepperBoundary]
        [DebuggerHidden]
        public ExecutionResult ExecuteMethod(GaugeMethod gaugeMethod, params string[] args)
        {
            var method = MethodMap[gaugeMethod.Name];
            var executionResult = new ExecutionResult {Success = true};
            try
            {
                var parameters = args.Select(o =>
                {
                    try
                    {
                        return GetTable(o);
                    }
                    catch
                    {
                        return o;
                    }
                }).ToArray();
                Execute(method, StringParamConverter.TryConvertParams(method, parameters));
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException ?? ex;
                executionResult.ExceptionMessage = innerException.Message;
                executionResult.StackTrace = innerException.StackTrace;
                executionResult.Source = innerException.Source;
                executionResult.Success = false;
            }

            return executionResult;
        }

        private object GetTable(string jsonString)
        {
            var serializer = new DataContractJsonSerializer(_libAssembly.GetType("Gauge.CSharp.Lib.Table"));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                return serializer.ReadObject(ms);
            }
        }

        public string TargetLibAssemblyVersion
        {
            get { return FileVersionInfo.GetVersionInfo(_libAssembly.Location).ProductVersion; }
        }

        public List<GaugeMethod> GetStepMethods()
        {
            var infos = _assemblyLoader.GetMethods("Gauge.CSharp.Lib.Attribute.Step");
            MethodMap = new Dictionary<string, MethodInfo>();
            foreach (var info in infos)
                MethodMap.Add(string.Format("{0}.{1}", info.DeclaringType.FullName, info.Name), info);
            return
                MethodMap.Keys.Select(
                    s => new GaugeMethod {Name = s, ParameterCount = MethodMap[s].GetParameters().Length}).ToList();
        }


        public List<string> GetAllStepTexts()
        {
            return GetStepMethods().SelectMany(GetStepTexts).ToList();
        }

        public void InitializeDataStore(string dataStoreType)
        {
            var remoteDataStoreType = _libAssembly.GetType("Gauge.CSharp.Lib.DataStoreFactory");
            var dataStoreGetter = remoteDataStoreType.GetMethod(string.Format("Initialize{0}DataStore", dataStoreType));
            if (dataStoreGetter != null)
                dataStoreGetter.Invoke(null, null);
        }

        public IEnumerable<string> GetStepTexts(GaugeMethod gaugeMethod)
        {
            const string fullStepName = "Gauge.CSharp.Lib.Attribute.Step";
            var stepMethod = MethodMap[gaugeMethod.Name];
            dynamic step = stepMethod.GetCustomAttributes()
                .FirstOrDefault(a => a.GetType().FullName.Equals(fullStepName));
            return step.Names;
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
            var targetMessageCollectorType = _libAssembly.GetType(typeof(MessageCollector).ToString());
            var targetMethod = targetMessageCollectorType.GetMethod("GetAllPendingMessages",
                BindingFlags.Static | BindingFlags.Public);
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

        public ExecutionResult ExecuteHooks(string hookType, IHooksStrategy strategy, IEnumerable<string> applicableTags)
        {
            var methods = GetHookMethods(hookType, strategy, applicableTags);
            var executionResult = new ExecutionResult
            {
                Success = true
            };
            foreach (var method in methods)
            {
                try
                {
                    ExecuteHook(_hookRegistry.MethodFor(method));
                }
                catch (Exception ex)
                {
//                    Logger.Debug("Hook execution failed : {0}.{1}", method.DeclaringType.FullName, method.Name);
                    var innerException = ex.InnerException ?? ex;
                    executionResult.ExceptionMessage = innerException.Message;
                    executionResult.StackTrace = innerException.StackTrace;
                    executionResult.Source = innerException.Source;
                    executionResult.Success = false;
                }
            }
            return executionResult;
        }

        public IEnumerable<string> Refactor(GaugeMethod methodInfo, IEnumerable<Tuple<int, int>> parameterPositions,
            IList<string> parametersList, string newStepValue)
        {
            return RefactorHelper.Refactor(MethodMap[methodInfo.Name], parameterPositions, parametersList, newStepValue);
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        [DebuggerHidden]
        private void ExecuteHook(MethodInfo method, params object[] objects)
        {
            if (HasArguments(method, objects))
                Execute(method, objects);
            else
                Execute(method);
        }

        private static bool HasArguments(MethodInfo method, object[] args)
        {
            if (method.GetParameters().Length != args.Length)
            {
                return false;
            }
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].GetType() != method.GetParameters()[i].ParameterType)
                {
                    return false;
                }
            }
            return true;
        }

        private IEnumerable<string> GetHookMethods(string hookType, IHooksStrategy strategy,
            IEnumerable<string> applicableTags)
        {
            var hooksFromRegistry = GetHooksFromRegistry(hookType);
            return strategy.GetApplicableHooks(applicableTags, hooksFromRegistry);
        }

        private IEnumerable<IHookMethod> GetHooksFromRegistry(string hookType)
        {
            switch (hookType)
            {
                case "BeforeSuite":
                    return _hookRegistry.BeforeSuiteHooks;
                case "BeforeSpec":
                    return _hookRegistry.BeforeSpecHooks;
                case "BeforeScenario":
                    return _hookRegistry.BeforeScenarioHooks;
                case "BeforeStep":
                    return _hookRegistry.BeforeStepHooks;
                case "AfterStep":
                    return _hookRegistry.AfterStepHooks;
                case "AfterScenario":
                    return _hookRegistry.AfterScenarioHooks;
                case "AfterSpec":
                    return _hookRegistry.AfterSpecHooks;
                case "AfterSuite":
                    return _hookRegistry.AfterSuiteHooks;
                default:
                    return null;
            }
        }

        private void SetAppConfigIfExists()
        {
            var targetAssembly = _assemblyLoader.AssembliesReferencingGaugeLib.FirstOrDefault();
            if (targetAssembly == null) return;

            var configFile = string.Format("{0}.config", targetAssembly.Location);
            if (_fileWrapper.Exists(configFile))
            {
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configFile);
            }
        }

        private void ScanCustomScreenGrabber()
        {
            ScreenGrabberType = _assemblyLoader.ScreengrabberTypes.FirstOrDefault();
            if (ScreenGrabberType != null)
            {
//                Logger.Debug("Custom ScreenGrabber found : {0}", ScreenGrabberType.FullName);
            }
            else
            {
//                Logger.Debug("No implementation of IScreenGrabber found. Using DefaultScreenGrabber");
                ScreenGrabberType = _libAssembly.GetType("Gauge.CSharp.Lib.DefaultScreenGrabber");
            }
            ScreenGrabberType = ScreenGrabberType ?? Assembly.GetExecutingAssembly().GetType("Gauge.CSharp.Lib.DefaultScreenGrabber");
        }

        private void Execute(MethodBase method, params object[] parameters)
        {
            var typeToLoad = method.DeclaringType;
            var instance = _classInstanceManager.Get(typeToLoad);
            if (instance == null)
            {
                var error = "Could not load instance type: " + typeToLoad;
                //                Logger.Error(error);
                throw new Exception(error);
            }
            //            Logger.Info(instance.GetType().FullName);
            method.Invoke(instance, parameters);
        }

        private void LoadClassInstanceManager()
        {
            var instanceManagerType = _assemblyLoader.ClassInstanceManagerTypes.FirstOrDefault();

            if (instanceManagerType == null)
            {
                Console.WriteLine("Loading default ClassInstanceManager");
                _classInstanceManager = _libAssembly.CreateInstance("Gauge.CSharp.Lib.DefaultClassInstanceManager");
            }
            else
            {
                Console.WriteLine("Loading : {0}", instanceManagerType.FullName);
                _classInstanceManager = _libAssembly.CreateInstance(instanceManagerType.FullName);
            }

            _classInstanceManager = _classInstanceManager ??
                                    Activator.CreateInstance(AppDomain.CurrentDomain,
                                        Assembly.GetExecutingAssembly()
                                            .GetReferencedAssemblies()
                                            .First(name => name.Name == "Gauge.CSharp.Lib")
                                            .Name, "Gauge.CSharp.Lib.DefaultClassInstanceManager").Unwrap();
            //            Logger.Info("Loaded Instance Manager of Type:" + _classInstanceManager.GetType().FullName);

            Console.WriteLine("Loaded : {0}", _classInstanceManager.GetType());
            _classInstanceManager.Initialize(_assemblyLoader.AssembliesReferencingGaugeLib);
        }
    }
}
