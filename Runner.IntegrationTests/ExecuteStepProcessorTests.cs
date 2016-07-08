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
using System.Text;
using Gauge.CSharp.Runner.Models;
using Gauge.CSharp.Runner.Processors;
using Gauge.Messages;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.IntegrationTests
{
    public class ExecuteStepProcessorTests : IntegrationTestsBase
    {
        [Test]
        public void ShouldExecuteMethodFromRequest()
        {
            const string parameterizedStepText = "Step that takes a table {}";
            const string stepText = "Step that takes a table <table>";
            var sandbox = SandboxFactory.Create();
            var gaugeMethod = sandbox.GetStepMethods()
                .First(method => method.Name == "IntegrationTestSample.StepImplementation.ReadTable");
            var scannedSteps = new List<KeyValuePair<string, GaugeMethod>> { new KeyValuePair<string, GaugeMethod>(parameterizedStepText, gaugeMethod) };
            var aliases = new Dictionary<string, bool> { { parameterizedStepText, false } };
            var stepTextMap = new Dictionary<string, string> { { parameterizedStepText, stepText } };
            var stepRegistry = new StepRegistry(scannedSteps, stepTextMap, aliases);

            var executeStepProcessor = new ExecuteStepProcessor(stepRegistry, new MethodExecutor(sandbox));

            var builder = Message.CreateBuilder();
            var protoTable = ProtoTable.CreateBuilder()
                .SetHeaders(
                    ProtoTableRow.CreateBuilder().AddRangeCells(new List<string> { "foo", "bar" }))
                .AddRangeRows(new List<ProtoTableRow>
                {
                    ProtoTableRow.CreateBuilder()
                        .AddRangeCells(new List<string> {"foorow1", "foorow2"})
                        .Build()
                }).Build();
            var message = builder
                .SetMessageId(1234)
                .SetMessageType(Message.Types.MessageType.ExecuteStep)
                .SetExecuteStepRequest(
                    ExecuteStepRequest.CreateBuilder()
                        .SetParsedStepText(parameterizedStepText)
                        .SetActualStepText(stepText)
                        .AddParameters(
                            Parameter.CreateBuilder()
                                .SetName("table")
                                .SetParameterType(Parameter.Types.ParameterType.Table)
                                .SetTable(protoTable).Build()
                        ).Build()
                ).Build();

            var result = executeStepProcessor.Process(message);

            AssertRunnerDomainDidNotLoadUsersAssembly();
            var protoExecutionResult = result.ExecutionStatusResponse.ExecutionResult;
            Assert.IsNotNull(protoExecutionResult);
            Assert.IsFalse(protoExecutionResult.Failed);
        }

        [Test]
        public void ShouldCaptureScreenshotOnFailure()
        {
            const string parameterizedStepText = "I throw a serializable exception";
            const string stepText = "I throw a serializable exception";
            var sandbox = SandboxFactory.Create();
            var gaugeMethod = sandbox.GetStepMethods()
                .First(method => method.Name == "IntegrationTestSample.StepImplementation.ThrowSerializableException");
            var scannedSteps = new List<KeyValuePair<string, GaugeMethod>> { new KeyValuePair<string, GaugeMethod>(parameterizedStepText, gaugeMethod) };
            var aliases = new Dictionary<string, bool> { { parameterizedStepText, false } };
            var stepTextMap = new Dictionary<string, string> { { parameterizedStepText, stepText } };
            var stepRegistry = new StepRegistry(scannedSteps, stepTextMap, aliases);

            var executeStepProcessor = new ExecuteStepProcessor(stepRegistry, new MethodExecutor(sandbox));

            var message = Message.CreateBuilder()
                .SetMessageId(1234)
                .SetMessageType(Message.Types.MessageType.ExecuteStep)
                .SetExecuteStepRequest(
                    ExecuteStepRequest.CreateBuilder()
                        .SetParsedStepText(parameterizedStepText)
                        .SetActualStepText(stepText).Build()
                ).Build();

            var result = executeStepProcessor.Process(message);
            var protoExecutionResult = result.ExecutionStatusResponse.ExecutionResult;

            Assert.IsNotNull(protoExecutionResult);
            Assert.IsTrue(protoExecutionResult.Failed);
            Assert.AreEqual(Encoding.UTF8.GetString(protoExecutionResult.ScreenShot.ToByteArray()), "ScreenShot");
        }
    }
}
