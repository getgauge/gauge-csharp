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
            var stepRegistry = new StepRegistry(methods, null, null);
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
            var stepRegistry = new StepRegistry(methods, null, null);
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
            var stepRegistry = new StepRegistry(methods, null, null);

            Assert.True(stepRegistry.ContainsStep("Foo"));
            Assert.True(stepRegistry.ContainsStep("Bar"));
        }

        [Test]
        public void ShouldGetAliasWhenExists()
        {
            var methods = new[]
            {
                new KeyValuePair<string, MethodInfo>("Foo", GetType().GetMethod("Foo")),
                new KeyValuePair<string, MethodInfo>("FooAlias", GetType().GetMethod("Foo")),
                new KeyValuePair<string, MethodInfo>("Bar", GetType().GetMethod("Bar"))
            };
            var stepRegistry = new StepRegistry(methods, null, new Dictionary<string, bool> { { "Foo", true}, {"FooAlias", true} });

            Assert.True(stepRegistry.HasAlias("Foo"));
            Assert.True(stepRegistry.HasAlias("FooAlias"));
        }

        [Test]
        public void ShouldNotHaveAliasWhenSingleStepTextIsDefined()
        {
            var methods = new[]
            {
                new KeyValuePair<string, MethodInfo>("Foo", GetType().GetMethod("Foo")),
                new KeyValuePair<string, MethodInfo>("Bar", GetType().GetMethod("Bar"))
            };
            var stepRegistry = new StepRegistry(methods, null, new Dictionary<string, bool> ());

            Assert.False(stepRegistry.HasAlias("Foo"));
            Assert.False(stepRegistry.HasAlias("Bar"));
        }

        [Test]
        public void ShouldGetStepTextFromParameterizedStepText()
        {
            var methods = new[]
            {
                new KeyValuePair<string, MethodInfo>("Foo", GetType().GetMethod("Foo")),
                new KeyValuePair<string, MethodInfo>("Bar", GetType().GetMethod("Bar"))
            };
            var stepTextMap = new Dictionary<string, string> { {"foo_parameterized", "Foo"} };

            var stepRegistry = new StepRegistry(methods, stepTextMap, null);

            Assert.AreEqual(stepRegistry.GetStepText("foo_parameterized"), "Foo");
        }

        [Test]
        public void ShouldGetEmptyStepTextForInvalidParameterizedStepText()
        {
            var methods = new[]
            {
                new KeyValuePair<string, MethodInfo>("Foo", GetType().GetMethod("Foo")),
                new KeyValuePair<string, MethodInfo>("Bar", GetType().GetMethod("Bar"))
            };
            var stepTextMap = new Dictionary<string, string> { {"foo_parameterized", "Foo"} };

            var stepRegistry = new StepRegistry(methods, stepTextMap, null);

            Assert.AreEqual(stepRegistry.GetStepText("random"), string.Empty);
        }
    }
}