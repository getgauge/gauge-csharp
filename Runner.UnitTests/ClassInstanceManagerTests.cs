using System;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    [TestFixture]
    public class ClassInstanceManagerTests
    {
        [Test]
        public void ShouldGetInstanceForType()
        {
            var type = typeof(object);
            var instance = ClassInstanceManager.Get(type);
            
            Assert.NotNull(instance);
            Assert.AreEqual(instance.GetType(), type);
        }

        [Test]
        public void ShouldGetMemoizedInstanceForType()
        {
            var type = typeof(object);
            var instance = ClassInstanceManager.Get(type);
            var anotherInstance = ClassInstanceManager.Get(type);
            
            Assert.AreSame(instance, anotherInstance);
        }
    }
}