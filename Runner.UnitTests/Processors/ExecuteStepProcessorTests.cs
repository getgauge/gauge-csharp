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

using System;
using System.Collections.Generic;
using Gauge.CSharp.Runner.Processors;
using Gauge.Messages;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    [TestFixture]
    public class ExecuteStepProcessorTests
    {
        public void Foo(string param)
        {
        }

        [Test]
        public void ShouldReportMissingStep()
        {
            const string parsedStepText = "Foo";
            var request = Message.CreateBuilder()
                .SetMessageType(Message.Types.MessageType.ExecuteStep)
                .SetExecuteStepRequest(
                    ExecuteStepRequest.CreateBuilder()
                        .SetActualStepText(parsedStepText)
                        .SetParsedStepText(parsedStepText)
                        .Build())
                .SetMessageId(20)
                .Build();
            var mockStepRegistry = new Mock<IStepRegistry>();
            mockStepRegistry.Setup(x => x.ContainsStep(parsedStepText)).Returns(false);
            var mockMethodExecutor = new Mock<IMethodExecutor>();
            var mockSandbox = new Mock<ISandbox>();

            var response = new ExecuteStepProcessor(mockStepRegistry.Object, mockMethodExecutor.Object, mockSandbox.Object).Process(request);

            Assert.True(response.ExecutionStatusResponse.ExecutionResult.Failed);
            Assert.AreEqual(response.ExecutionStatusResponse.ExecutionResult.ErrorMessage,
                "Step Implementation not found");
        }

        [Test]
        public void ShouldReportArgumentMismatch()
        {
            const string parsedStepText = "Foo";
            var request = Message.CreateBuilder()
                .SetMessageType(Message.Types.MessageType.ExecuteStep)
                .SetExecuteStepRequest(
                    ExecuteStepRequest.CreateBuilder()
                        .SetActualStepText(parsedStepText)
                        .SetParsedStepText(parsedStepText)
                        .Build())
                .SetMessageId(20)
                .Build();
            var mockStepRegistry = new Mock<IStepRegistry>();
            mockStepRegistry.Setup(x => x.ContainsStep(parsedStepText)).Returns(true);
            var fooMethod = new GaugeMethod {Name = "Foo", ParameterCount = 1};
            mockStepRegistry.Setup(x => x.MethodFor(parsedStepText)).Returns(fooMethod);
            var mockMethodExecutor = new Mock<IMethodExecutor>();
            var mockSandbox = new Mock<ISandbox>();

            var response = new ExecuteStepProcessor(mockStepRegistry.Object, mockMethodExecutor.Object, mockSandbox.Object).Process(request);
            
            Assert.True(response.ExecutionStatusResponse.ExecutionResult.Failed);
            Assert.AreEqual(response.ExecutionStatusResponse.ExecutionResult.ErrorMessage,
                "Argument length mismatch for Foo. Actual Count: 0, Expected Count: 1");
        }

        [Test]
        public void ShouldProcessExecuteStepRequest()
        {
            const string parsedStepText = "Foo";
            var request = Message.CreateBuilder()
                .SetMessageType(Message.Types.MessageType.ExecuteStep)
                .SetExecuteStepRequest(
                    ExecuteStepRequest.CreateBuilder()
                        .SetActualStepText(parsedStepText)
                        .SetParsedStepText(parsedStepText)
                        .AddParameters(
                            Parameter.CreateBuilder()
                                .SetParameterType(Parameter.Types.ParameterType.Static)
                                .SetName("Foo")
                                .SetValue("Bar")
                                .Build())
                        .Build())
                .SetMessageId(20)
                .Build();
            var mockStepRegistry = new Mock<IStepRegistry>();
            mockStepRegistry.Setup(x => x.ContainsStep(parsedStepText)).Returns(true);
            var fooMethodInfo = new GaugeMethod {Name = "Foo", ParameterCount = 1};
            mockStepRegistry.Setup(x => x.MethodFor(parsedStepText)).Returns(fooMethodInfo);
            var mockMethodExecutor = new Mock<IMethodExecutor>();
            mockMethodExecutor.Setup(e => e.Execute(fooMethodInfo, It.IsAny<KeyValuePair<string, string>[]>())).Returns(() => ProtoExecutionResult.CreateBuilder().SetExecutionTime(1).SetFailed(false).Build());
            var mockSandbox = new Mock<ISandbox>();

            var response = new ExecuteStepProcessor(mockStepRegistry.Object, mockMethodExecutor.Object, mockSandbox.Object).Process(request);

            Assert.False(response.ExecutionStatusResponse.ExecutionResult.Failed);
        }
    }
}