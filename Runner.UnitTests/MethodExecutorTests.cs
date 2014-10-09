using System;
using main;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    [TestFixture]
    public class MethodExecutorTests
    {
        public void Foo(string foo)
        {
            Console.WriteLine(foo);
        }

        public void Bar() { }

        public void ErrorFoo(string foo)
        {
            throw new Exception(foo);
        }

        [Test]
        public void ShouldExecuteMethod()
        {
            var executionResult = new MethodExecutor().Execute(GetType().GetMethod("Foo"), "Bar");
            
            Assert.False(executionResult.Failed);
            Assert.True(executionResult.ExecutionTime > 0);
        }

        [Test]
        public void ShouldTakeScreenShotOnFailedExecution()
        {
            var executionResult = new MethodExecutor().Execute(GetType().GetMethod("ErrorFoo"), "Bar");
            
            Assert.True(executionResult.Failed);
            Assert.True(executionResult.HasScreenShot);
            Assert.True(executionResult.ScreenShot.Length > 0);
        }

        [Test]
        public void ShouldExecuteHooks()
        {
            var executionResult = new MethodExecutor().ExecuteHooks(new[] {GetType().GetMethod("Bar")},
                ExecutionInfo.CreateBuilder().Build());

            Assert.False(executionResult.Failed);
            Assert.True(executionResult.ExecutionTime > 0);

        }
    }
}