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
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Gauge.CSharp.Lib.Attribute;

namespace Gauge.CSharp.Runner.IntegrationTests
{
    [TestFixture]
    public class SandBoxTests
    {		
		private readonly string _testProjectPath = TestUtils.GetIntegrationTestSampleDirectory();

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", _testProjectPath);
        }

		static AppDomainSetup SetupInformation ()
		{
			return AppDomain.CurrentDomain.SetupInformation;
		}

		static void AssertRunnerDomainDidNotLoadUsersAssembly ()
		{
			Assert.AreNotEqual ("0.0.0", FileVersionInfo.GetVersionInfo (typeof(AfterScenario).Assembly.Location).ProductVersion,
				"Runner's test domain should not load the Gauge.CSharp.Lib assembly with 0.0.0 version");
			// 0.0.0 version should be only loaded in sandbox. 
			// Runner should have its own version, the one we just built in this project
		}

        [Test]
        public void ShouldLoadTargetLibAssemblyInSandbox()
        {
            var sandbox = SandboxFactory.Create (SetupInformation ());

            // The sample project uses a special version of Gauge Lib, versioned 0.0.0 for testing.
            // The actual Gauge CSharp runner uses a different version of Lib 
			// used by sample project
			Assert.AreEqual("0.0.0",sandbox.TargetLibAssemblyVersion);
			// used by runner
			AssertRunnerDomainDidNotLoadUsersAssembly ();
        }

		[Test]
		public void ShouldNotLoadTargetLibAssemblyInRunnersDomain()
		{
			SandboxFactory.Create (SetupInformation ());

			// The sample project uses a special version of Gauge Lib, versioned 0.0.0 for testing.
			// The actual Gauge CSharp runner uses a different version of Lib 
			// used by runner
			AssertRunnerDomainDidNotLoadUsersAssembly ();
		}

        [Test]
        public void ShouldGetAllStepMethods()
        {
            var sandbox = SandboxFactory.Create(SetupInformation());
			AssertRunnerDomainDidNotLoadUsersAssembly ();
            var stepMethods = sandbox.GetStepMethods();

            Assert.AreEqual(9, stepMethods.Count);
        }

        [Test]
        public void ShouldGetAllStepTexts()
        {
            var sandbox = SandboxFactory.Create(SetupInformation());
            var stepTexts = sandbox.GetAllStepTexts().ToList();

            new List<string>
            {
                "Say <what> to <who>",
                "A context step which gets executed before every scenario",
                "Step that takes a table <table>",
                "Refactoring Say <what> to <who>",
                "Refactoring A context step which gets executed before every scenario",
                "Refactoring Step that takes a table <table>"
            }.ForEach(s => Assert.Contains(s, stepTexts));
        }

        [Test]
        public void ShouldGetBeforeSuiteHooks()
        {
            var sandbox = SandboxFactory.Create(SetupInformation());
            var hookRegistry = sandbox.GetHookRegistry();
			AssertRunnerDomainDidNotLoadUsersAssembly ();

            Assert.AreEqual(1, hookRegistry.BeforeSuiteHooks.Count);

            var hookMethod = hookRegistry.BeforeSuiteHooks.First();
            Assert.AreEqual("BeforeSuite", hookMethod.Method.Name);
        }

        [Test]
        public void ShouldGetAfterSuiteHooks()
        {
            var sandbox = SandboxFactory.Create(SetupInformation());
            var hookRegistry = sandbox.GetHookRegistry();
			AssertRunnerDomainDidNotLoadUsersAssembly ();

            Assert.AreEqual(1, hookRegistry.AfterSuiteHooks.Count);

            var hookMethod = hookRegistry.AfterSuiteHooks.First();
            Assert.AreEqual("AfterSuite", hookMethod.Method.Name);
        }

        [Test]
        public void ShouldGetBeforeScenarioHooks()
        {
            var sandbox = SandboxFactory.Create(SetupInformation());
            var hookRegistry = sandbox.GetHookRegistry();
			AssertRunnerDomainDidNotLoadUsersAssembly ();

            Assert.AreEqual(1, hookRegistry.BeforeScenarioHooks.Count);

            var hookMethod = hookRegistry.BeforeScenarioHooks.First();
            Assert.AreEqual("BeforeScenario", hookMethod.Method.Name);
        }

