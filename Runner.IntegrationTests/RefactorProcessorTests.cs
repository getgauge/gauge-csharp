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
            const string parameterizedStepText = "Refactoring Say <what> to <who>";
            const string stepValue = "Refactoring Say {} to {}";
            var sandbox = SandboxBuilder.Build();
            var gaugeMethod = sandbox.GetStepMethods()
                .First(method => method.Name == "IntegrationTestSample.RefactoringSample.RefactoringSaySomething-StringwhatStringwho");
            var scannedSteps = new List<KeyValuePair<string, GaugeMethod>> { new KeyValuePair<string, GaugeMethod>(stepValue, gaugeMethod) };
            var aliases = new Dictionary<string, bool> { { stepValue, false } };
            var stepTextMap = new Dictionary<string, string> { { stepValue, parameterizedStepText } };
            var stepRegistry = new StepRegistry(scannedSteps, stepTextMap, aliases);
            var message = new Message()
            {
               MessageId = 1234,
               MessageType = Message.Types.MessageType.RefactorRequest,
               RefactorRequest = new RefactorRequest()
               {
                   OldStepValue = new ProtoStepValue()
                   {
                       StepValue = stepValue,
                       ParameterizedStepValue = parameterizedStepText,
                       Parameters = { "what", "who"}
                   },
                   NewStepValue = new ProtoStepValue()
                   {
                       StepValue = "Refactoring Say {} to {} at {}",
                       ParameterizedStepValue = "Refactoring Say <what> to <who> at <when>",
                       Parameters = { "who", "what", "when"}
                   },
                   ParamPositions =
                   {
                       new ParameterPosition() { OldPosition = 0, NewPosition = 0},
                       new ParameterPosition() { OldPosition = 1, NewPosition = 1},
                       new ParameterPosition() { OldPosition = -1, NewPosition = 2}
                       
                   }
               }
            };
            
            var refactorProcessor = new RefactorProcessor(stepRegistry, sandbox);
            var result = refactorProcessor.Process(message);
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