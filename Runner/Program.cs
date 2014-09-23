using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Runner.Processors;
using main;

namespace Gauge.CSharp.Runner
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            using (var gaugeConnection = new GaugeConnection(Convert.ToInt32(Utils.GaugePort)))
            {
                var messageDispacher = GetMessageDispacher();
                while (gaugeConnection.Connected)
                {
                    var messageBytes = gaugeConnection.ReadFromStream();
                    var message = Message.ParseFrom(messageBytes);
                    if (messageDispacher.ContainsKey(message.MessageType))
                    {
                        var response = messageDispacher[message.MessageType].Process(message);
                        gaugeConnection.WriteResponse(response);
                        if (message.MessageType == Message.Types.MessageType.KillProcessRequest)
                        {
                            return;
                        }
                    }
                    else
                    {
                        var response = new DefaultProcessor().Process(message);
                        gaugeConnection.WriteResponse(response);
                    }
                }
            }
        }

        private static Dictionary<Message.Types.MessageType, IMessageProcessor> GetMessageDispacher()
        {
            using (var apiConnection = new GaugeApiConnection(Convert.ToInt32(Utils.GaugeApiPort)))
            {
                var stepRegistry = new StepRegistry(ScanSteps(apiConnection));
                var messageDispacher = InitializeMessageHandlers(stepRegistry);
                return messageDispacher;                
            }
        }

        private static Dictionary<Message.Types.MessageType, IMessageProcessor> InitializeMessageHandlers(
            StepRegistry stepRegistry)
        {
            var messageHandlers = new Dictionary<Message.Types.MessageType, IMessageProcessor>
            {
                {Message.Types.MessageType.ExecuteStep, new ExecuteStepProcessor(stepRegistry)},
                {Message.Types.MessageType.KillProcessRequest, new KillProcessProcessor()},
                {Message.Types.MessageType.StepNamesRequest, new StepNamesProcessor(stepRegistry)},
                {Message.Types.MessageType.StepValidateRequest, new StepValidationProcessor(stepRegistry)}
            };
            return messageHandlers;
        }

        private static Hashtable ScanSteps(GaugeApiConnection apiConnection)
        {
            var hashtable = new Hashtable();
            var enumerateFiles = Directory.EnumerateFiles(Utils.GaugeProjectRoot, "*.dll", SearchOption.AllDirectories);
            foreach (var specAssembly in enumerateFiles)
            {
                var assembly = Assembly.LoadFile(specAssembly);
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var method in type.GetMethods())
                    {
                        var step = method.GetCustomAttributes<Step>(false);
                        foreach (var stepValue in step.SelectMany(s => apiConnection.GetStepValue(s.Names, false)))
                        {
                            hashtable.Add(stepValue, method);
                        }
                    }
                }
            }
            return hashtable;
        }
    }
}