using System;
using System.IO;
using Gauge.CSharp.Runner.Communication;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    [TestFixture]
    class UtilsTest
    {
        [Test]
        public void ShouldGetCustomBuildPathFromEnv()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", @"C:\Blah");

            var imaginaryPath = string.Format("foo{0}bar", Path.DirectorySeparatorChar);
            Environment.SetEnvironmentVariable("gauge_custom_build_path", imaginaryPath);
            var gaugeBinDir = Utils.GetGaugeBinDir();
            Assert.AreEqual(@"C:\Blah\foo\bar", gaugeBinDir);
        }
    }
}
