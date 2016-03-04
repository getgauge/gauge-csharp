using Gauge.CSharp.Lib;
using Gauge.CSharp.Lib.Attribute;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gauge.CSharp.Runner
{
    public class AssemblyScanner
    {
        private static readonly Logger Logger = LogManager.GetLogger("Sandbox");

        public List<Assembly> AssembliesReferencingGaugeLib = new List<Assembly>();
        public List<Type> ScreengrabberTypes = new List<Type>();
        public Dictionary<Type, List<MethodInfo>> AttributeToMethodInfo;

        public AssemblyScanner()
        {
            AttributeToMethodInfo = new Dictionary<Type, List<MethodInfo>>();
            AttributeToMethodInfo[typeof(BeforeSuite)] = new List<MethodInfo>();
            AttributeToMethodInfo[typeof(AfterSuite)] = new List<MethodInfo>();
            AttributeToMethodInfo[typeof(BeforeSpec)] = new List<MethodInfo>();
            AttributeToMethodInfo[typeof(AfterSpec)] = new List<MethodInfo>();
            AttributeToMethodInfo[typeof(BeforeScenario)] = new List<MethodInfo>();
            AttributeToMethodInfo[typeof(AfterScenario)] = new List<MethodInfo>();
            AttributeToMethodInfo[typeof(BeforeStep)] = new List<MethodInfo>();
            AttributeToMethodInfo[typeof(AfterStep)] = new List<MethodInfo>();
            AttributeToMethodInfo[typeof(Step)] = new List<MethodInfo>();
        }

        public void Scan(string path)
        {
            Logger.Debug("Loading assembly from : {0}", path);
            Assembly assembly;
            try
            {
                // Load assembly for reflection only to avoid exceptions when referenced assemblies cannot be found
                assembly = Assembly.ReflectionOnlyLoadFrom(path);
            }
            catch (Exception e)
            {
                Logger.Warn("Failed to scan assembly {0}", path);
                return;
            }

            var isReferencingGaugeLib = assembly.GetReferencedAssemblies()
                .Select(name => name.Name)
                .Contains(typeof(Step).Assembly.GetName().Name);

            var loadableTypes = isReferencingGaugeLib ? GetLoadableTypes(assembly) : new List<Type>();

            // Load assembly so that code can be executed
            var fullyLoadedAssembly = Assembly.Load(AssemblyName.GetAssemblyName(path));
            var types = GetFullyLoadedTypes(loadableTypes, fullyLoadedAssembly);

            if (isReferencingGaugeLib)
                AssembliesReferencingGaugeLib.Add(fullyLoadedAssembly);

            ScanForScreengrabber(types);
            ScanForMethodAttributes(types);
        }

        private IEnumerable<Type> GetFullyLoadedTypes(IEnumerable<Type> loadableTypes, Assembly fullyLoadedAssembly)
        {
            foreach (var type in loadableTypes)
                yield return fullyLoadedAssembly.GetType(type.FullName);
        }

        private void ScanForMethodAttributes(IEnumerable<Type> types)
        {
            foreach (var type in AttributeToMethodInfo.Keys)
            {
                var methodInfos = types
                    .SelectMany(t => t.GetMethods().Where(info => info.GetCustomAttributes(type).Any()));
                AttributeToMethodInfo[type].AddRange(methodInfos);
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
    }
}
