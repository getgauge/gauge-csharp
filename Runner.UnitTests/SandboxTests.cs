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
using System.IO;
using System.Reflection;
using System.Text;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Runner.Models;
using Gauge.CSharp.Runner.Wrappers;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    public class SandboxTests
    {
        public class TestAssembly : Assembly { }

        [Test]
        public void ShouldLoadAppConfigFromTargetLocation()
        {
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockAssembly = new Mock<TestAssembly>();
            var someTmpLocationDll = Path.GetFullPath("/some/tmp/location.dll");
            mockAssembly.Setup(assembly => assembly.Location).Returns(someTmpLocationDll);
            mockAssemblyLoader.Setup(loader => loader.AssembliesReferencingGaugeLib).Returns(new List<Assembly> { mockAssembly.Object });
            mockAssemblyLoader.Setup(loader => loader.ScreengrabberTypes).Returns(new List<Type>());
            mockAssemblyLoader.Setup(loader => loader.ClassInstanceManagerTypes).Returns(new List<Type> {typeof(DefaultClassInstanceManager)});
            mockAssemblyLoader.Setup(loader => loader.GetTargetLibAssembly()).Returns(GetType().Assembly);
            var mockHookRegistry = new Mock<IHookRegistry>();
            var mockFileWrapper = new Mock<IFileWrapper>();
            var expectedConfigLocation = string.Format("{0}.config", someTmpLocationDll);
            mockFileWrapper.Setup(wrapper => wrapper.Exists(expectedConfigLocation)).Returns(true);

            new Sandbox(mockAssemblyLoader.Object, mockHookRegistry.Object, mockFileWrapper.Object);

            Assert.AreEqual(expectedConfigLocation, AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
        }

        [Test]
        public void ShouldLoadScreenGrabber()
        {
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockAssembly = new Mock<TestAssembly>();
            mockAssemblyLoader.Setup(loader => loader.AssembliesReferencingGaugeLib).Returns(new List<Assembly> { mockAssembly.Object });
            mockAssemblyLoader.Setup(loader => loader.ScreengrabberTypes).Returns(new List<Type> { typeof(TestScreenGrabber)});
            mockAssemblyLoader.Setup(loader => loader.ClassInstanceManagerTypes).Returns(new List<Type> { typeof(DefaultClassInstanceManager) });
            mockAssemblyLoader.Setup(loader => loader.GetTargetLibAssembly()).Returns(GetType().Assembly);
            var mockHookRegistry = new Mock<IHookRegistry>();
            var mockFileWrapper = new Mock<IFileWrapper>();

            var sandbox = new Sandbox(mockAssemblyLoader.Object, mockHookRegistry.Object, mockFileWrapper.Object);
            byte[] screenshot;
            var tryScreenCapture = sandbox.TryScreenCapture(out screenshot);

            Assert.IsTrue(tryScreenCapture);
            Assert.AreEqual("TestScreenGrabber", Encoding.UTF8.GetString(screenshot));
        }

        [Test]
        public void ShouldLoadClassInstanceManager()
        {
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var assemblyLoaded = false;
            var mockAssembly = new Mock<TestAssembly>();
            mockAssembly.Setup(assembly => assembly.FullName).Callback(() => assemblyLoaded = true);
            mockAssemblyLoader.Setup(loader => loader.AssembliesReferencingGaugeLib).Returns(new List<Assembly> { mockAssembly.Object });
            mockAssemblyLoader.Setup(loader => loader.ScreengrabberTypes).Returns(new List<Type> { typeof(DefaultScreenGrabber)});
            mockAssemblyLoader.Setup(loader => loader.ClassInstanceManagerTypes).Returns(new List<Type> { typeof(TestClassInstanceManager) });
            mockAssemblyLoader.Setup(loader => loader.GetTargetLibAssembly()).Returns(GetType().Assembly);
            var mockHookRegistry = new Mock<IHookRegistry>();
            var mockFileWrapper = new Mock<IFileWrapper>();

            var sandbox = new Sandbox(mockAssemblyLoader.Object, mockHookRegistry.Object, mockFileWrapper.Object);

            Assert.IsTrue(assemblyLoaded, "Mock Assembly was not initialized by TestClassInstanceManager");
        }

        public class TestClassInstanceManager : IClassInstanceManager
        {
            public void Initialize(List<Assembly> assemblies)
            {
                foreach (var assembly in assemblies)
                {
                    var fullName = assembly.FullName;
                }
            }

            public object Get(Type declaringType)
            {
                throw new NotImplementedException();
            }

            public void StartScope(string tag)
            {
                throw new NotImplementedException();
            }

            public void CloseScope()
            {
                throw new NotImplementedException();
            }

            public void ClearCache()
            {
                throw new NotImplementedException();
            }
        }

        private class TestScreenGrabber : IScreenGrabber {
            public byte[] TakeScreenShot()
            {
                return Encoding.UTF8.GetBytes("TestScreenGrabber");
            }
        }
    }
}
