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
    [TestFixture]
    public class ExecutionStartingProcessorTests
    {
        private ExecutionStartingProcessor _executionStartingProcessor;
        private Message _request;
        private Mock<IMethodExecutor> _mockMethodExecutor;
        private ProtoExecutionResult.Builder _protoExecutionResultBuilder;

        public void Foo()
        {
        }

        [SetUp]
        public void Setup()
        {
            var mockHookRegistry = new Mock<IHookRegistry>();

            var hooks = new HashSet<HookMethod> { new HookMethod(GetType().GetMethod("Foo"), typeof(Step).Assembly) };
            mockHookRegistry.Setup(x => x.BeforeSuiteHooks).Returns(hooks);
            var executionEndingRequest = ExecutionStartingRequest.DefaultInstance;
            _request = Message.CreateBuilder()
                            .SetMessageId(20)
                            .SetMessageType(Message.Types.MessageType.ExecutionEnding)
                            .SetExecutionStartingRequest(executionEndingRequest)
                            .Build();

            _mockMethodExecutor = new Mock<IMethodExecutor>();
            _protoExecutionResultBuilder = ProtoExecutionResult.CreateBuilder().SetExecutionTime(0).SetFailed(false);
            _mockMethodExecutor.Setup(x => x.ExecuteHooks("BeforeSuite", It.IsAny<HooksStrategy>(), new List<string>(), executionEndingRequest.CurrentExecutionInfo))
                .Returns(_protoExecutionResultBuilder);
            _executionStartingProcessor = new ExecutionStartingProcessor(_mockMethodExecutor.Object);
        }
        [Test]
        public void ShouldProcessHooks()
        {
            _executionStartingProcessor.Process(_request);
            _mockMethodExecutor.VerifyAll();
        }

        [Test]
        public void ShouldExtendFromHooksExecutionProcessor()
        {
            AssertEx.InheritsFrom<HookExecutionProcessor, ExecutionStartingProcessor>();
            AssertEx.DoesNotInheritsFrom<TaggedHooksFirstExecutionProcessor, ExecutionStartingProcessor>();
            AssertEx.DoesNotInheritsFrom<UntaggedHooksFirstExecutionProcessor, ExecutionStartingProcessor>();
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

            var tags = AssertEx.ExecuteProtectedMethod<ExecutionStartingProcessor>("GetApplicableTags", message);
            Assert.IsEmpty(tags);
        }
    }
}