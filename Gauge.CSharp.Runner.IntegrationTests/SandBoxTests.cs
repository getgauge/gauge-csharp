using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.IntegrationTests
{
    [TestFixture]
    public class SandBoxTests
    {
        readonly string _testProjectPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\IntegrationTestSample");

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", _testProjectPath);
        }

        [TestCase]
        public void ShouldLoadTargetLibAssemblyInSandbox()
        {
            var sandbox = SandboxFactory.Create(AppDomain.CurrentDomain.SetupInformation);

            // The sample project uses a special version of Gauge Lib, versioned 0.0.0 for testing.
            // The actual Gauge CSharp runner uses a different version of Lib 
            Assert.AreEqual(FileVersionInfo.GetVersionInfo(sandbox.TargetLibAssembly.Location).ProductVersion, "0.0.0");
        }

        [TestCase]
        public void ShouldGetAllStepMethods()
        {
            var sandbox = SandboxFactory.Create(AppDomain.CurrentDomain.SetupInformation);
            var stepMethods = sandbox.GetStepMethods();

            Assert.AreEqual(3, stepMethods.Count);
        }

        [TestCase]
        public void ShouldGetAllStepTexts()
        {
            var sandbox = SandboxFactory.Create(AppDomain.CurrentDomain.SetupInformation);
            var stepTexts = sandbox.GetAllStepTexts().ToList();

            new List<string>
            {
                "Say <what> to <who>",
                "A context step which gets executed before every scenario",
                "Step that takes a table <table>"
            }.ForEach(s => Assert.Contains(s, stepTexts));
        }

        [TestCase]
        public void ShouldGetBeforeSuiteHooks()
        {
            var sandbox = SandboxFactory.Create(AppDomain.CurrentDomain.SetupInformation);
            var hookRegistry = sandbox.GetHookRegistry();

            Assert.AreEqual(1, hookRegistry.BeforeSuiteHooks.Count);

            var hookMethod = hookRegistry.BeforeSuiteHooks.First();
            Assert.AreEqual("BeforeSuite", hookMethod.Method.Name);
        }

        [TestCase]
        public void ShouldGetAfterSuiteHooks()
        {
            var sandbox = SandboxFactory.Create(AppDomain.CurrentDomain.SetupInformation);
            var hookRegistry = sandbox.GetHookRegistry();

            Assert.AreEqual(1, hookRegistry.AfterSuiteHooks.Count);

            var hookMethod = hookRegistry.AfterSuiteHooks.First();
            Assert.AreEqual("AfterSuite", hookMethod.Method.Name);
        }

        [TestCase]
        public void ShouldGetBeforeScenarioHooks()
        {
            var sandbox = SandboxFactory.Create(AppDomain.CurrentDomain.SetupInformation);
            var hookRegistry = sandbox.GetHookRegistry();

            Assert.AreEqual(1, hookRegistry.BeforeScenarioHooks.Count);

            var hookMethod = hookRegistry.BeforeScenarioHooks.First();
            Assert.AreEqual("BeforeScenario", hookMethod.Method.Name);
        }

        [TestCase]
        public void ShouldGetAfterScenarioHooks()
        {
            var sandbox = SandboxFactory.Create(AppDomain.CurrentDomain.SetupInformation);
            var hookRegistry = sandbox.GetHookRegistry();

            Assert.AreEqual(1, hookRegistry.AfterScenarioHooks.Count);

            var hookMethod = hookRegistry.AfterScenarioHooks.First();
            Assert.AreEqual("AfterScenario", hookMethod.Method.Name);
        }

        [TestCase]
        public void ShouldGetBeforeSpecHooks()
        {
            var sandbox = SandboxFactory.Create(AppDomain.CurrentDomain.SetupInformation);
            var hookRegistry = sandbox.GetHookRegistry();

            Assert.AreEqual(1, hookRegistry.BeforeSpecHooks.Count);

            var hookMethod = hookRegistry.BeforeSpecHooks.First();
            Assert.AreEqual("BeforeSpec", hookMethod.Method.Name);
        }

        [TestCase]
        public void ShouldGetAfterSpecHooks()
        {
            var sandbox = SandboxFactory.Create(AppDomain.CurrentDomain.SetupInformation);
            var hookRegistry = sandbox.GetHookRegistry();

            Assert.AreEqual(1, hookRegistry.AfterSpecHooks.Count);

            var hookMethod = hookRegistry.AfterSpecHooks.First();
            Assert.AreEqual("AfterSpec", hookMethod.Method.Name);
        }

        [TestCase]
        public void ShouldGetBeforeStepHooks()
        {
            var sandbox = SandboxFactory.Create(AppDomain.CurrentDomain.SetupInformation);
            var hookRegistry = sandbox.GetHookRegistry();

            Assert.AreEqual(1, hookRegistry.BeforeStepHooks.Count);

            var hookMethod = hookRegistry.BeforeStepHooks.First();
            Assert.AreEqual("BeforeStep", hookMethod.Method.Name);
        }

        [TestCase]
        public void ShouldGetAfterStepHooks()
        {
            var sandbox = SandboxFactory.Create(AppDomain.CurrentDomain.SetupInformation);
            var hookRegistry = sandbox.GetHookRegistry();

            Assert.AreEqual(1, hookRegistry.AfterStepHooks.Count);

            var hookMethod = hookRegistry.AfterStepHooks.First();
            Assert.AreEqual("AfterStep", hookMethod.Method.Name);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }
    }
}
