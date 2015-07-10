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

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }
    }
}
