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
using System.Linq.Expressions;
using System.Reflection;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Runner.Models;
using Gauge.CSharp.Runner.Strategy;
using Gauge.CSharp.Runner.Wrappers;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    public class SandboxHookExecutionTests
    {
        private static readonly IEnumerable<string> HookTypes = Hooks.Keys;
        private IList<string> _applicableTags;
        private string _gaugeProjectRootEnv;
        private HashSet<IHookMethod> _hookMethods;
        private Mock<IAssemblyLoader> _mockAssemblyLoader;
        private Mock<IFileWrapper> _mockFileWrapper;
        private Mock<IHookRegistry> _mockHookRegistry;
        private Mock<IHooksStrategy> _mockStrategy;

        private static Dictionary<string, Expression<Func<IHookRegistry, HashSet<IHookMethod>>>> Hooks
        {
            get
            {
                return new Dictionary<string, Expression<Func<IHookRegistry, HashSet<IHookMethod>>>>
                {
                    {"BeforeSuite", x => x.BeforeSuiteHooks},
                    {"BeforeSpec", x => x.BeforeSpecHooks},
                    {"BeforeScenario", x => x.BeforeScenarioHooks},
                    {"BeforeStep", x => x.BeforeStepHooks},
                    {"AfterStep", x => x.AfterStepHooks},
                    {"AfterScenario", x => x.AfterScenarioHooks},
                    {"AfterSpec", x => x.AfterSpecHooks},
                    {"AfterSuite", x => x.AfterSuiteHooks}
                };
            }
        }

        [SetUp]
        public void Setup()
        {
            _gaugeProjectRootEnv = Environment.GetEnvironmentVariable("GAUGE_PROJECT_ROOT");
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", Directory.GetCurrentDirectory());
            _mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockAssembly = new Mock<TestAssembly>();
            _mockAssemblyLoader.Setup(loader => loader.AssembliesReferencingGaugeLib)
                .Returns(new List<Assembly> {mockAssembly.Object});
            _mockAssemblyLoader.Setup(loader => loader.ScreengrabberTypes)
                .Returns(new List<Type> {typeof(DefaultScreenGrabber)});
            _mockAssemblyLoader.Setup(loader => loader.ClassInstanceManagerTypes)
                .Returns(new List<Type> {typeof(DefaultClassInstanceManager)});
            _mockAssemblyLoader.Setup(loader => loader.GetTargetLibAssembly()).Returns(typeof(ExecutionContext).Assembly);
            _mockHookRegistry = new Mock<IHookRegistry>();
            var mockHookMethod = new Mock<IHookMethod>();
            mockHookMethod.Setup(method => method.Method).Returns("DummyHook");
            _hookMethods = new HashSet<IHookMethod> {mockHookMethod.Object};
            _mockFileWrapper = new Mock<IFileWrapper>();
            _mockStrategy = new Mock<IHooksStrategy>();
            _applicableTags = Enumerable.Empty<string>().ToList();
            _mockStrategy.Setup(strategy => strategy.GetApplicableHooks(_applicableTags, _hookMethods))
                .Returns(new[] {"DummyHook"});
        }

        [Test]
        [TestCaseSource("HookTypes")]
        public void ShouldExecuteHook(string hookType)
        {
            var expression = Hooks[hookType];
            _mockHookRegistry.Setup(registry => registry.MethodFor("DummyHook"))
                .Returns(GetType().GetMethod("DummyHook"));
            _mockHookRegistry.Setup(expression).Returns(_hookMethods).Verifiable();

            var sandbox = new Sandbox(_mockAssemblyLoader.Object, _mockHookRegistry.Object, _mockFileWrapper.Object);
            var executionResult = sandbox.ExecuteHooks(hookType, _mockStrategy.Object, _applicableTags, new ExecutionContext());

            Assert.IsTrue(executionResult.Success);
            _mockHookRegistry.VerifyAll();
        }

        [Test]
        [TestCaseSource("HookTypes")]
        public void ShouldExecuteHookAndReportFailureOnException(string hookType)
        {
            var expression = Hooks[hookType];
            _mockHookRegistry.Setup(registry => registry.MethodFor("DummyHook"))
                .Returns(GetType().GetMethod("DummyHookThrowsException"));
            _mockHookRegistry.Setup(expression).Returns(_hookMethods).Verifiable();

            var sandbox = new Sandbox(_mockAssemblyLoader.Object, _mockHookRegistry.Object, _mockFileWrapper.Object);
            var executionResult = sandbox.ExecuteHooks(hookType, _mockStrategy.Object, _applicableTags, new ExecutionContext());

            Assert.False(executionResult.Success);
            Assert.AreEqual("foo", executionResult.ExceptionMessage);
        }

        [Test]
        [TestCaseSource("HookTypes")]
        public void ShouldExecuteHookWithExecutionContext(string hookType)
        {
            var expression = Hooks[hookType];
            _mockHookRegistry.Setup(registry => registry.MethodFor("DummyHook"))
                .Returns(GetType().GetMethod("DummyHookTakesExecutionContext"));
            _mockHookRegistry.Setup(expression).Returns(_hookMethods).Verifiable();

            var sandbox = new Sandbox(_mockAssemblyLoader.Object, _mockHookRegistry.Object, _mockFileWrapper.Object);
            var expected = new ExecutionContext(new ExecutionContext.Specification("TestSpec", "TestFileName", false, new List<string>()), null, null);
            var executionResult = sandbox.ExecuteHooks(hookType, _mockStrategy.Object, _applicableTags, expected);

            Assert.True(executionResult.Success);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", _gaugeProjectRootEnv);
        }

        public void DummyHook(ExecutionContext executionContext)
        {
        }

        public void DummyHookThrowsException()
        {
            throw new Exception("foo");
        }

        public void DummyHookTakesExecutionContext(ExecutionContext executionContext){
            if (executionContext.CurrentSpecification.Name != "TestSpec")
                throw new Exception("TestSpec does not exist in ExecutionContext");
        }
    }
}