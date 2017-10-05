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

using System.Linq;
using Gauge.CSharp.Runner.Processors;
using Gauge.Messages;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    internal class ScenarioExecutionEndingProcessorTests
    {
        [Test]
        public void ShouldExtendFromTaggedHooksFirstExecutionProcessor()
        {
            AssertEx.InheritsFrom<TaggedHooksFirstExecutionProcessor, ScenarioExecutionEndingProcessor>();
        }

        [Test]
        public void ShouldGetTagListFromSpecAndScenario()
        {
            var specInfo = new SpecInfo
            {
                Tags = {"foo"},
                Name = "",
                FileName = "",
                IsFailed = false
            };
            var scenarioInfo = new ScenarioInfo
            {
                Tags = {"bar"},
                Name = "",
                IsFailed = false
            };
            var currentScenario = new ExecutionInfo
            {
                CurrentSpec = specInfo,
                CurrentScenario = scenarioInfo
            };
            var currentExecutionInfo = new ScenarioExecutionEndingRequest
            {
                CurrentExecutionInfo = currentScenario
            };
            var message = new Message
            {
                ScenarioExecutionEndingRequest = currentExecutionInfo,
                MessageType = Message.Types.MessageType.ScenarioExecutionEnding,
                MessageId = 0
            };

            var tags = AssertEx.ExecuteProtectedMethod<ScenarioExecutionEndingProcessor>("GetApplicableTags", message)
                .ToList();

            Assert.IsNotEmpty(tags);
            Assert.AreEqual(2, tags.Count);
            Assert.Contains("foo", tags);
            Assert.Contains("bar", tags);
        }

        [Test]
        public void ShouldNotGetDuplicateTags()
        {
            var specInfo = new SpecInfo
            {
                Tags = {"foo"},
                Name = "",
                FileName = "",
                IsFailed = false
            };
            var scenarioInfo = new ScenarioInfo
            {
                Tags = {"foo"},
                Name = "",
                IsFailed = false
            };
            var currentScenario = new ExecutionInfo
            {
                CurrentSpec = specInfo,
                CurrentScenario = scenarioInfo
            };
            var currentExecutionInfo = new ScenarioExecutionEndingRequest
            {
                CurrentExecutionInfo = currentScenario
            };
            var message = new Message
            {
                ScenarioExecutionEndingRequest = currentExecutionInfo,
                MessageType = Message.Types.MessageType.ScenarioExecutionEnding,
                MessageId = 0
            };

            var tags = AssertEx.ExecuteProtectedMethod<ScenarioExecutionEndingProcessor>("GetApplicableTags", message)
                .ToList();

            Assert.IsNotEmpty(tags);
            Assert.AreEqual(1, tags.Count);
            Assert.Contains("foo", tags);
        }
    }
}