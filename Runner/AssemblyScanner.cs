using Gauge.CSharp.Lib;
using Gauge.CSharp.Lib.Attribute;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Gauge.CSharp.Core;

namespace Gauge.CSharp.Runner
{
    public class AssemblyScanner
    {
        private static readonly string GaugeLibAssembleName = typeof(Step).Assembly.GetName().Name;

        private static readonly Logger Logger = LogManager.GetLogger("AssemblyScanner");
        private readonly Dictionary<Type, List<MethodInfo>> _attributeToMethodInfo;

        public List<Assembly> AssembliesReferencingGaugeLib = new List<Assembly>();
        public List<Type> ScreengrabberTypes = new List<Type>();
        private Assembly _targetLibAssembly;

        public AssemblyScanner()
        {
            LoadTargetLibAssembly();
            _attributeToMethodInfo = new Dictionary<Type, List<MethodInfo>>();
            _attributeToMethodInfo[GetTypeFromTargetLib<BeforeSuite>()] = new List<MethodInfo>();
            _attributeToMethodInfo[GetTypeFromTargetLib<AfterSuite>()] = new List<MethodInfo>();
            _attributeToMethodInfo[GetTypeFromTargetLib<BeforeSpec>()] = new List<MethodInfo>();
            _attributeToMethodInfo[GetTypeFromTargetLib<AfterSpec>()] = new List<MethodInfo>();
            _attributeToMethodInfo[GetTypeFromTargetLib<BeforeScenario>()] = new List<MethodInfo>();
            _attributeToMethodInfo[GetTypeFromTargetLib<AfterScenario>()] = new List<MethodInfo>();
            _attributeToMethodInfo[GetTypeFromTargetLib<BeforeStep>()] = new List<MethodInfo>();
            _attributeToMethodInfo[GetTypeFromTargetLib<AfterStep>()] = new List<MethodInfo>();
            _attributeToMethodInfo[GetTypeFromTargetLib<Step>()] = new List<MethodInfo>();
        }

        public List<MethodInfo> GetMethods<T>() where T : Attribute
        {
            return _attributeToMethodInfo[GetTypeFromTargetLib<T>()];
        }

        public Assembly GetTargetLibAssembly()
        {
            return _targetLibAssembly;
        }

        public void TryAdd(string path)
        {
            Logger.Debug("Loading assembly from : {0}", path);
            Assembly assembly;
            try
            {
                // Load assembly for reflection only to avoid exceptions when referenced assemblies cannot be found
                assembly = Assembly.ReflectionOnlyLoadFrom(path);
            }
            catch
            {
                Logger.Warn("Failed to scan assembly {0}", path);
                return;
            }

            var isReferencingGaugeLib = assembly.GetReferencedAssemblies()
                .Select(name => name.Name)
                .Contains(typeof(Step).Assembly.GetName().Name);

            var loadableTypes = new HashSet<Type>(isReferencingGaugeLib ? GetLoadableTypes(assembly) : new Type[]{});

            // Load assembly so that code can be executed
            var fullyLoadedAssembly = Assembly.LoadFrom(path);
            var types = GetFullyLoadedTypes(loadableTypes, fullyLoadedAssembly).ToList();

            if (isReferencingGaugeLib)
                AssembliesReferencingGaugeLib.Add(fullyLoadedAssembly);

            ScanForScreengrabber(types);
            ScanForMethodAttributes(types);
        }

        private Type GetTypeFromTargetLib<T>() where T : Attribute
        {
            return _targetLibAssembly.GetType(typeof(T).FullName);
        }

        private IEnumerable<Type> GetFullyLoadedTypes(IEnumerable<Type> loadableTypes, Assembly fullyLoadedAssembly)
        {
            foreach (var type in loadableTypes)
            {
                var fullyLoadedType = fullyLoadedAssembly.GetType(type.FullName);
                if (fullyLoadedType == null)
                    Logger.Warn("Cannot scan type '{0}'", type.FullName);                    
                else
                    yield return fullyLoadedType;
            }
        }

        private void ScanForMethodAttributes(IEnumerable<Type> types)
        {
            foreach (var type in _attributeToMethodInfo.Keys)
            {
                var methodInfos = types
                    .SelectMany(t => t.GetMethods().Where(info => info.GetCustomAttributes(type).Any()));
                _attributeToMethodInfo[type].AddRange(methodInfos);
            }
        }

        private void ScanForScreengrabber(IEnumerable<Type> types)
        {
            var implementingTypes =
                types.Where(type => type.GetInterfaces().Any(t => t.FullName == typeof(IScreenGrabber).FullName));
            ScreengrabberTypes.AddRange(implementingTypes);
        }

        private IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                Logger.Warn("Could not scan all types in assembly {0}", assembly.CodeBase);
                return e.Types.Where(type => type != null);
            }
        }

        private void LoadTargetLibAssembly()
        {
            var targetLibLocation = Path.GetFullPath(Path.Combine(Utils.GetGaugeBinDir(), string.Concat(GaugeLibAssembleName, ".dll")));
            _targetLibAssembly = Assembly.LoadFrom(targetLibLocation);
            Logger.Debug("Target Lib loaded : {0}, from {1}", _targetLibAssembly.FullName, _targetLibAssembly.Location);
        }
    }
}
