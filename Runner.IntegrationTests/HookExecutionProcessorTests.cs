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

using Gauge.CSharp.Runner.Processors;
using Gauge.Messages;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.IntegrationTests
{
    public class HookExecutionProcessorTests : IntegrationTestsBase
    {
        [Test]
        public void ShouldPasssExecutionContextToHook()
        {
            var sandbox = SandboxBuilder.Build();
            var stepExecutionStartingProcessor = new StepExecutionStartingProcessor(new MethodExecutor(sandbox));

            var message = new Message
            {
                MessageId = 1234,
                MessageType = Message.Types.MessageType.StepExecutionStarting,
                StepExecutionStartingRequest = new StepExecutionStartingRequest
                {
                    CurrentExecutionInfo = new ExecutionInfo
                    {
                        CurrentSpec = new SpecInfo { Name = "Foo Spec" },
                        CurrentScenario = new ScenarioInfo { Name = "Foo Scenario" },
                        CurrentStep = new StepInfo
                        {
                            Step = new ExecuteStepRequest
                            {
                                ParsedStepText = "Foo Step",
                                ActualStepText = "Foo Step"
                            }
                        }
                    }
                }
            };
            var result = stepExecutionStartingProcessor.Process(message);

            AssertRunnerDomainDidNotLoadUsersAssembly();
            var protoExecutionResult = result.ExecutionStatusResponse.ExecutionResult;
            Assert.IsNotNull(protoExecutionResult);
            Assert.IsFalse(protoExecutionResult.Failed);
        }
    }
}