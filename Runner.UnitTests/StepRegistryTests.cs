// Copyright 2015 ThoughtWorks, Inc.

// This file is part of Gauge-CSharp.

// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

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