        [Test]
        public void ShouldGetAfterScenarioHooks()
        {
            var sandbox = SandboxFactory.Create(SetupInformation());
            var hookRegistry = sandbox.GetHookRegistry();
			AssertRunnerDomainDidNotLoadUsersAssembly ();

            Assert.AreEqual(1, hookRegistry.AfterScenarioHooks.Count);

            var hookMethod = hookRegistry.AfterScenarioHooks.First();
            Assert.AreEqual("AfterScenario", hookMethod.Method.Name);
        }

        [Test]
        public void ShouldGetBeforeSpecHooks()
        {
            var sandbox = SandboxFactory.Create(SetupInformation());
            var hookRegistry = sandbox.GetHookRegistry();
			AssertRunnerDomainDidNotLoadUsersAssembly ();

            Assert.AreEqual(1, hookRegistry.BeforeSpecHooks.Count);

            var hookMethod = hookRegistry.BeforeSpecHooks.First();
            Assert.AreEqual("BeforeSpec", hookMethod.Method.Name);
        }

        [Test]
        public void ShouldGetAfterSpecHooks()
        {
            var sandbox = SandboxFactory.Create(SetupInformation());
            var hookRegistry = sandbox.GetHookRegistry();
			AssertRunnerDomainDidNotLoadUsersAssembly ();

            Assert.AreEqual(1, hookRegistry.AfterSpecHooks.Count);

            var hookMethod = hookRegistry.AfterSpecHooks.First();
            Assert.AreEqual("AfterSpec", hookMethod.Method.Name);
        }

        [Test]
        public void ShouldGetBeforeStepHooks()
        {
            var sandbox = SandboxFactory.Create(SetupInformation());
            var hookRegistry = sandbox.GetHookRegistry();
			AssertRunnerDomainDidNotLoadUsersAssembly ();

            Assert.AreEqual(1, hookRegistry.BeforeStepHooks.Count);

            var hookMethod = hookRegistry.BeforeStepHooks.First();
            Assert.AreEqual("BeforeStep", hookMethod.Method.Name);
        }

        [Test]
        public void ShouldGetAfterStepHooks()
        {
            var sandbox = SandboxFactory.Create(SetupInformation());
            var hookRegistry = sandbox.GetHookRegistry();
			AssertRunnerDomainDidNotLoadUsersAssembly ();

            Assert.AreEqual(1, hookRegistry.AfterStepHooks.Count);

            var hookMethod = hookRegistry.AfterStepHooks.First();
            Assert.AreEqual("AfterStep", hookMethod.Method.Name);
        }

        [Test]
        public void ShouldExecuteMethodAndReturnResult()
        {
            var sandbox = SandboxFactory.Create(SetupInformation());
            var stepMethods = sandbox.GetStepMethods();
			AssertRunnerDomainDidNotLoadUsersAssembly ();
            var methodInfo = stepMethods.First(info => string.CompareOrdinal(info.Name, "Context") == 0);

            var executionResult = sandbox.ExecuteMethod(methodInfo);
            Assert.True(executionResult.Success);
        }

        [Test]
        public void SuccessIsFalseOnUnserializableExceptionThrown()
        {
            const string expectedMessage = "I am a custom exception";
            var sandbox = SandboxFactory.Create(SetupInformation());
            var stepMethods = sandbox.GetStepMethods();
			AssertRunnerDomainDidNotLoadUsersAssembly ();
            var methodInfo = stepMethods.First(info => string.CompareOrdinal(info.Name, "ThrowUnserializableException") ==0);

            var executionResult = sandbox.ExecuteMethod(methodInfo);
            Assert.False(executionResult.Success);
            Assert.AreEqual(expectedMessage, executionResult.ExceptionMessage);
			StringAssert.Contains("IntegrationTestSample.StepImplementation.ThrowUnserializableException",executionResult.StackTrace);
        }

        [Test]
        public void SuccessIsFalseOnSerializableExceptionThrown()
        {
            const string expectedMessage = "I am a custom serializable exception";
            var sandbox = SandboxFactory.Create(SetupInformation());
            var stepMethods = sandbox.GetStepMethods();
            var methodInfo = stepMethods.First(info => string.CompareOrdinal(info.Name, "ThrowSerializableException") ==0);

            var executionResult = sandbox.ExecuteMethod(methodInfo);

            Assert.False(executionResult.Success);
            Assert.AreEqual(expectedMessage, executionResult.ExceptionMessage);
			StringAssert.Contains("IntegrationTestSample.StepImplementation.ThrowSerializableException",executionResult.StackTrace);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
			AssertRunnerDomainDidNotLoadUsersAssembly ();
        }
    }
}
