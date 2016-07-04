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
using System.Linq;
using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Processors;
using Gauge.CSharp.Runner.Strategy;
using Gauge.Messages;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    internal class StepExecutionEndingProcessorTests
    {
        private Message _request;
        private Mock<IMethodExecutor> _mockMethodExecutor;
        private ProtoExecutionResult.Builder _protoExecutionResultBuilder;
        private readonly IEnumerable<string> _pendingMessages = new List<string> { "Foo", "Bar" };
        private StepExecutionEndingProcessor _stepExecutionEndingProcessor;

        public void Foo()
        {
        }

        [SetUp]
        public void Setup()
        {
            var mockHookRegistry = new Mock<IHookRegistry>();
            var mockSandbox = new Mock<ISandbox>();
            mockSandbox.Setup(sandbox => sandbox.GetAllPendingMessages()).Returns(_pendingMessages);
            var hooks = new HashSet<HookMethod> { new HookMethod(GetType().GetMethod("Foo"), typeof(Step).Assembly) };
            mockHookRegistry.Setup(x => x.AfterStepHooks).Returns(hooks);
            var stepExecutionEndingRequest = StepExecutionEndingRequest.DefaultInstance;
            _request = Message.CreateBuilder()
                            .SetMessageId(20)
                            .SetMessageType(Message.Types.MessageType.StepExecutionEnding)
                            .SetStepExecutionEndingRequest(stepExecutionEndingRequest)
                            .Build();

            _mockMethodExecutor = new Mock<IMethodExecutor>();
            _protoExecutionResultBuilder = ProtoExecutionResult.CreateBuilder()
                                        .SetExecutionTime(0)
                                        .SetFailed(false)
                                        .AddRangeMessage(_pendingMessages);
            _mockMethodExecutor.Setup(x => x.ExecuteHooks("AfterStep", It.IsAny<TaggedHooksFirstStrategy>(), new List<string>(), stepExecutionEndingRequest.CurrentExecutionInfo))
                .Returns(_protoExecutionResultBuilder);
            _stepExecutionEndingProcessor = new StepExecutionEndingProcessor(_mockMethodExecutor.Object);
        }

        [Test]
        public void ShouldExtendFromHooksExecutionProcessor()
        {
            AssertEx.InheritsFrom<TaggedHooksFirstExecutionProcessor, StepExecutionEndingProcessor>();
        }

        [Test]
        public void ShouldReadPendingMessages()
        {
            var response = _stepExecutionEndingProcessor.Process(_request);

            Assert.True(response.HasExecutionStatusResponse);
            Assert.True(response.ExecutionStatusResponse.HasExecutionResult);
            Assert.AreEqual(2, response.ExecutionStatusResponse.ExecutionResult.MessageCount);
            foreach (var pendingMessage in _pendingMessages)
            {
                Assert.Contains(pendingMessage, response.ExecutionStatusResponse.ExecutionResult.MessageList.ToList());
            }
        }

        [Test]
        public void ShouldGetTagListFromScenarioAndSpec()
        {
            var specInfo = SpecInfo.CreateBuilder()
                .AddTags("foo")
                .SetName("")
                .SetFileName("")
                .SetIsFailed(false)
                .Build();
            var scenarioInfo = ScenarioInfo.CreateBuilder()
                .AddTags("bar")
                .SetName("")
                .SetIsFailed(false)
                .Build();
            var currentScenario = ExecutionInfo.CreateBuilder()
                .SetCurrentScenario(scenarioInfo)
                .SetCurrentSpec(specInfo)
                .Build();
            var currentExecutionInfo = StepExecutionEndingRequest.CreateBuilder()
                .SetCurrentExecutionInfo(currentScenario)
                .Build();
            var message = Message.CreateBuilder()
                .SetStepExecutionEndingRequest(currentExecutionInfo)
                .SetMessageType(Message.Types.MessageType.StepExecutionEnding)
                .SetMessageId(0)
                .Build();
            var tags = AssertEx.ExecuteProtectedMethod<StepExecutionEndingProcessor>("GetApplicableTags", message).ToList();
            Assert.IsNotEmpty(tags);
            Assert.AreEqual(2, tags.Count);
            Assert.Contains("foo", tags);
            Assert.Contains("bar", tags);
        }

        [Test]
        public void ShouldGetTagListFromScenarioAndSpecAndIgnoreDuplicates()
        {
            var specInfo = SpecInfo.CreateBuilder()
                .AddTags("foo")
                .SetName("")
                .SetFileName("")
                .SetIsFailed(false)
                .Build();
            var scenarioInfo = ScenarioInfo.CreateBuilder()
                .AddTags("foo")
                .SetName("")
                .SetIsFailed(false)
                .Build();
            var currentScenario = ExecutionInfo.CreateBuilder()
                .SetCurrentScenario(scenarioInfo)
                .SetCurrentSpec(specInfo)
                .Build();
            var currentExecutionInfo = StepExecutionEndingRequest.CreateBuilder()
                .SetCurrentExecutionInfo(currentScenario)
                .Build();
            var message = Message.CreateBuilder()
                .SetStepExecutionEndingRequest(currentExecutionInfo)
                .SetMessageType(Message.Types.MessageType.StepExecutionEnding)
                .SetMessageId(0)
                .Build();
            var tags = AssertEx.ExecuteProtectedMethod<StepExecutionEndingProcessor>("GetApplicableTags", message).ToList();
            Assert.IsNotEmpty(tags);
            Assert.AreEqual(1, tags.Count);
            Assert.Contains("foo", tags);
        }
    }
}