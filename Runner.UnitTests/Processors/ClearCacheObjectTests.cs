using System;
using Gauge.CSharp.Runner.Processors;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    public class ClearCacheObjectTests
    {
        [Test]
        public void ShouldNotGetMemoizedInstanceForTypeWhenClassObjectMapIsCleared()
        {
            var type = typeof(object);
            var instance = ClassInstanceManager.Get(type);
            ClassInstanceManager.ClearCache();
            var anotherInstance = ClassInstanceManager.Get(type);

            Assert.AreNotSame(instance, anotherInstance);
        }
    }
}