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

using Gauge.CSharp.Lib;
using Gauge.CSharp.Lib.Attribute;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Gauge.CSharp.Core;
using Gauge.CSharp.Runner.Wrappers;

namespace Gauge.CSharp.Runner
{
    public class AssemblyLoader : IAssemblyLoader
    {
        private static readonly string GaugeLibAssembleName = typeof(Step).Assembly.GetName().Name;

        private static readonly Logger Logger = LogManager.GetLogger("AssemblyLoader");

        public List<Assembly> AssembliesReferencingGaugeLib { get; private set; }
        public List<Type> ScreengrabberTypes { get; private set; }
        private Assembly _targetLibAssembly;
        private readonly IAssemblyWrapper _assemblyWrapper;
        private readonly IFileWrapper _fileWrapper;

        public AssemblyLoader(IAssemblyWrapper assemblyWrapper, IFileWrapper fileWrapper, IEnumerable<string> assemblyLocations)
        {
            _assemblyWrapper = assemblyWrapper;
            _fileWrapper = fileWrapper;
            LoadTargetLibAssembly();
            AssembliesReferencingGaugeLib= new List<Assembly>();
            ScreengrabberTypes = new List<Type>();
            foreach (var location in assemblyLocations)
            {
                ScanAndLoad(location);
            }
        }

        public AssemblyLoader(IEnumerable<string> assemblyLocations)
            : this(new AssemblyWrapper(), new FileWrapper(), assemblyLocations)
        {
        }

        public Assembly GetTargetLibAssembly()
        {
            return _targetLibAssembly;
        }

        public List<MethodInfo> GetMethods(Type annotationType)
        {
            Func<MethodInfo, bool> methodFilter = info => info.GetCustomAttributes()
				.Any(a => a.GetType().FullName.Equals(annotationType.FullName));
            Func<Type, IEnumerable<MethodInfo>> methodSelector = t => t.GetMethods().Where(methodFilter);
            return AssembliesReferencingGaugeLib.SelectMany(assembly => assembly.GetTypes().SelectMany(methodSelector)).ToList();
        }

        private void ScanAndLoad(string path)
        {
            Logger.Debug("Loading assembly from : {0}", path);
            Assembly assembly;
            try
            {
                // Load assembly for reflection only to avoid exceptions when referenced assemblyLocations cannot be found
                assembly = _assemblyWrapper.ReflectionOnlyLoadFrom(path);
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
            var fullyLoadedAssembly = _assemblyWrapper.LoadFrom(path);
            var types = GetFullyLoadedTypes(loadableTypes, fullyLoadedAssembly).ToList();

            if (isReferencingGaugeLib)
                AssembliesReferencingGaugeLib.Add(fullyLoadedAssembly);

            ScanForScreengrabber(types);
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

        private void ScanForScreengrabber(IEnumerable<Type> types)
        {
            var implementingTypes = types.Where(type => type.GetInterfaces().Any(t => t.FullName == typeof(IScreenGrabber).FullName));
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
            if (!_fileWrapper.Exists(targetLibLocation))
            {
                var message = string.Format("Unable to locate Gauge Lib at: {0}", targetLibLocation);
                Logger.Error(message);
                throw new FileNotFoundException(message);
            }
            _targetLibAssembly = _assemblyWrapper.LoadFrom(targetLibLocation);
            Logger.Debug("Target Lib loaded : {0}, from {1}", _targetLibAssembly.FullName, _targetLibAssembly.Location);
        }

        private Type GetTypeFromTargetLib(Type type)
        {
            return _targetLibAssembly.GetType(type.FullName);
        }
    }
}
