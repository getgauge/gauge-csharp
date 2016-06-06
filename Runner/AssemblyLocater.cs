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
using Gauge.CSharp.Core;
using Gauge.CSharp.Runner.Wrappers;
using NLog;

namespace Gauge.CSharp.Runner
{
    public class AssemblyLocater : IAssemblyLocater
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDirectoryWrapper _directoryWrapper;
        private readonly IFileWrapper _fileWrapper;

        public AssemblyLocater(IDirectoryWrapper directoryWrapper, IFileWrapper fileWrapper)
        {
            _directoryWrapper = directoryWrapper;
            _fileWrapper = fileWrapper;
        }

        public IEnumerable<string> GetAllAssemblies()
        {
            var assemblies = _directoryWrapper.EnumerateFiles(Utils.GetGaugeBinDir(), "*.dll", SearchOption.TopDirectoryOnly).ToList();
            var gaugeAdditionalLibsPath = Utils.TryReadEnvValue("GAUGE_ADDITIONAL_LIBS");
            if (string.IsNullOrEmpty(gaugeAdditionalLibsPath))
                return assemblies;

            var additionalLibPaths = gaugeAdditionalLibsPath.Split(',').Select(s => Path.GetFullPath(s.Trim()));
            foreach (var libPath in additionalLibPaths)
            {
                if (Path.HasExtension(libPath))
                {
                    AddFile(libPath, assemblies);
                    continue;
                }
                AddFilesFromDirectory(libPath, assemblies);
            }
            return assemblies;
        }

        private void AddFilesFromDirectory(string path, List<string> assemblies)
        {
            if (!_directoryWrapper.Exists(path))
            {
                Logger.Warn("Path does not exist: {0}", path);
                return;
            }
            assemblies.AddRange(_directoryWrapper.EnumerateFiles(path, "*.dll", SearchOption.TopDirectoryOnly));
        }

        private void AddFile(string path, List<string> assemblies)
        {
            if (!_fileWrapper.Exists(path))
            {
                Logger.Warn("Path does not exist: {0}", path);
                return;
            }
            assemblies.Add(path);
        }
    }
}