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
using Gauge.Messages;
using Moq;
using NUnit.Framework;
using Gauge.CSharp.Core;
using Gauge.CSharp.Runner.Strategy;

namespace Gauge.CSharp.Runner.UnitTests
{
    [TestFixture]
    public class MethodExecutorTests
    {
        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", Directory.GetDirectoryRoot(Assembly.GetExecutingAssembly().Location));
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }

        [Test]
        public void ShouldExecuteMethod()
        {
            var mockSandBox = new Mock<ISandbox>();
            var method = new Mock<GaugeMethod>();
            method.Setup(info => info.Name).Returns("ShouldExecuteMethod");
            method.Setup(info => info.ParameterCount).Returns(1);
            mockSandBox.Setup(sandbox => sandbox.ExecuteMethod(method.Object, "Bar"))
                .Returns(() => new ExecutionResult {Success = true})
                .Callback(() => Thread.Sleep(1)); // Simulate a delay in method execution

            var executionResult = new MethodExecutor(mockSandBox.Object).Execute(method.Object, new KeyValuePair<string, string>("Bar", "String"));
            
            mockSandBox.VerifyAll();
            Assert.False(executionResult.Failed);
            Assert.True(executionResult.ExecutionTime > 0);
        }

        [Test, Ignore("Screenshots are not available in CI - to use Gauge_screenshot instead")]
        public void ShouldTakeScreenShotOnFailedExecution()
        {
            var mockSandBox = new Mock<ISandbox>();
            var method = new Mock<GaugeMethod>();
            method.Setup(info => info.Name).Returns("ShouldTakeScreenShotOnFailedExecution");
            method.Setup(info => info.ParameterCount).Returns(1);
            mockSandBox.Setup(sandbox => sandbox.ExecuteMethod(method.Object, "Bar")).Throws<Exception>();

            var executionResult = new MethodExecutor(mockSandBox.Object).Execute(method.Object, new KeyValuePair<string, string>("Bar", "String"));
            
            mockSandBox.VerifyAll();
            Assert.True(executionResult.Failed);
            Assert.True(executionResult.HasScreenShot);
            Assert.True(executionResult.ScreenShot.Length > 0);
        }
        
        [Test]
        public void ShouldTakeScreenShotUsingCustomScreenShotMethod()
        {
            var mockSandBox = new Mock<ISandbox>();
            var method = new Mock<GaugeMethod>();
            method.Setup(info => info.Name).Returns("ShouldTakeScreenShotUsingCustomScreenShotMethod");
            method.Setup(info => info.ParameterCount).Returns(1);

            var result = new ExecutionResult {Success = false, ExceptionMessage = "Some Error", StackTrace = "StackTrace"};
            mockSandBox.Setup(sandbox => sandbox.ExecuteMethod(method.Object, "Bar")).Returns(result);

            byte[] bytes = {0x20, 0x20};
            mockSandBox.Setup(sandbox => sandbox.TryScreenCapture(out bytes)).Returns(true);

            var executionResult = new MethodExecutor(mockSandBox.Object).Execute(method.Object, new KeyValuePair<string, string>("Bar", "String"));
            
            mockSandBox.VerifyAll();
            Assert.True(executionResult.Failed);
            Assert.True(executionResult.HasScreenShot);
            Assert.AreEqual(2, executionResult.ScreenShot.Length);
        }

        [Test]
        public void ShouldNotTakeScreenShotWhenDisabled()
        {
            var mockSandBox = new Mock<ISandbox>();
            var method = new Mock<GaugeMethod>();
            method.Setup(info => info.Name).Returns("ShouldNotTakeScreenShotWhenDisabled");
            method.Setup(info => info.ParameterCount).Returns(1);

            var result = new ExecutionResult { Success = false, ExceptionMessage = "Some Error", StackTrace = "StackTrace" };
            mockSandBox.Setup(sandbox => sandbox.ExecuteMethod(method.Object, "Bar")).Returns(result);

            var screenshotEnabled = Utils.TryReadEnvValue("SCREENSHOT_ENABLED");
            Environment.SetEnvironmentVariable("SCREENSHOT_ENABLED", "false");
            
            var executionResult = new MethodExecutor(mockSandBox.Object).Execute(method.Object, new KeyValuePair<string, string>("Bar", "String"));
            
            mockSandBox.VerifyAll();
            Assert.False(executionResult.HasScreenShot);
            Environment.SetEnvironmentVariable("SCREENSHOT_ENABLED", screenshotEnabled);
        }

        [Test]
        public void ShouldExecuteHooks()
        {
            var mockSandBox = new Mock<ISandbox>();
            var method = new Mock<GaugeMethod>();
            method.Setup(info => info.Name).Returns("ShouldExecuteHooks");
            method.Setup(info => info.ParameterCount).Returns(1);
            var hooksStrategy = new HooksStrategy();
            ExecutionInfo executionInfo = ExecutionInfo.CreateBuilder().Build();
            var protoExecutionResultBuilder = ProtoExecutionResult.CreateBuilder();
            mockSandBox.Setup(sandbox =>
                sandbox.ExecuteHooks("hook", hooksStrategy, new List<string>(), executionInfo)
            ).Returns(protoExecutionResultBuilder);
            mockSandBox.Setup(sandbox => sandbox.GetAllPendingMessages()).Returns(new List<string>());

            var executionResult = new MethodExecutor(mockSandBox.Object).ExecuteHooks("hooks", hooksStrategy, new List<string>(), executionInfo);
            Console.WriteLine(executionResult.ErrorMessage);
            Assert.False(executionResult.Failed);
            Assert.True(executionResult.ExecutionTime > 0);
        }
    }
}