using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.IntegrationTests
{
    [TestFixture]
    public class SandBoxTests
    {
        readonly string _testProjectPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\IntegrationTestSample");

        [TestCase]
        public void ShouldLoadTargetLibAssemblyInSandbox()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", _testProjectPath);
            var sandbox = SandboxFactory.Create(AppDomain.CurrentDomain.SetupInformation);

            // The sample project uses a special version of Gauge Lib, versioned 0.0.0 for testing.
            // The actual Gauge CSharp runner uses a different version of Lib 
            Assert.AreEqual(FileVersionInfo.GetVersionInfo(sandbox.TargetLibAssembly.Location).ProductVersion, "0.0.0");

            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }
    }
}
