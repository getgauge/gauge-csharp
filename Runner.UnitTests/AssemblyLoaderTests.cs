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
using System.Reflection;
using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Wrappers;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    [TestFixture]
    public class AssemblyLoaderTests
    {
        public class TestAssembly : Assembly { }

        [Step("Foo text")]
        public void DummyStepMethod() { }

        private Mock<TestAssembly> _mockAssembly;
        private MethodInfo _stepMethod;
        private AssemblyLoader _assemblyLoader;
        private Mock<IAssemblyWrapper> _mockAssemblyWrapper;

        [SetUp]
        public void Setup()
        {
            const string tmpLocation = "/tmp/location";
            var libPath = Path.GetFullPath(Path.Combine(tmpLocation, "gauge-bin", "Gauge.CSharp.Lib.dll"));
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", tmpLocation);
            var thisType = GetType();
            var assemblyLocation = thisType.Assembly.Location;
            _mockAssemblyWrapper = new Mock<IAssemblyWrapper>();
            var fileWrapper = new Mock<IFileWrapper>();
            _stepMethod = thisType.GetMethod("DummyStepMethod");
            _mockAssembly = new Mock<TestAssembly>();
            _mockAssembly.Setup(assembly => assembly.GetTypes()).Returns(new[] { thisType });
            _mockAssembly.Setup(assembly => assembly.GetType(thisType.FullName)).Returns(thisType);
            _mockAssembly.Setup(assembly => assembly.GetReferencedAssemblies()).Returns(new[] { new AssemblyName("Gauge.CSharp.Lib") });
            fileWrapper.Setup(wrapper => wrapper.Exists(libPath)).Returns(true);
            _mockAssemblyWrapper.Setup(wrapper => wrapper.LoadFrom(libPath)).Returns(typeof(Step).Assembly).Verifiable();
            _mockAssemblyWrapper.Setup(wrapper => wrapper.ReflectionOnlyLoadFrom(assemblyLocation)).Returns(_mockAssembly.Object);
            _mockAssemblyWrapper.Setup(wrapper => wrapper.LoadFrom(assemblyLocation)).Returns(_mockAssembly.Object);
            _assemblyLoader = new AssemblyLoader(_mockAssemblyWrapper.Object, fileWrapper.Object, new[] { assemblyLocation });
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }

        [Test]
        public void ShouldThrowExceptionWhenLibAssemblyNotFound()
        {
            const string tmpLocation = "/tmp/location";
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", tmpLocation);
            var mockAssemblyWrapper = new Mock<IAssemblyWrapper>();
            var fileWrapper = new Mock<IFileWrapper>();

            Assert.Throws<FileNotFoundException>(() => new AssemblyLoader(mockAssemblyWrapper.Object, fileWrapper.Object, new[] { tmpLocation }));
        }

        [Test]
        public void ShouldGetTargetAssembly()
        {
            _mockAssemblyWrapper.VerifyAll();
        }

        [Test]
        public void ShouldGetAssemblyReferencingGaugeLib()
        {
            Assert.Contains(_mockAssembly.Object, _assemblyLoader.AssembliesReferencingGaugeLib);
        }

        [Test]
        public void ShouldGetMethodsForGaugeAttribute()
        {
            Assert.Contains(_stepMethod, _assemblyLoader.GetMethods(typeof(Step)));
        }
    }
}
