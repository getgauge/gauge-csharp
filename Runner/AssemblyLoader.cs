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
using System.IO;
using System.Linq;
using System.Reflection;
using Gauge.CSharp.Runner.Exceptions;
using Gauge.CSharp.Runner.Wrappers;
using NLog;

namespace Gauge.CSharp.Runner
{
    public class AssemblyLoader : IAssemblyLoader
    {
        private const string GaugeLibAssembleName = "Gauge.CSharp.Lib";
        private readonly IAssemblyWrapper _assemblyWrapper;
        private readonly IFileWrapper _fileWrapper;
        private readonly Version _minimumLibversion = new Version("0.7.2");
        private Assembly _targetLibAssembly;

        public AssemblyLoader(string runnerBasePath, IAssemblyWrapper assemblyWrapper, IFileWrapper fileWrapper,
            IEnumerable<string> assemblyLocations)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var logger = LogManager.GetLogger("AssemblyLoader");
                logger.Debug("Loading {0}", args.Name);
                try
                {
                    var assemblyName = args.Name.Split(',').FirstOrDefault();
                    var gaugeBinDir = AssemblyLocater.GetGaugeBinDir();

                    var probePath = Path.GetFullPath(Path.Combine(gaugeBinDir, string.Format("{0}.dll", assemblyName)));
                    if (File.Exists(probePath)) return Assembly.LoadFrom(probePath);

                    probePath = Path.GetFullPath(Path.Combine(runnerBasePath, string.Format("{0}.dll", assemblyName)));

                    if (File.Exists(probePath)) return Assembly.LoadFrom(probePath);

                    var executingAssembly = Assembly.GetExecutingAssembly();
                    return executingAssembly.GetName().Name == assemblyName ? executingAssembly : null;
                }
                catch (Exception e)
                {
                    logger.Error(e);
                    return null;
                }
            };
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (sender, args) =>
            {
                var logger = LogManager.GetLogger("AssemblyLoader");
                logger.Debug("Reflection only Loading {0}", args.Name);
                try
                {
                    var assemblyName = args.Name.Split(',').FirstOrDefault();
                    var gaugeBinDir = AssemblyLocater.GetGaugeBinDir();

                    var probePath = Path.GetFullPath(Path.Combine(gaugeBinDir, string.Format("{0}.dll", assemblyName)));
                    if (File.Exists(probePath)) return Assembly.ReflectionOnlyLoadFrom(probePath);

                    probePath = Path.GetFullPath(Path.Combine(runnerBasePath, string.Format("{0}.dll", assemblyName)));

                    return File.Exists(probePath) ? Assembly.ReflectionOnlyLoadFrom(probePath) : null;
                }
                catch (Exception e)
                {
                    logger.Error(e);
                    return null;
                }
            };
            _assemblyWrapper = assemblyWrapper;
            _fileWrapper = fileWrapper;
            LoadTargetLibAssembly();
            AssembliesReferencingGaugeLib = new List<Assembly>();
            ScreengrabberTypes = new List<Type>();
            ClassInstanceManagerTypes = new List<Type>();
            foreach (var location in assemblyLocations)
                ScanAndLoad(location);
        }

        public AssemblyLoader(string runnerBasePath, IEnumerable<string> assemblyLocations)
            : this(runnerBasePath, new AssemblyWrapper(), new FileWrapper(), assemblyLocations)
        {
        }

        public AssemblyLoader(string runnerBasePath)
            : this(runnerBasePath, new AssemblyLocater(new DirectoryWrapper(), new FileWrapper()).GetAllAssemblies())
        {
        }

        public List<Assembly> AssembliesReferencingGaugeLib { get; }
        public List<Type> ScreengrabberTypes { get; }
        public List<Type> ClassInstanceManagerTypes { get; }

        public Assembly GetTargetLibAssembly()
        {
            return _targetLibAssembly;
        }

        public List<MethodInfo> GetMethods(string annotation)
        {
            Func<MethodInfo, bool> methodFilter = info => info.GetCustomAttributes()
                .Any(a => a.GetType().FullName.Equals(annotation));
            Func<Type, IEnumerable<MethodInfo>> methodSelector = t => t.GetMethods().Where(methodFilter);
            return AssembliesReferencingGaugeLib.SelectMany(assembly => assembly.GetTypes().SelectMany(methodSelector))
                .ToList();
        }

        private void ScanAndLoad(string path)
        {
            var logger = LogManager.GetLogger("AssemblyLoader");
            logger.Debug("Loading assembly from : {0}", path);
            Assembly assembly;
            try
            {
                // Load assembly for reflection only to avoid exceptions when referenced assemblyLocations cannot be found
                assembly = _assemblyWrapper.ReflectionOnlyLoadFrom(path);
            }
            catch
            {
                logger.Warn("Failed to scan assembly {0}", path);
                return;
            }

            var isReferencingGaugeLib = assembly.GetReferencedAssemblies()
                .Select(name => name.Name)
                .Contains(GaugeLibAssembleName);

            var loadableTypes = new HashSet<Type>(isReferencingGaugeLib ? GetLoadableTypes(assembly) : new Type[] { });

            // Load assembly so that code can be executed
            var fullyLoadedAssembly = _assemblyWrapper.LoadFrom(path);
            var types = GetFullyLoadedTypes(loadableTypes, fullyLoadedAssembly).ToList();

            if (isReferencingGaugeLib)
                AssembliesReferencingGaugeLib.Add(fullyLoadedAssembly);

            ScanForScreengrabber(types);
            ScanForInstanceManager(types);
        }

        private IEnumerable<Type> GetFullyLoadedTypes(IEnumerable<Type> loadableTypes, Assembly fullyLoadedAssembly)
        {
            foreach (var type in loadableTypes)
            {
                var fullyLoadedType = fullyLoadedAssembly.GetType(type.FullName);
                if (fullyLoadedType != null)
                    yield return fullyLoadedType;
            }
        }

        private void ScanForScreengrabber(IEnumerable<Type> types)
        {
            var implementingTypes = types.Where(type =>
                type.GetInterfaces().Any(t => t.FullName == "Gauge.CSharp.Lib.IScreenGrabber" 
                    || t.FullName == "Gauge.CSharp.Lib.ICustomScreenshotGrabber"));
            ScreengrabberTypes.AddRange(implementingTypes);
        }

        private void ScanForInstanceManager(IEnumerable<Type> types)
        {
            var implementingTypes = types.Where(type =>
                type.GetInterfaces().Any(t => t.FullName == "Gauge.CSharp.Lib.IClassInstanceManager"));
            ClassInstanceManagerTypes.AddRange(implementingTypes);
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                LogManager.GetLogger("AssemblyLoader")
                    .Warn("Could not scan all types in assembly {0}", assembly.CodeBase);
                return e.Types.Where(type => type != null);
            }
        }

        private void LoadTargetLibAssembly()
        {
            var targetLibLocation = Path.GetFullPath(Path.Combine(AssemblyLocater.GetGaugeBinDir(),
                string.Concat(GaugeLibAssembleName, ".dll")));
            var logger = LogManager.GetLogger("AssemblyLoader");
            if (!_fileWrapper.Exists(targetLibLocation))
            {
                var message = string.Format("Unable to locate Gauge Lib at: {0}", targetLibLocation);
                logger.Error(message);
                throw new FileNotFoundException(message);
            }
            _targetLibAssembly = _assemblyWrapper.LoadFrom(targetLibLocation);
            var targetLibVersion = _targetLibAssembly.GetName().Version;
            if (targetLibVersion <= _minimumLibversion)
                throw new GaugeLibVersionMismatchException(targetLibVersion, _minimumLibversion);

            logger.Debug("Target Lib loaded : {0}, from {1}", _targetLibAssembly.FullName, _targetLibAssembly.Location);
        }
    }
}