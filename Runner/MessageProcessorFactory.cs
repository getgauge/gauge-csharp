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
                var stepScanner = new MethodScanner(apiConnection);
                var stepRegistry = stepScanner.GetStepRegistry();
                return InitializeMessageHandlers(stepRegistry);
            }
        }

        private static Dictionary<Message.Types.MessageType, IMessageProcessor> InitializeMessageHandlers(StepRegistry stepRegistry)
        {
            var messageHandlers = new Dictionary<Message.Types.MessageType, IMessageProcessor>
            {
                {Message.Types.MessageType.ExecutionStarting, new ExecutionStartingProcessor()},
                {Message.Types.MessageType.ExecutionEnding, new ExecutionEndingProcessor()},
                {Message.Types.MessageType.SpecExecutionStarting, new SpecExecutionStartingProcessor()},
                {Message.Types.MessageType.SpecExecutionEnding, new SpecExecutionEndingProcessor()},
                {Message.Types.MessageType.ScenarioExecutionStarting, new ScenarioExecutionStartingProcessor()},
                {Message.Types.MessageType.ScenarioExecutionEnding, new ScenarioExecutionEndingProcessor()},
                {Message.Types.MessageType.StepExecutionStarting, new StepExecutionStartingProcessor()},
                {Message.Types.MessageType.StepExecutionEnding, new StepExecutionEndingProcessor()},
                {Message.Types.MessageType.ExecuteStep, new ExecuteStepProcessor(stepRegistry)},
                {Message.Types.MessageType.KillProcessRequest, new KillProcessProcessor()},
                {Message.Types.MessageType.StepNamesRequest, new StepNamesProcessor(stepRegistry)},
                {Message.Types.MessageType.StepValidateRequest, new StepValidationProcessor(stepRegistry)}
            };
            return messageHandlers;
        }
    }
}