using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using NuGet;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    [TestFixture]
    internal class SetupCommandTests
    {
        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", Directory.GetCurrentDirectory());
            _packageRepositoryFactory = new Mock<IPackageRepositoryFactory>();
            var packageRepository = new Mock<IPackageRepository>();
            var package = new Mock<IPackage>();
            package.Setup(p => p.Id).Returns("Gauge.CSharp.Lib");
            var list = new List<IPackage> {package.Object};
            package.Setup(p => p.Version).Returns(new SemanticVersion(Version));
            packageRepository.Setup(repository => repository.GetPackages()).Returns(list.AsQueryable());
            _packageRepositoryFactory.Setup(factory => factory.CreateRepository(SetupCommand.NugetEndpoint))
                .Returns(packageRepository.Object);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }

        private const string Version = "0.5.2";
        private Mock<IPackageRepositoryFactory> _packageRepositoryFactory;

        [Test]
        public void ShouldFetchMaxLibVersionOnlyOnce()
        {
            var setupCommand = new SetupCommand(_packageRepositoryFactory.Object);
            var maxLibVersion = setupCommand.MaxLibVersion;

            maxLibVersion = setupCommand.MaxLibVersion; // call again, just for fun!

            Assert.AreEqual(Version, maxLibVersion.ToString());
            _packageRepositoryFactory.Verify(factory => factory.CreateRepository(SetupCommand.NugetEndpoint),
                Times.Once);
        }
    }
}