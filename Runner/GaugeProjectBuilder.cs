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
using Gauge.CSharp.Runner.Exceptions;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using NLog;
using ILogger = Microsoft.Build.Framework.ILogger;

namespace Gauge.CSharp.Runner
{
    public class GaugeProjectBuilder : IGaugeProjectBuilder
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public bool BuildTargetGaugeProject()
        {
            var consoleLogger = new ConsoleLogger(LoggerVerbosity.Quiet);
            var projectFullPath = GetProjectFullPath();
            var gaugeBinDir = Utils.GetGaugeBinDir();
            try
            {
                Logger.Debug("Create Gauge Bin Directory: {0}", gaugeBinDir);
                Directory.CreateDirectory(gaugeBinDir);
            }
            catch (IOException ex)
            {
                Logger.Fatal(ex, "Unable to create Gauge Bin Directory in {0}", gaugeBinDir);
                throw;
            }
            Logger.Info("Building Project: {0}", projectFullPath);
            var pc = new ProjectCollection();
            var globalProperty = new Dictionary<string, string>
            {
                {"Configuration", "Release"},
                {"Platform", "Any CPU"},
                {"OutputPath", gaugeBinDir}
            };

            var buildRequestData = new BuildRequestData(projectFullPath, globalProperty, null, new[] {"Build"}, null);

            var errorCodeAggregator = new ErrorCodeAggregator();
            var buildParameters = new BuildParameters(pc) {Loggers = new ILogger[] {consoleLogger, errorCodeAggregator}};

            var buildResult = BuildManager.DefaultBuildManager.Build(buildParameters, buildRequestData);

            if (errorCodeAggregator.ErrorCodes.Contains("CS1001"))
            {
                Logger.Error("You have chosen an invalid folder name to initialize a Gauge project.");
                Logger.Error("Please choose a project name that complies with C# Project naming conventions.");
            }

            Logger.Info(buildResult.OverallResult);
            return buildResult.OverallResult == BuildResultCode.Success;
        }

        private static string GetProjectFullPath()
        {
            var csprojEnvVariable = Utils.TryReadEnvValue("GAUGE_CSHARP_PROJECT_FILE");
            if (!string.IsNullOrEmpty(csprojEnvVariable))
            {
                return csprojEnvVariable;
            }

            var projectFileList = Directory.GetFiles(Utils.GaugeProjectRoot, "*.csproj");

            if (!projectFileList.Any())
            {
                throw new NotAValidGaugeProjectException();
            }
            var projectFullPath = projectFileList.First();
            return projectFullPath;
        }
    }
}