using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Gauge.Messages;
using Moq;
using NUnit.Framework;

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

        [Test]
        public void ShouldExecuteMethod()
        {
            var mockSandBox = new Mock<ISandbox>();
            var method = new Mock<MethodInfo>();
            mockSandBox.Setup(sandbox => sandbox.ExecuteMethod(method.Object, "Bar")).Callback(() => Thread.Sleep(1)); // Simulate a delay in method execution

            var executionResult = new MethodExecutor(mockSandBox.Object).Execute(method.Object, "Bar");
            
            mockSandBox.VerifyAll();
            Assert.False(executionResult.Failed);
            Assert.True(executionResult.ExecutionTime > 0);
        }

        [Test, Ignore("Screenshots are not available in CI - to use Gauge_screenshot instead")]
        public void ShouldTakeScreenShotOnFailedExecution()
        {
            var mockSandBox = new Mock<ISandbox>();
            var method = new Mock<MethodInfo>();
            mockSandBox.Setup(sandbox => sandbox.ExecuteMethod(method.Object, "Bar")).Throws<Exception>();

            var executionResult = new MethodExecutor(mockSandBox.Object).Execute(method.Object, "Bar");
            
            mockSandBox.VerifyAll();
            Assert.True(executionResult.Failed);
            Assert.True(executionResult.HasScreenShot);
            Assert.True(executionResult.ScreenShot.Length > 0);
        }

        [Test]
        public void ShouldNotTakeScreenShotWhenDisabled()
        {
            var mockSandBox = new Mock<ISandbox>();
            var method = new Mock<MethodInfo>();
            mockSandBox.Setup(sandbox => sandbox.ExecuteMethod(method.Object, "Bar")).Throws<Exception>();
            var screenshotEnabled = Environment.GetEnvironmentVariable("screenshot_enabled");
            Environment.SetEnvironmentVariable("screenshot_enabled", "false");
            
            var executionResult = new MethodExecutor(mockSandBox.Object).Execute(method.Object, "Bar");
            
            mockSandBox.VerifyAll();
            Assert.False(executionResult.HasScreenShot);
            Environment.SetEnvironmentVariable("screenshot_enabled", screenshotEnabled);
        }

        [Test]
        public void ShouldExecuteHooks()
        {
            var mockSandBox = new Mock<ISandbox>();
            var method = new Mock<MethodInfo>();
            mockSandBox.Setup(sandbox => sandbox.ExecuteMethod(method.Object));

            var executionResult = new MethodExecutor(mockSandBox.Object).ExecuteHooks(new[] { method.Object },
                ExecutionInfo.CreateBuilder().Build());
            Console.WriteLine(executionResult.ErrorMessage);
            Assert.False(executionResult.Failed);
            Assert.True(executionResult.ExecutionTime > 0);
        }
    }
}