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
using System.IO;
using System.Linq;
using Fake;
using Gauge.CSharp.Runner.Exceptions;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using NLog;
using Utils = Gauge.CSharp.Core.Utils;

namespace Gauge.CSharp.Runner
{
    public class GaugeProjectBuilder : IGaugeProjectBuilder
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public bool BuildTargetGaugeProject()
        {
            var projectFullPath = GetProjectFullPath();
            if (!File.Exists(projectFullPath))
            {
                Logger.Error("Error: Project file path {0} doesn't exist. " +
                    "Check GAUGE_CSHARP_PROJECT_FILE in environment properties.", projectFullPath);
                return false;
            }

            var projectConfig = GetProjectConfiguration();
            var projectPlatform = GetProjectPlatform();
            var gaugeBinDir = Utils.GetGaugeBinDir();
            try
            {
                var properties = new FSharpList<Tuple<string, string>>(Tuple.Create("Configuration", projectConfig),
                    FSharpList<Tuple<string, string>>.Empty);
                properties = new FSharpList<Tuple<string, string>>(Tuple.Create("Platform", projectPlatform), properties);
                properties = new FSharpList<Tuple<string, string>>(Tuple.Create("OutputPath", gaugeBinDir), properties);
                MSBuildHelper.build(FuncConvert.ToFSharpFunc(delegate(MSBuildHelper.MSBuildParams input)
                {
                    input.Verbosity =
                        FSharpOption<MSBuildHelper.MSBuildVerbosity>.Some(MSBuildHelper.MSBuildVerbosity.Quiet);
                    input.Targets = new FSharpList<string>("Build", FSharpList<string>.Empty);
                    input.Properties = properties;
                    return input;
                }), projectFullPath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "C# Project build failed {0}", ex.Message);
                return false;
            }
            return true;
        }

        private static string GetProjectFullPath()
        {
            var csprojEnvVariable = Utils.TryReadEnvValue("GAUGE_CSHARP_PROJECT_FILE");
            if (!string.IsNullOrEmpty(csprojEnvVariable))
                return csprojEnvVariable;

            var projectFileList = Directory.GetFiles(Utils.GaugeProjectRoot, "*.csproj");

            if (!projectFileList.Any())
                throw new NotAValidGaugeProjectException();
            var projectFullPath = projectFileList.First();
            return projectFullPath;
        }

        private static string GetProjectConfiguration()
        {
            var csprojEnvVariable = Utils.TryReadEnvValue("GAUGE_CSHARP_PROJECT_CONFIG");
            if (!string.IsNullOrEmpty(csprojEnvVariable))
                return csprojEnvVariable;

            return "Debug";
        }

        private static string GetProjectPlatform()
        {
            var csprojEnvVariable = Utils.TryReadEnvValue("GAUGE_CSHARP_PROJECT_PLATFORM");
            if (!string.IsNullOrEmpty(csprojEnvVariable))
                return csprojEnvVariable;

            return "Any CPU";
        }
    }
}
