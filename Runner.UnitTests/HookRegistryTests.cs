// Copyright 2015 ThoughtWorks, Inc.

// This file is part of Gauge-CSharp.

// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

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