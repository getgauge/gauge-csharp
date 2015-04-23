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