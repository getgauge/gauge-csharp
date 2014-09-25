using System;
using System.Collections.Generic;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Runner.Processors;
using main;

namespace Gauge.CSharp.Runner
{
    public static class MessageProcessorFactory
    {
        private static readonly Dictionary<Message.Types.MessageType, IMessageProcessor> MessageProcessorsDictionary = InitializeProcessors();

        public static IMessageProcessor GetProcessor(Message.Types.MessageType messageType)
        {
            return MessageProcessorsDictionary.ContainsKey(messageType) ? MessageProcessorsDictionary[messageType] : new DefaultProcessor();
        }


        private static Dictionary<Message.Types.MessageType, IMessageProcessor> InitializeProcessors()
        {
            using (var apiConnection = new GaugeApiConnection(new TcpClientWrapper(Utils.GaugeApiPort)))
            {
                var stepScanner = new StepScanner(apiConnection);
                var stepRegistry = stepScanner.CreateStepRegistry();
                return InitializeMessageHandlers(stepRegistry);
            }
        }

        private static Dictionary<Message.Types.MessageType, IMessageProcessor> InitializeMessageHandlers(StepRegistry stepRegistry)
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
    }
}