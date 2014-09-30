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
    internal class StartPhaseExecutor : IPhaseExecutor
    {
        public void Execute()
        {
            BuildTargetGaugeProject();
            using (var gaugeConnection = new GaugeConnection(new TcpClientWrapper(Utils.GaugePort)))
            {
                while (gaugeConnection.Connected)
                {
                    var messageBytes = gaugeConnection.ReadBytes();
                    var message = Message.ParseFrom(messageBytes.ToArray());
                    var processor = MessageProcessorFactory.GetProcessor(message.MessageType);
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
            var consoleLogger = new ConsoleLogger(LoggerVerbosity.Minimal);
            var projectFileList = Directory.GetFiles(Utils.GaugeProjectRoot, "*.csproj");

            if (!projectFileList.Any())
            {
                Console.Out.WriteLine("Cannot locate a Project File in {0}", Utils.GaugeProjectRoot);
                Environment.Exit(0);
            }
            var projectFullPath = projectFileList.First();

            Console.WriteLine("Building Project: {0}", projectFullPath);
            var pc = new ProjectCollection();
            var globalProperty = new Dictionary<string, string> {{"Configuration", "Release"}, {"Platform", "Any CPU"}, {"OutputPath", Utils.GaugeBinDir}};

            var buildRequestData = new BuildRequestData(projectFullPath, globalProperty, null, new[] {"Rebuild"}, null);

            var buildParameters = new BuildParameters(pc) {Loggers = new[] {consoleLogger}};

            BuildManager.DefaultBuildManager.Build(buildParameters, buildRequestData);
        }
    }
}