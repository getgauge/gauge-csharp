// Copyright 2015 ThoughtWorks, Inc.

// This file is part of Gauge-CSharp.

// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

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
    internal static class GaugeBuildManager
    {
        internal static void BuildTargetGaugeProject(Logger logger)
        {
            var consoleLogger = new ConsoleLogger(LoggerVerbosity.Quiet);
            var solutionFileList = Directory.GetFiles(Utils.GaugeProjectRoot, "*.sln");

            if (!solutionFileList.Any())
            {
                throw new NotAValidGaugeProjectException();
            }
            var solutionFullPath = solutionFileList.First();

            if (!Directory.Exists(Utils.GetGaugeBinDir()))
            {
                Directory.CreateDirectory(Utils.GetGaugeBinDir());
            }

            logger.Info("Building Project: {0}", solutionFullPath);
            var pc = new ProjectCollection();
            var globalProperty = new Dictionary<string, string>
            {
                {"Configuration", "Release"},
                {"Platform", "Any CPU"},
                {"OutputPath", Utils.GetGaugeBinDir()}
            };

            var buildRequestData = new BuildRequestData(solutionFullPath, globalProperty, null, new[] {"Build"}, null);

            var errorCodeAggregator = new ErrorCodeAggregator();
            var buildParameters = new BuildParameters(pc) {Loggers = new ILogger[] {consoleLogger, errorCodeAggregator}};

            var buildResult = BuildManager.DefaultBuildManager.Build(buildParameters, buildRequestData);

            if (errorCodeAggregator.ErrorCodes.Contains("CS1001"))
            {
                logger.Error("You have chosen an invalid folder name to initialize a Gauge project.");
                logger.Error("Please choose a project name that complies with C# Project naming conventions.");
            }

            logger.Info(buildResult.OverallResult);

            if (buildResult.OverallResult == BuildResultCode.Success) return;

            logger.Error("Build of solution ({0}) failed.", solutionFullPath);
            Environment.Exit(1);
        }
    }
}