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
using Gauge.CSharp.Runner.UnitTests.Processors.Stubs;
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
        private readonly IEnumerable<string> _pendingMessages = new List<string> { "foo", "bar" };
        private StepExecutionEndingProcessor _stepExecutionEndingProcessor;

        public void Foo()
        {
        }

        [SetUp]
        public void Setup()
        {
            var mockHookRegistry = new Mock<IHookRegistry>();
            var mockSandbox = new Mock<ISandbox>();
            mockSandbox.Setup(sandbox => sandbox.TargetLibAssembly).Returns(typeof(Step).Assembly);
            mockSandbox.Setup(sandbox => sandbox.GetAllPendingMessages()).Returns(_pendingMessages);
            var hooks = new HashSet<HookMethod> { new HookMethod(GetType().GetMethod("Foo"), mockSandbox.Object) };
            var hooksToExecute = hooks.Select(method => method.Method);
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
            _mockMethodExecutor.Setup(x => x.ExecuteHooks(hooksToExecute, stepExecutionEndingRequest.CurrentExecutionInfo))
                .Returns(_protoExecutionResultBuilder);
            _stepExecutionEndingProcessor = new StepExecutionEndingProcessor(mockHookRegistry.Object, _mockMethodExecutor.Object);
        }

        [Test]
        public void ShouldExtendFromHooksExecutionProcessor()
        {
            AssertEx.InheritsFrom<HookExecutionProcessor, StepExecutionEndingProcessor>();
            AssertEx.DoesNotInheritsFrom<TaggedHooksFirstExecutionProcessor, StepExecutionEndingProcessor>();
            AssertEx.DoesNotInheritsFrom<UntaggedHooksFirstExecutionProcessor, StepExecutionEndingProcessor>();
        }

        [Test]
        public void ReaddAllMessagesFlagIsEnabled()
        {
            Assert.True(new TestStepExecutionEndingProcessor().ShouldReadMessages());
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
    }
}