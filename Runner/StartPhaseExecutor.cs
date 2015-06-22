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
using System.Diagnostics;
using System.IO;
using System.Linq;
using Gauge.CSharp.Runner.Communication;
using Gauge.CSharp.Runner.Exceptions;
using Gauge.Messages;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace Gauge.CSharp.Runner
{
    public class StartPhaseExecutor : IPhaseExecutor
    {
        private readonly MessageProcessorFactory _messageProcessorFactory;

        private static StartPhaseExecutor _instance;
        private static bool _shouldBuildProject = true;

        public static StartPhaseExecutor GetDefaultInstance()
        {
            return _instance ?? (_instance = new StartPhaseExecutor());
        }

        private StartPhaseExecutor()
        {
            if (_shouldBuildProject)
            {
                try
                {
                    BuildTargetGaugeProject();
                    _shouldBuildProject = false;
                }
                catch (NotAValidGaugeProjectException)
                {
                    Console.Out.WriteLine("Cannot locate a Project File in {0}", Utils.GaugeProjectRoot);
                    Environment.Exit(1);
                }
            }
            var sandbox = SandboxFactory.Create();
            _messageProcessorFactory = new MessageProcessorFactory(sandbox);
        }

        [DebuggerHidden]
        public void Execute()
        {
            try
            {
                using (var gaugeConnection = new GaugeConnection(new TcpClientWrapper(Utils.GaugePort)))
                {
                    while (gaugeConnection.Connected)
                    {
                        var messageBytes = gaugeConnection.ReadBytes();
                        var message = Message.ParseFrom(messageBytes.ToArray());

                        var processor = _messageProcessorFactory.GetProcessor(message.MessageType);
                        var response = processor.Process(message);
                        gaugeConnection.WriteMessage(response);
                        if (message.MessageType == Message.Types.MessageType.KillProcessRequest)
                        {
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WARN] Exception thrown: {0}", ex);
            }
        }

        private static void BuildTargetGaugeProject()
        {
            var consoleLogger = new ConsoleLogger(LoggerVerbosity.Quiet);
            var solutionFileList = Directory.GetFiles(Utils.GaugeProjectRoot, "*.sln");

            if (!solutionFileList.Any())
            {
                throw new NotAValidGaugeProjectException();
            }
            var solutionFullPath = solutionFileList.First();
            Directory.CreateDirectory(Utils.GaugeBinDir);
            Console.WriteLine("Building Project: {0}", solutionFullPath);
            var pc = new ProjectCollection();
            var globalProperty = new Dictionary<string, string>
            {
                {"Configuration", "Release"},
                {"Platform", "Any CPU"},
                {"OutputPath", Utils.GaugeBinDir}
            };

            var buildRequestData = new BuildRequestData(solutionFullPath, globalProperty, null, new[] {"Build"}, null);

            var errorCodeAggregator = new ErrorCodeAggregator();
            var buildParameters = new BuildParameters(pc) {Loggers = new ILogger[] {consoleLogger, errorCodeAggregator}};

            var buildResult = BuildManager.DefaultBuildManager.Build(buildParameters, buildRequestData);

            if (buildResult.OverallResult == BuildResultCode.Success)
            {
                Console.Out.WriteLine(buildResult.OverallResult);
                return;
            }
            if (errorCodeAggregator.ErrorCodes.Contains("CS1001"))
            {
                Console.WriteLine();
                Console.WriteLine("You have chosen an invalid folder name to initialize a Gauge project.");
                Console.WriteLine("Please choose a project name that complies with C# Project naming conventions.");
                Console.WriteLine();
            }
            Environment.Exit(1);
        }
    }
}