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
using Gauge.CSharp.Core;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Gauge.CSharp.Runner
{
    class LogConfiguration
    {
        internal static void Initialize()
        {
            var config = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget();
            config.AddTarget("console", consoleTarget);

            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            consoleTarget.Layout = @"${logger} ${message}";

            fileTarget.FileName = GetLogFilePath();
            fileTarget.Layout = "${message}";

            var logLevel = Utils.TryReadEnvValue("GAUGE_LOG_LEVEL");

            if (string.IsNullOrEmpty(logLevel))
            {
                logLevel = "INFO";
            }
            var level = LogLevel.FromString(logLevel.Trim());
            var consoleRule = new LoggingRule("*", level, consoleTarget);
            config.LoggingRules.Add(consoleRule);

            var fileRule = new LoggingRule("*", level, fileTarget);
            config.LoggingRules.Add(fileRule);

            LogManager.Configuration = config;
        }

        private static string GetLogFilePath()
        {
            var logDir = Utils.TryReadEnvValue("LOGS_DIRECTORY");
            if (string.IsNullOrEmpty(logDir))
            {
                logDir = Path.Combine(Utils.GaugeProjectRoot, "logs");
            }
            else
            {
                if (!Path.IsPathRooted(logDir))
                {
                    logDir = Path.Combine(Utils.GaugeProjectRoot, logDir);
                }
            }
            return Path.GetFullPath(Path.Combine(logDir, "gauge.log"));
        }
    }
}
