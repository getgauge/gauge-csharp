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
using System.IO;
using System.Linq;
using Gauge.CSharp.Core;
using Gauge.CSharp.Runner.Wrappers;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    [TestFixture]
    internal class AssemblyLocaterTests
    {
        private readonly Mock<IDirectoryWrapper> _mockDirectoryWrapper = new Mock<IDirectoryWrapper>();
        private readonly Mock<IFileWrapper> _mockFileWrapper = new Mock<IFileWrapper>();

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", Directory.GetCurrentDirectory());
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_ADDITIONAL_LIBS", null);
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }

        [Test]
        public void ShouldGetAssembliesFromGaugeBin()
        {
            var expectedAssemblies = new[] {"fooAssemblyLocation", "barAssemblyLocation"};
            _mockDirectoryWrapper.Setup(wrapper => wrapper.EnumerateFiles(Utils.GetGaugeBinDir(), "*.dll", SearchOption.TopDirectoryOnly))
                .Returns(expectedAssemblies);
            var assemblyLocater = new AssemblyLocater(_mockDirectoryWrapper.Object, _mockFileWrapper.Object);
            
            var assemblies = assemblyLocater.GetAllAssemblies();
            
            Assert.AreEqual(expectedAssemblies, assemblies);
        }
        
        [Test]
        public void ShouldGetAssembliesFromGaugeAdditionalLibsEnvVar()
        {
            Environment.SetEnvironmentVariable("GAUGE_ADDITIONAL_LIBS", "foo/");
            var expectedAssemblies = new[] { "fooAssemblyLocation", "barAssemblyLocation" };
            _mockDirectoryWrapper.Setup(wrapper => wrapper.Exists(Path.GetFullPath("foo/"))).Returns(true);
            _mockDirectoryWrapper.Setup(wrapper => wrapper.EnumerateFiles(Utils.GetGaugeBinDir(), "*.dll", SearchOption.TopDirectoryOnly))
                .Returns(Enumerable.Empty<string>());
            _mockDirectoryWrapper.Setup(wrapper => wrapper.EnumerateFiles(Path.GetFullPath("foo/"), "*.dll", SearchOption.TopDirectoryOnly))
                .Returns(expectedAssemblies);

            var assemblyLocater = new AssemblyLocater(_mockDirectoryWrapper.Object, _mockFileWrapper.Object);

            var assemblies = assemblyLocater.GetAllAssemblies();
            Assert.AreEqual(expectedAssemblies, assemblies);
        }

        [Test]
        public void ShouldAddAssembliyFromGaugeAdditionalLibFile()
        {
            Environment.SetEnvironmentVariable("GAUGE_ADDITIONAL_LIBS", "foo.dll");
            var expectedAssemblies = new[] { Path.GetFullPath("foo.dll") };
            _mockDirectoryWrapper.Setup(wrapper => wrapper.EnumerateFiles(Utils.GetGaugeBinDir(), "*.dll", SearchOption.TopDirectoryOnly))
                .Returns(Enumerable.Empty<string>());
            _mockFileWrapper.Setup(wrapper => wrapper.Exists(Path.GetFullPath("foo.dll"))).Returns(true);
            var assemblyLocater = new AssemblyLocater(_mockDirectoryWrapper.Object, _mockFileWrapper.Object);

            var assemblies = assemblyLocater.GetAllAssemblies();
            Assert.AreEqual(expectedAssemblies, assemblies);
        }

        [Test]
        public void ShouldAddAssembliesFromMultipleLocations()
        {
            Environment.SetEnvironmentVariable("GAUGE_ADDITIONAL_LIBS", "foo.dll, foo/");
            var expectedAssemblies = new[] { Path.GetFullPath("foo.dll"), "fooAssemblyLocation", "barAssemblyLocation" };
            _mockDirectoryWrapper.Setup(wrapper => wrapper.Exists(Path.GetFullPath("foo/"))).Returns(true);
            _mockDirectoryWrapper.Setup(wrapper => wrapper.EnumerateFiles(Utils.GetGaugeBinDir(), "*.dll", SearchOption.TopDirectoryOnly))
                .Returns(Enumerable.Empty<string>());
            _mockDirectoryWrapper.Setup(wrapper => wrapper.EnumerateFiles(Path.GetFullPath("foo/"), "*.dll", SearchOption.TopDirectoryOnly))
                .Returns(new []{"fooAssemblyLocation", "barAssemblyLocation"});
            _mockFileWrapper.Setup(wrapper => wrapper.Exists(Path.GetFullPath("foo.dll"))).Returns(true);
            var assemblyLocater = new AssemblyLocater(_mockDirectoryWrapper.Object, _mockFileWrapper.Object);

            var assemblies = assemblyLocater.GetAllAssemblies();

            Assert.AreEqual(expectedAssemblies, assemblies);
        }

        [Test]
        public void ShoulNotdAddAssembliesFromInvalidDirectory()
        {
            Environment.SetEnvironmentVariable("GAUGE_ADDITIONAL_LIBS", "foo/");
            _mockDirectoryWrapper.Setup(wrapper => wrapper.Exists(Path.GetFullPath("foo/"))).Returns(false);
            _mockDirectoryWrapper.Setup(wrapper => wrapper.EnumerateFiles(Utils.GetGaugeBinDir(), "*.dll", SearchOption.TopDirectoryOnly))
                .Returns(Enumerable.Empty<string>());

            var assemblyLocater = new AssemblyLocater(_mockDirectoryWrapper.Object, _mockFileWrapper.Object);

            var assemblies = assemblyLocater.GetAllAssemblies();
            Assert.IsEmpty(assemblies);
        }

        [Test]
        public void ShouldNotAddAssembliesFromInvalidFile()
        {
            Environment.SetEnvironmentVariable("GAUGE_ADDITIONAL_LIBS", "foo.dll");
            _mockFileWrapper.Setup(wrapper => wrapper.Exists(Path.GetFullPath("foo.dll"))).Returns(false);
            _mockDirectoryWrapper.Setup(wrapper => wrapper.EnumerateFiles(Utils.GetGaugeBinDir(), "*.dll", SearchOption.TopDirectoryOnly))
                .Returns(Enumerable.Empty<string>());

            var assemblyLocater = new AssemblyLocater(_mockDirectoryWrapper.Object, _mockFileWrapper.Object);

            var assemblies = assemblyLocater.GetAllAssemblies();
            Assert.IsEmpty(assemblies);
        }
    }
}
