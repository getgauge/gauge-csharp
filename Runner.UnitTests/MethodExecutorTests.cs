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
using System.Reflection;
using System.Threading;
using Gauge.CSharp.Core;
using Gauge.CSharp.Runner.Models;
using Gauge.CSharp.Runner.Strategy;
using Moq;
using NUnit.Framework;
using ExecutionContext = Gauge.CSharp.Lib.ExecutionContext;

namespace Gauge.CSharp.Runner.UnitTests
{
    [TestFixture]
    public class MethodExecutorTests
    {
        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT",
                Directory.GetDirectoryRoot(Assembly.GetExecutingAssembly().Location));
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }

        [Test]
        public void ShouldExecuteHooks()
        {
            var mockSandBox = new Mock<ISandbox>();
            var hooksStrategy = new HooksStrategy();
            var executionResult = new ExecutionResult {Success = true};
            mockSandBox.Setup(sandbox =>
                sandbox.ExecuteHooks("hooks", hooksStrategy, new List<string>(), It.IsAny<ExecutionContext>())
            ).Returns(executionResult).Verifiable();

            new MethodExecutor(mockSandBox.Object).ExecuteHooks("hooks", hooksStrategy, new List<string>(),
                new ExecutionContext());

            mockSandBox.VerifyAll();
        }

        [Test]
        public void ShouldExecuteHooksAndNotTakeScreenshotOnFailureWhenDisabled()
        {
            var mockSandBox = new Mock<ISandbox>();
            var hooksStrategy = new HooksStrategy();
            var result = new ExecutionResult
            {
                Success = false,
                ExceptionMessage = "Some Error",
                StackTrace = "StackTrace"
            };
            mockSandBox.Setup(sandbox =>
                sandbox.ExecuteHooks("hooks", hooksStrategy, new List<string>(), It.IsAny<ExecutionContext>())
            ).Returns(result).Verifiable();

            var screenshotEnabled = Utils.TryReadEnvValue("SCREENSHOT_ON_FAILURE");
            Environment.SetEnvironmentVariable("SCREENSHOT_ON_FAILURE", "false");

            var protoExecutionResult =
                new MethodExecutor(mockSandBox.Object).ExecuteHooks("hooks", hooksStrategy, new List<string>(),
                    new ExecutionContext());

            mockSandBox.VerifyAll();
            Assert.False(protoExecutionResult.FailureScreenshot == null);
            Environment.SetEnvironmentVariable("SCREENSHOT_ON_FAILURE", screenshotEnabled);
        }

        [Test]
        public void ShouldExecuteMethod()
        {
            var mockSandBox = new Mock<ISandbox>();
            var gaugeMethod = new GaugeMethod {Name = "ShouldExecuteMethod", ParameterCount = 1};
            var args = new[] {"Bar", "String"};
            mockSandBox.Setup(sandbox => sandbox.ExecuteMethod(gaugeMethod, It.IsAny<string[]>()))
                .Returns(() => new ExecutionResult {Success = true})
                .Callback(() => Thread.Sleep(1)); // Simulate a delay in method execution

            var executionResult = new MethodExecutor(mockSandBox.Object).Execute(gaugeMethod, args);

            mockSandBox.VerifyAll();
            Assert.False(executionResult.Failed);
            Assert.True(executionResult.ExecutionTime > 0);
        }

        [Test]
        public void ShouldNotTakeScreenShotWhenDisabled()
        {
            var mockSandBox = new Mock<ISandbox>();
            var gaugeMethod = new GaugeMethod {Name = "ShouldNotTakeScreenShotWhenDisabled", ParameterCount = 1};

            var result = new ExecutionResult
            {
                Success = false,
                ExceptionMessage = "Some Error",
                StackTrace = "StackTrace"
            };
            mockSandBox.Setup(sandbox => sandbox.ExecuteMethod(gaugeMethod, It.IsAny<string[]>())).Returns(result);

            var screenshotEnabled = Utils.TryReadEnvValue("SCREENSHOT_ON_FAILURE");
            Environment.SetEnvironmentVariable("SCREENSHOT_ON_FAILURE", "false");

            var executionResult = new MethodExecutor(mockSandBox.Object).Execute(gaugeMethod, "Bar", "String");

            mockSandBox.VerifyAll();
            Assert.False(executionResult.FailureScreenshot == null);
            Environment.SetEnvironmentVariable("SCREENSHOT_ON_FAILURE", screenshotEnabled);
        }

        [Test]
        [Ignore("Screenshots are not available in CI - to use Gauge_screenshot instead")]
        public void ShouldTakeScreenShotOnFailedExecution()
        {
            var mockSandBox = new Mock<ISandbox>();
            var gaugeMethod = new GaugeMethod {Name = "ShouldExecuteMethod", ParameterCount = 1};
            mockSandBox.Setup(sandbox => sandbox.ExecuteMethod(gaugeMethod, "Bar")).Throws<Exception>();

            var executionResult = new MethodExecutor(mockSandBox.Object).Execute(gaugeMethod, "Bar", "String");

            mockSandBox.VerifyAll();
            Assert.True(executionResult.Failed);
            Assert.True(executionResult.FailureScreenshot != null);
            Assert.True(executionResult.FailureScreenshot.Length > 0);
        }

        [Test]
        public void ShouldTakeScreenShotUsingCustomScreenShotMethod()
        {
            var mockSandBox = new Mock<ISandbox>();
            var gaugeMethod = new GaugeMethod
            {
                Name = "ShouldTakeScreenShotUsingCustomScreenShotMethod",
                ParameterCount = 1
            };

            var result = new ExecutionResult
            {
                Success = false,
                ExceptionMessage = "Some Error",
                StackTrace = "StackTrace"
            };
            mockSandBox.Setup(sandbox => sandbox.ExecuteMethod(gaugeMethod, It.IsAny<string[]>())).Returns(result);

            byte[] bytes = {0x20, 0x20};
            mockSandBox.Setup(sandbox => sandbox.TryScreenCapture(out bytes)).Returns(true);

            var executionResult = new MethodExecutor(mockSandBox.Object).Execute(gaugeMethod, "Bar", "String");

            mockSandBox.VerifyAll();
            Assert.True(executionResult.Failed);
            Assert.IsNotEmpty(executionResult.FailureScreenshot);
            Assert.True(executionResult.FailureScreenshot.Length > 0);
        }
    }
}