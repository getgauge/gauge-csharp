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
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    [TestFixture]
    internal class StartCommandTests
    {
        private Mock<IGaugeListener> _mockGaugeListener;
        private Mock<IGaugeProjectBuilder> _mockGaugeProjectBuilder;
        private StartCommand _startCommand;

        [SetUp]
        public void Setup()
        {
            _mockGaugeListener = new Mock<IGaugeListener>();
            _mockGaugeProjectBuilder = new Mock<IGaugeProjectBuilder>();
            _startCommand = new StartCommand(() => _mockGaugeListener.Object, () => _mockGaugeProjectBuilder.Object);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_CUSTOM_BUILD_PATH", null);
        }

        [Test]
        public void ShouldNotBuildWhenCustomBuildPathIsSet()
        {
            Environment.SetEnvironmentVariable("GAUGE_CUSTOM_BUILD_PATH", "GAUGE_CUSTOM_BUILD_PATH");
            _startCommand.Execute();

            _mockGaugeProjectBuilder.Verify(builder => builder.BuildTargetGaugeProject(), Times.Never);
        }

        [Test]
        public void ShouldInvokeProjectBuild()
        {
            _startCommand.Execute();

            _mockGaugeProjectBuilder.Verify(builder => builder.BuildTargetGaugeProject(), Times.Once);
        }

        [Test]
        public void ShouldNotPollForMessagesWhenBuildFails()
        {
            _mockGaugeProjectBuilder.Setup(builder => builder.BuildTargetGaugeProject()).Returns(false);
            
            _startCommand.Execute();

            _mockGaugeListener.Verify(listener => listener.PollForMessages(), Times.Never);
        }
        
        [Test]
        public void ShouldPollForMessagesWhenBuildPasses()
        {
            _mockGaugeProjectBuilder.Setup(builder => builder.BuildTargetGaugeProject()).Returns(true);

            _startCommand.Execute();

            _mockGaugeListener.Verify(listener => listener.PollForMessages(), Times.Once);
        }

        [Test]
        public void ShouldPollForMessagesWhenCustomBuildPathIsSet()
        {
            Environment.SetEnvironmentVariable("GAUGE_CUSTOM_BUILD_PATH", "GAUGE_CUSTOM_BUILD_PATH");
            _startCommand.Execute();

            _mockGaugeListener.Verify(listener => listener.PollForMessages(), Times.Once);
        }
    }
}
