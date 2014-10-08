using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    [TestFixture]
    public class StepRegistryTests
    {
        public void Foo(){ }
        public void Bar(){ }

        [Test]
        public void ShouldGetAllSteps()
        {
            var methods = new[]
            {
                new KeyValuePair<string, MethodInfo>("Foo", GetType().GetMethod("Foo")),
                new KeyValuePair<string, MethodInfo>("Bar", GetType().GetMethod("Bar"))
            };
            var stepRegistry = new StepRegistry(methods);
            var allSteps = stepRegistry.AllSteps();

            Assert.AreEqual(allSteps.Count(), 2);
            Assert.True(allSteps.Contains("Foo"));
            Assert.True(allSteps.Contains("Bar"));
        }

        [Test]
        public void ShouldGetMethodForStep()
        {
            var methods = new[]
            {
                new KeyValuePair<string, MethodInfo>("Foo", GetType().GetMethod("Foo")),
                new KeyValuePair<string, MethodInfo>("Bar", GetType().GetMethod("Bar"))
            };
            var stepRegistry = new StepRegistry(methods);
            var method = stepRegistry.MethodFor("Foo");

            Assert.AreEqual(method.Name, "Foo");
        }

        [Test]
        public void ShouldContainMethodForStepDefined()
        {
            var methods = new[]
            {
                new KeyValuePair<string, MethodInfo>("Foo", GetType().GetMethod("Foo")),
                new KeyValuePair<string, MethodInfo>("Bar", GetType().GetMethod("Bar"))
            };
            var stepRegistry = new StepRegistry(methods);

            Assert.True(stepRegistry.ContainsStep("Foo"));
            Assert.True(stepRegistry.ContainsStep("Bar"));
        }
    }
}