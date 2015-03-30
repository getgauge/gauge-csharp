using System.Collections.Generic;
using Gauge.CSharp.Runner.Communication;
using Gauge.CSharp.Runner.Processors;
using Gauge.Messages;

namespace Gauge.CSharp.Runner
{
    public class MessageProcessorFactory
    {
        private readonly ISandbox _sandbox;
        private Dictionary<Message.Types.MessageType, IMessageProcessor> _messageProcessorsDictionary;

        public MessageProcessorFactory()
        {
            _sandbox = new Sandbox();
            using (var apiConnection = new GaugeApiConnection(new TcpClientWrapper(Utils.GaugeApiPort)))
            {
                InitializeProcessors(new MethodScanner(apiConnection));
            }
        }

        public MessageProcessorFactory(IMethodScanner stepScanner, ISandbox sandbox)
        {
            _sandbox = sandbox;
            InitializeProcessors(stepScanner);
        }

        public IMessageProcessor GetProcessor(Message.Types.MessageType messageType)
        {
            return _messageProcessorsDictionary.ContainsKey(messageType) ? _messageProcessorsDictionary[messageType] : new DefaultProcessor();
        }


        private Dictionary<Message.Types.MessageType, IMessageProcessor> InitializeMessageHandlers(IStepRegistry stepRegistry,
            IHookRegistry hookRegistry)
        {
            var methodExecutor = new MethodExecutor(_sandbox);
            var messageHandlers = new Dictionary<Message.Types.MessageType, IMessageProcessor>
            {
                {Message.Types.MessageType.ExecutionStarting, new ExecutionStartingProcessor(hookRegistry, methodExecutor, _sandbox)},
                {Message.Types.MessageType.ExecutionEnding, new ExecutionEndingProcessor(hookRegistry, methodExecutor, _sandbox)},
                {Message.Types.MessageType.SpecExecutionStarting, new SpecExecutionStartingProcessor(hookRegistry, _sandbox)},
                {Message.Types.MessageType.SpecExecutionEnding, new SpecExecutionEndingProcessor(hookRegistry, _sandbox)},
                {Message.Types.MessageType.ScenarioExecutionStarting, new ScenarioExecutionStartingProcessor(hookRegistry, _sandbox)},
                {Message.Types.MessageType.ScenarioExecutionEnding, new ScenarioExecutionEndingProcessor(hookRegistry, _sandbox)},
                {Message.Types.MessageType.StepExecutionStarting, new StepExecutionStartingProcessor(hookRegistry, _sandbox)},
                {Message.Types.MessageType.StepExecutionEnding, new StepExecutionEndingProcessor(hookRegistry, _sandbox)},
                {Message.Types.MessageType.ExecuteStep, new ExecuteStepProcessor(stepRegistry, _sandbox)},
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