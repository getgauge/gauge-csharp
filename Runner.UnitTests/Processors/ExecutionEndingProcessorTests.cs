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
using Gauge.Messages;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    [TestFixture]
    public class ExecutionEndingProcessorTests
    {
        private ExecutionEndingProcessor _executionEndingProcessor;
        private Message _request;
        private Mock<IMethodExecutor> _mockMethodExecutor;
        private ProtoExecutionResult.Builder _protoExecutionResultBuilder;
        private readonly IEnumerable<string> _pendingMessages = new List<string> {"Foo" , "Bar"};

        public void Foo()
        {
        }

        [SetUp]
        public void Setup()
        {
            var mockHookRegistry = new Mock<IHookRegistry>();
            var mockSandbox = new Mock<ISandbox>();
            mockSandbox.Setup(sandbox => sandbox.TargetLibAssembly).Returns(typeof (Step).Assembly);
            mockSandbox.Setup(sandbox => sandbox.GetAllPendingMessages()).Returns(_pendingMessages);
            var hooks = new HashSet<HookMethod> {new HookMethod(GetType().GetMethod("Foo"), mockSandbox.Object)};
            var hooksToExecute = hooks.Select(method => method.Method);
            mockHookRegistry.Setup(x => x.AfterSuiteHooks).Returns(hooks);
            var executionEndingRequest = ExecutionEndingRequest.DefaultInstance;
            _request = Message.CreateBuilder()
                            .SetMessageId(20)
                            .SetMessageType(Message.Types.MessageType.ExecutionEnding)
                            .SetExecutionEndingRequest(executionEndingRequest)
                            .Build();

            _mockMethodExecutor = new Mock<IMethodExecutor>();
            _protoExecutionResultBuilder = ProtoExecutionResult.CreateBuilder()
                                        .SetExecutionTime(0)
                                        .SetFailed(false)
                                        .AddRangeMessage(_pendingMessages);
            _mockMethodExecutor.Setup(x => x.ExecuteHooks(hooksToExecute, executionEndingRequest.CurrentExecutionInfo))
                .Returns(_protoExecutionResultBuilder);
            _executionEndingProcessor = new ExecutionEndingProcessor(mockHookRegistry.Object, _mockMethodExecutor.Object);
        }
        [Test]
        public void ShouldProcessHooks()
        {
            _executionEndingProcessor.Process(_request);
            _mockMethodExecutor.VerifyAll();
        }

        [Test]
        public void ShouldWrapInMessage()
        {
            var message = _executionEndingProcessor.Process(_request);
            
            Assert.AreEqual(_request.MessageId, message.MessageId);
            Assert.AreEqual(Message.Types.MessageType.ExecutionStatusResponse, message.MessageType);
            Assert.AreEqual(_protoExecutionResultBuilder.Build(), message.ExecutionStatusResponse.ExecutionResult);
        }

        [Test]
        public void ShouldExtendFromHooksExecutionProcessor()
        {
            AssertEx.InheritsFrom<HookExecutionProcessor, ExecutionEndingProcessor>();
            AssertEx.DoesNotInheritsFrom<TaggedHooksFirstExecutionProcessor, ExecutionEndingProcessor>();
            AssertEx.DoesNotInheritsFrom<UntaggedHooksFirstExecutionProcessor, ExecutionEndingProcessor>();
        }

        [Test]
        public void ShouldGetEmptyTagListByDefault()
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
            var currentExecutionInfo = ScenarioExecutionStartingRequest.CreateBuilder()
                .SetCurrentExecutionInfo(currentScenario)
                .Build();
            var message = Message.CreateBuilder()
                .SetScenarioExecutionStartingRequest(currentExecutionInfo)
                .SetMessageType(Message.Types.MessageType.ScenarioExecutionStarting)
                .SetMessageId(0)
                .Build();
            var tags = AssertEx.ExecuteProtectedMethod<ExecutionEndingProcessor>("GetApplicableTags", message);
            Assert.IsEmpty(tags);
        }
    }
}