using NUnit.Framework;
using System;
using System.IO;

namespace Gauge.CSharp.Runner.UnitTests
{
    class GaugeProjectBuilderTest
    {
        private readonly string TempPath = Path.GetTempPath();
        IGaugeProjectBuilder projectBuilder = new GaugeProjectBuilder();

        public void ShouldReturnTrueForExistingProject()
        {
            Environment.SetEnvironmentVariable("GAUGE_CSHARP_PROJECT_FILE", TempPath);
            bool isProjectBuild = projectBuilder.BuildTargetGaugeProject();

            Assert.True(isProjectBuild);
            Environment.SetEnvironmentVariable("GAUGE_CSHARP_PROJECT_FILE", null);
        }

        [Test]
        public void ShouldReturnFalseForNonExistingProject()
        {
            Environment.SetEnvironmentVariable("GAUGE_CSHARP_PROJECT_FILE", "/tmp/foo");
            bool isProjectBuild = projectBuilder.BuildTargetGaugeProject();

            Assert.False(isProjectBuild);
            Environment.SetEnvironmentVariable("GAUGE_CSHARP_PROJECT_FILE", null);
        }
    }
}
