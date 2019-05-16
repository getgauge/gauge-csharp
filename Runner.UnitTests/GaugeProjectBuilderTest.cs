using NUnit.Framework;
using System;
using System.IO;

namespace Gauge.CSharp.Runner.UnitTests
{
    class GaugeProjectBuilderTest
    {
        IGaugeProjectBuilder projectBuilder = new GaugeProjectBuilder();

        public void ShouldReturnTrueForExistingProject()
        {
            var tempPath = Path.GetTempPath();
            Environment.SetEnvironmentVariable("GAUGE_CSHARP_PROJECT_FILE", tempPath);
            var isProjectBuild = projectBuilder.BuildTargetGaugeProject();

            Assert.True(isProjectBuild);
            Environment.SetEnvironmentVariable("GAUGE_CSHARP_PROJECT_FILE", null);
        }

        [Test]
        public void ShouldReturnFalseForNonExistingProject()
        {
            Environment.SetEnvironmentVariable("GAUGE_CSHARP_PROJECT_FILE", "/tmp/foo");
            var isProjectBuild = projectBuilder.BuildTargetGaugeProject();

            Assert.False(isProjectBuild);
            Environment.SetEnvironmentVariable("GAUGE_CSHARP_PROJECT_FILE", null);
        }
    }
}
