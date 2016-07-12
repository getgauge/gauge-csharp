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
using System.Linq;
using Gauge.CSharp.Runner.Models;
using Gauge.CSharp.Runner.Processors;
using Gauge.Messages;
using Google.ProtocolBuffers;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.IntegrationTests
{
    public class RefactorProcessorTests
    {
        private readonly string _testProjectPath = TestUtils.GetIntegrationTestSampleDirectory();

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", _testProjectPath);

            File.Copy(Path.Combine(_testProjectPath, "RefactoringSample.cs"), Path.Combine(_testProjectPath, "RefactoringSample_copy.cs"), true);
        }

        [Test]
        public void ShouldAddParameters()
        {
            const string parameterizedStepText = "Refactoring Say {} to {}";
            const string stepText = "Refactoring Say <what> to <who>";
            var sandbox = SandboxBuilder.Build();
            var gaugeMethod = sandbox.GetStepMethods()
                .First(method => method.Name == "IntegrationTestSample.RefactoringSample.RefactoringSaySomething");
            var scannedSteps = new List<KeyValuePair<string, GaugeMethod>> { new KeyValuePair<string, GaugeMethod>(parameterizedStepText, gaugeMethod) };
            var aliases = new Dictionary<string, bool> { { parameterizedStepText, false } };
            var stepTextMap = new Dictionary<string, string> { { parameterizedStepText, stepText } };
            var stepRegistry = new StepRegistry(scannedSteps, stepTextMap, aliases);

            var message = Message.CreateBuilder()
                .SetMessageId(1234)
                .SetMessageType(Message.Types.MessageType.RefactorRequest)
                .SetRefactorRequest(
                    RefactorRequest.CreateBuilder()
                        .SetOldStepValue(
                            ProtoStepValue.CreateBuilder()
                                .SetStepValue(stepText)
                                .SetParameterizedStepValue(parameterizedStepText)
                                .AddParameters("what")
                                .AddParameters("who")
                                .Build())
                        .SetNewStepValue(
                            ProtoStepValue.CreateBuilder()
                                .SetStepValue("Refactoring Say {} to {} at {}")
                                .SetParameterizedStepValue("Refactoring Say <what> to <who> at <when>")
                                .AddParameters("what")
                                .AddParameters("who")
                                .AddParameters("when")
                                .Build())
                        .AddParamPositions(ParameterPosition.CreateBuilder().SetOldPosition(0).SetNewPosition(0))
                        .AddParamPositions(ParameterPosition.CreateBuilder().SetOldPosition(1).SetNewPosition(1))
                        .AddParamPositions(ParameterPosition.CreateBuilder().SetOldPosition(-1).SetNewPosition(2))
                ).Build();

            var refactorProcessor = new RefactorProcessor(stepRegistry, sandbox);
            var result = refactorProcessor.Process(message);
            Console.WriteLine(result.RefactorResponse.ToJson());
            Assert.IsTrue(result.RefactorResponse.Success);
        }
        [TearDown]
        public void TearDown()
        {
            var sourceFileName = Path.Combine(_testProjectPath, "RefactoringSample_copy.cs");
            File.Copy(sourceFileName, Path.Combine(_testProjectPath, "RefactoringSample.cs"), true);
            File.Delete(sourceFileName);
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }
    }
}