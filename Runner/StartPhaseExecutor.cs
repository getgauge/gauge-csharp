using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gauge.CSharp.Lib;
using main;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace Gauge.CSharp.Runner
{
    public class StartPhaseExecutor : IPhaseExecutor
    {
        private readonly MessageProcessorFactory _messageProcessorFactory;

        public static StartPhaseExecutor DefaultInstance = new StartPhaseExecutor();
        private StartPhaseExecutor()
        {
            _messageProcessorFactory = new MessageProcessorFactory();
        }

        public void Execute()
        {
            BuildTargetGaugeProject();
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
        
        private static void BuildTargetGaugeProject()
        {
            var consoleLogger = new ConsoleLogger(LoggerVerbosity.Quiet);
            var solutionFileList = Directory.GetFiles(Utils.GaugeProjectRoot, "*.sln");

            if (!solutionFileList.Any())
            {
                Console.Out.WriteLine("Cannot locate a Project File in {0}", Utils.GaugeProjectRoot);
                Environment.Exit(0);
            }
            var solutionFullPath = solutionFileList.First();
            Directory.CreateDirectory(Utils.GaugeBinDir);
            Console.WriteLine("Building Project: {0}", solutionFullPath);
            var pc = new ProjectCollection();
            var globalProperty = new Dictionary<string, string> {{"Configuration", "Release"}, {"Platform", "Any CPU"}, {"OutputPath", Utils.GaugeBinDir}};

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