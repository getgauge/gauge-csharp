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
using System.Linq;
using Gauge.CSharp.Runner.Processors;
using Gauge.Messages;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    [TestFixture]
    public class ExecutionEStartingProcessorTests
    {
        private ExecutionStartingProcessor _executionStartingProcessor;
        private Message _request;
        private Mock<IMethodExecutor> _mockMethodExecutor;
        private ProtoExecutionResult _protoExecutionResult;

        public void Foo()
        {
        }

        [SetUp]
        public void Setup()
        {
            var mockHookRegistry = new Mock<IHookRegistry>();
            var hooks = new HashSet<HookMethod> {new HookMethod(GetType().GetMethod("Foo"))};
            var hooksToExecute = hooks.Select(method => method.Method);
            mockHookRegistry.Setup(x => x.BeforeSuiteHooks).Returns(hooks);
            var executionEndingRequest = ExecutionEndingRequest.DefaultInstance;
            _request = Message.CreateBuilder()
                            .SetMessageId(20)
                            .SetMessageType(Message.Types.MessageType.ExecutionEnding)
                            .SetExecutionEndingRequest(executionEndingRequest)
                            .Build();

            _mockMethodExecutor = new Mock<IMethodExecutor>();
            _protoExecutionResult = ProtoExecutionResult.CreateBuilder().SetExecutionTime(0).SetFailed(false).Build();
            _mockMethodExecutor.Setup(x => x.ExecuteHooks(hooksToExecute, executionEndingRequest.CurrentExecutionInfo))
                .Returns(_protoExecutionResult);
            _executionStartingProcessor = new ExecutionStartingProcessor(mockHookRegistry.Object, _mockMethodExecutor.Object);
        }
        [Test]
        public void ShouldProcessHooks()
        {
            var message = _executionStartingProcessor.Process(_request);

            _mockMethodExecutor.VerifyAll();
        }
    }
}