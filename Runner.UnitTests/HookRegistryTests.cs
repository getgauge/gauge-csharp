using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    [TestFixture]
    public class HookRegistryTests
    {
        public void DummmyHook(){}

        [Test]
        public void ShouldAddAndGetBeforeScenarioHook()
        {
            var hookRegistry = new HookRegistry();
            var methodInfo = GetType().GetMethod("DummyHook");
            hookRegistry.AddBeforeScenarioHooks(new [] {methodInfo});
            Assert.True(hookRegistry.BeforeScenarioHooks.Contains(methodInfo));
        }

        [Test]
        public void ShouldAddAndGetAfterScenarioHook()
        {
            var hookRegistry = new HookRegistry();
            var methodInfo = GetType().GetMethod("DummyHook");
            hookRegistry.AddAfterScenarioHooks(new [] {methodInfo});
            Assert.True(hookRegistry.AfterScenarioHooks.Contains(methodInfo));
        }

        [Test]
        public void ShouldAddAndGetBeforeSpecHook()
        {
            var hookRegistry = new HookRegistry();
            var methodInfo = GetType().GetMethod("DummyHook");
            hookRegistry.AddBeforeSpecHooks(new [] {methodInfo});
            Assert.True(hookRegistry.BeforeSpecHooks.Contains(methodInfo));
        }

        [Test]
        public void ShouldAddAndGetAfterSpecHook()
        {
            var hookRegistry = new HookRegistry();
            var methodInfo = GetType().GetMethod("DummyHook");
            hookRegistry.AddAfterSpecHooks(new [] {methodInfo});
            Assert.True(hookRegistry.AfterSpecHooks.Contains(methodInfo));
        }

        [Test]
        public void ShouldAddAndGetBeforeStepHook()
        {
            var hookRegistry = new HookRegistry();
            var methodInfo = GetType().GetMethod("DummyHook");
            hookRegistry.AddBeforeStepHooks(new [] {methodInfo});
            Assert.True(hookRegistry.BeforeStepHooks.Contains(methodInfo));
        }

        [Test]
        public void ShouldAddAndGetAfterStepHook()
        {
            var hookRegistry = new HookRegistry();
            var methodInfo = GetType().GetMethod("DummyHook");
            hookRegistry.AddAfterStepHooks(new [] {methodInfo});
            Assert.True(hookRegistry.AfterStepHooks.Contains(methodInfo));
        }

        [Test]
        public void ShouldAddAndGetBeforeSuiteHook()
        {
            var hookRegistry = new HookRegistry();
            var methodInfo = GetType().GetMethod("DummyHook");
            hookRegistry.AddBeforeSuiteHooks(new [] {methodInfo});
            Assert.True(hookRegistry.BeforeSuiteHooks.Contains(methodInfo));
        }

        [Test]
        public void ShouldAddAndGetAfterSuiteHook()
        {
            var hookRegistry = new HookRegistry();
            var methodInfo = GetType().GetMethod("DummyHook");
            hookRegistry.AddAfterSuiteHooks(new [] {methodInfo});
            Assert.True(hookRegistry.AfterSuiteHooks.Contains(methodInfo));
        }
    }
}