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
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Runner.Models;
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

            var response = new ExecuteStepProcessor(mockStepRegistry.Object, mockMethodExecutor.Object).Process(request);

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

            var response = new ExecuteStepProcessor(mockStepRegistry.Object, mockMethodExecutor.Object).Process(request);
            
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
            mockMethodExecutor.Setup(e => e.Execute(fooMethodInfo, It.IsAny<string[]>())).Returns(() => ProtoExecutionResult.CreateBuilder().SetExecutionTime(1).SetFailed(false).Build());

            var response = new ExecuteStepProcessor(mockStepRegistry.Object, mockMethodExecutor.Object).Process(request);

            Assert.False(response.ExecutionStatusResponse.ExecutionResult.Failed);
        }

        [Test]
        [TestCase(Parameter.Types.ParameterType.Table)]
        [TestCase(Parameter.Types.ParameterType.Special_Table)]
        public void ShouldProcessExecuteStepRequestForTableParam(Parameter.Types.ParameterType parameterType)
        {
            const string parsedStepText = "Foo";
            var protoTable = ProtoTable.CreateBuilder()
                .SetHeaders(ProtoTableRow.CreateBuilder().AddRangeCells(new List<string> { "foo", "bar" }))
                .AddRangeRows(new List<ProtoTableRow>
                {
                    ProtoTableRow.CreateBuilder()
                        .AddRangeCells(new List<string> {"foorow1", "foorow2"})
                        .Build()
                }).Build();

            var request = Message.CreateBuilder()
                .SetMessageType(Message.Types.MessageType.ExecuteStep)
                .SetExecuteStepRequest(
                    ExecuteStepRequest.CreateBuilder()
                        .SetActualStepText(parsedStepText)
                        .SetParsedStepText(parsedStepText)
                        .AddParameters(
                            Parameter.CreateBuilder()
                                .SetParameterType(parameterType)
                                .SetTable(protoTable)
                                .Build())
                        .Build())
                .SetMessageId(20)
                .Build();
            var mockStepRegistry = new Mock<IStepRegistry>();
            mockStepRegistry.Setup(x => x.ContainsStep(parsedStepText)).Returns(true);
            var fooMethodInfo = new GaugeMethod { Name = "Foo", ParameterCount = 1 };
            mockStepRegistry.Setup(x => x.MethodFor(parsedStepText)).Returns(fooMethodInfo);
            var mockMethodExecutor = new Mock<IMethodExecutor>();
            mockMethodExecutor.Setup(e => e.Execute(fooMethodInfo, It.IsAny<string[]>())).Returns(() => ProtoExecutionResult.CreateBuilder().SetExecutionTime(1).SetFailed(false).Build());

            var response = new ExecuteStepProcessor(mockStepRegistry.Object, mockMethodExecutor.Object).Process(request);

            mockMethodExecutor.Verify(executor => executor.Execute(fooMethodInfo, It.Is<string[]>(strings => HasTable(strings))));
            Assert.False(response.ExecutionStatusResponse.ExecutionResult.Failed);
        }

        private static bool HasTable(IReadOnlyList<string> parameters)
        {
            var serializer = new DataContractJsonSerializer(typeof(Table));

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(parameters[0])))
            {
                try
                {
                    serializer.ReadObject(stream);
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
    }
}