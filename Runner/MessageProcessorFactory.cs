using System.Collections.Generic;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Runner.Processors;
using Gauge.Messages;

namespace Gauge.CSharp.Runner
{
    public class MessageProcessorFactory
    {
        private Dictionary<Message.Types.MessageType, IMessageProcessor> _messageProcessorsDictionary;

        public MessageProcessorFactory()
        {
            using (var apiConnection = new GaugeApiConnection(new TcpClientWrapper(Utils.GaugeApiPort)))
            {
                InitializeProcessors(new MethodScanner(apiConnection));
            }
        }

        public MessageProcessorFactory(IMethodScanner stepScanner)
        {
            InitializeProcessors(stepScanner);
        }

        public IMessageProcessor GetProcessor(Message.Types.MessageType messageType)
        {
            return _messageProcessorsDictionary.ContainsKey(messageType) ? _messageProcessorsDictionary[messageType] : new DefaultProcessor();
        }


        private static Dictionary<Message.Types.MessageType, IMessageProcessor> InitializeMessageHandlers(IStepRegistry stepRegistry,
            IHookRegistry hookRegistry)
        {
            var methodExecutor = new MethodExecutor();
            var messageHandlers = new Dictionary<Message.Types.MessageType, IMessageProcessor>
            {
                {Message.Types.MessageType.ExecutionStarting, new ExecutionStartingProcessor(hookRegistry, methodExecutor)},
                {Message.Types.MessageType.ExecutionEnding, new ExecutionEndingProcessor(hookRegistry, methodExecutor)},
                {Message.Types.MessageType.SpecExecutionStarting, new SpecExecutionStartingProcessor(hookRegistry)},
                {Message.Types.MessageType.SpecExecutionEnding, new SpecExecutionEndingProcessor(hookRegistry)},
                {Message.Types.MessageType.ScenarioExecutionStarting, new ScenarioExecutionStartingProcessor(hookRegistry)},
                {Message.Types.MessageType.ScenarioExecutionEnding, new ScenarioExecutionEndingProcessor(hookRegistry)},
                {Message.Types.MessageType.StepExecutionStarting, new StepExecutionStartingProcessor(hookRegistry)},
                {Message.Types.MessageType.StepExecutionEnding, new StepExecutionEndingProcessor(hookRegistry)},
                {Message.Types.MessageType.ExecuteStep, new ExecuteStepProcessor(stepRegistry)},
                {Message.Types.MessageType.KillProcessRequest, new KillProcessProcessor()},
                {Message.Types.MessageType.StepNamesRequest, new StepNamesProcessor(stepRegistry)},
                {Message.Types.MessageType.StepValidateRequest, new StepValidationProcessor(stepRegistry)},
                {Message.Types.MessageType.ScenarioDataStoreInit, new DataStoreInitProcessor()},
                {Message.Types.MessageType.SpecDataStoreInit, new DataStoreInitProcessor()},
                {Message.Types.MessageType.SuiteDataStoreInit, new DataStoreInitProcessor()},
            };
            return messageHandlers;
        }

        private void InitializeProcessors(IMethodScanner stepScanner)
        {
            var stepRegistry = stepScanner.GetStepRegistry();
            var hookRegistry = stepScanner.GetHookRegistry();
            _messageProcessorsDictionary = InitializeMessageHandlers(stepRegistry, hookRegistry);
        }
    }
}