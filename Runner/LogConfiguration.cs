using System;
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
            fileTarget.FileName = "${basedir}/log/gauge.log";
            fileTarget.Layout = "${message}";

            var logLevel = Environment.GetEnvironmentVariable("GAUGE_LOG_LEVEL");

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
    }
}
