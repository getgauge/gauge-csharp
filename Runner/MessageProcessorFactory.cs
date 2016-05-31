// Copyright 2015 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Gauge.CSharp.Core;
using Gauge.CSharp.Runner.Processors;
using Gauge.Messages;
using System;

namespace Gauge.CSharp.Runner
{
    public class MessageProcessorFactory
    {
        private readonly IMethodScanner _stepScanner;
        private readonly ISandbox _sandbox;
        private Dictionary<Message.Types.MessageType, IMessageProcessor> _messageProcessorsDictionary;

		public MessageProcessorFactory() : this(SandboxFactory.Create(AppDomain.CurrentDomain.SetupInformation))
        {
        }

        public MessageProcessorFactory(ISandbox sandbox)
        {
            _sandbox = sandbox;
            using (var apiConnection = new GaugeApiConnection(new TcpClientWrapper(Utils.GaugeApiPort)))
            {
                _stepScanner = new MethodScanner(apiConnection, _sandbox);
                InitializeProcessors(_stepScanner);
            }
        }

        public MessageProcessorFactory(IMethodScanner stepScanner, ISandbox sandbox)
        {
            _stepScanner = stepScanner;
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
                {Message.Types.MessageType.ExecutionStarting, new ExecutionStartingProcessor(hookRegistry, methodExecutor)},
                {Message.Types.MessageType.ExecutionEnding, new ExecutionEndingProcessor(hookRegistry, methodExecutor)},
                {Message.Types.MessageType.SpecExecutionStarting, new SpecExecutionStartingProcessor(hookRegistry, methodExecutor)},
                {Message.Types.MessageType.SpecExecutionEnding, new SpecExecutionEndingProcessor(hookRegistry, methodExecutor)},
                {Message.Types.MessageType.ScenarioExecutionStarting, new ScenarioExecutionStartingProcessor(hookRegistry, methodExecutor)},
                {Message.Types.MessageType.ScenarioExecutionEnding, new ScenarioExecutionEndingProcessor(hookRegistry, methodExecutor)},
                {Message.Types.MessageType.StepExecutionStarting, new StepExecutionStartingProcessor(hookRegistry, methodExecutor)},
                {Message.Types.MessageType.StepExecutionEnding, new StepExecutionEndingProcessor(hookRegistry, methodExecutor)},
                {Message.Types.MessageType.ExecuteStep, new ExecuteStepProcessor(stepRegistry, methodExecutor)},
                {Message.Types.MessageType.KillProcessRequest, new KillProcessProcessor()},
                {Message.Types.MessageType.StepNamesRequest, new StepNamesProcessor(_stepScanner)},
                {Message.Types.MessageType.StepValidateRequest, new StepValidationProcessor(stepRegistry)},
                {Message.Types.MessageType.ScenarioDataStoreInit, new ScenarioDataStoreInitProcessor(_sandbox)},
                {Message.Types.MessageType.SpecDataStoreInit, new SpecDataStoreInitProcessor(_sandbox)},
                {Message.Types.MessageType.SuiteDataStoreInit, new SuiteDataStoreInitProcessor(_sandbox)},
                {Message.Types.MessageType.StepNameRequest, new StepNameProcessor(stepRegistry) },
                {Message.Types.MessageType.RefactorRequest, new RefactorProcessor(stepRegistry)},
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