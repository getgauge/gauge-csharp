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
    internal class SpecExecutionEndingProcessorTests
    {
        [Test]
        public void ShouldExtendFromTaggedHooksFirstExecutionProcessor()
        {
            AssertEx.InheritsFrom<TaggedHooksFirstExecutionProcessor, SpecExecutionEndingProcessor>();
        }

        [Test]
        public void ShouldGetTagListFromExecutionInfo()
        {
            var specInfo = SpecInfo.CreateBuilder()
                .AddTags("foo")
                .SetName("")
                .SetFileName("")
                .SetIsFailed(false)
                .Build();
            var executionInfo = ExecutionInfo.CreateBuilder()
                .SetCurrentSpec(specInfo)
                .Build();
            var currentExecutionInfo = SpecExecutionEndingRequest.CreateBuilder()
                .SetCurrentExecutionInfo(executionInfo)
                .Build();
            var message = Message.CreateBuilder()
                .SetSpecExecutionEndingRequest(currentExecutionInfo)
                .SetMessageType(Message.Types.MessageType.SpecExecutionEnding)
                .SetMessageId(0)
                .Build();

            var tags = AssertEx.ExecuteProtectedMethod<SpecExecutionEndingProcessor>("GetApplicableTags", message).ToList();

            Assert.IsNotEmpty(tags);
            Assert.AreEqual(1, tags.Count);
            Assert.Contains("foo", tags);
        }
    }
}