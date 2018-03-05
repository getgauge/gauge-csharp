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
using Gauge.CSharp.Lib;
using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Models;
using Gauge.CSharp.Runner.Processors;
using Gauge.CSharp.Runner.Strategy;
using Gauge.Messages;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    [TestFixture]
    public class ExecutionEndingProcessorTests
    {
        [SetUp]
        public void Setup()
        {
            var mockHookRegistry = new Mock<IHookRegistry>();
            var mockSandbox = new Mock<ISandbox>();
            mockSandbox.Setup(sandbox => sandbox.GetAllPendingMessages()).Returns(_pendingMessages);
            var hooks = new HashSet<IHookMethod>
            {
                new HookMethod("BeforeSpec", GetType().GetMethod("Foo"), typeof(Step).Assembly)
            };
            mockHookRegistry.Setup(x => x.AfterSuiteHooks).Returns(hooks);
            var executionEndingRequest = new ExecutionEndingRequest
            {
                CurrentExecutionInfo = new ExecutionInfo
                {
                    CurrentSpec = new SpecInfo(),
                    CurrentScenario = new ScenarioInfo()
                }
            };
            _request = new Message
            {
                MessageId = 20,
                MessageType = Message.Types.MessageType.ExecutionEnding,
                ExecutionEndingRequest = executionEndingRequest
            };

            _mockMethodExecutor = new Mock<IMethodExecutor>();
            _protoExecutionResult = new ProtoExecutionResult
            {
                ExecutionTime = 0,
                Failed = false
            };
            _protoExecutionResult.Message.AddRange(_pendingMessages);
            _mockMethodExecutor.Setup(x =>
                    x.ExecuteHooks("AfterSuite", It.IsAny<HooksStrategy>(), It.IsAny<IList<string>>(), It.IsAny<ExecutionContext>()))
                .Returns(_protoExecutionResult);
            _executionEndingProcessor = new ExecutionEndingProcessor(_mockMethodExecutor.Object);
        }

        private ExecutionEndingProcessor _executionEndingProcessor;
        private Message _request;
        private Mock<IMethodExecutor> _mockMethodExecutor;
        private ProtoExecutionResult _protoExecutionResult;
        private readonly IEnumerable<string> _pendingMessages = new List<string> {"Foo", "Bar"};

        public void Foo()
        {
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
            var tags = AssertEx.ExecuteProtectedMethod<ExecutionEndingProcessor>("GetApplicableTags", _request);
            Assert.IsEmpty(tags);
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
            Assert.AreEqual(_protoExecutionResult, message.ExecutionStatusResponse.ExecutionResult);
        }
    }
}