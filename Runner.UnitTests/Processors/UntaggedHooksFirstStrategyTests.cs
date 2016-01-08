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
using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Strategy;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    [TestFixture]
    public class UntaggedHooksFirstStrategyTests
    {
        [AfterScenario("foo")]
        public void foo()
        {
        }

        [AfterScenario("bar", "baz")]
        public void bar()
        {
        }

        [AfterScenario("foo", "baz")]
        [TagAggregationBehaviour(TagAggregation.Or)]
        public void baz()
        {
        }

        [AfterScenario]
        public void blah()
        {
        }

        [AfterScenario]
        public void zed()
        {
        }

        /*
         * untagged hooks are executed for all.
         * Tags     | Methods
         * foo      | foo, baz
         * bar      | NONE
         * baz      | baz
         * bar, baz | bar, baz
         * foo, baz | baz
         * After hooks should execute tagged hooks prior to untagged
         */
        private HashSet<HookMethod> _hookMethods;

        [SetUp]
        public void Setup()
        {
            var mockSandbox = new Mock<ISandbox>();
            mockSandbox.Setup(sandbox => sandbox.TargetLibAssembly).Returns(typeof (Step).Assembly);

            _hookMethods = new HashSet<HookMethod>
            {
                new HookMethod(GetType().GetMethod("foo"), mockSandbox.Object),
                new HookMethod(GetType().GetMethod("bar"), mockSandbox.Object),
                new HookMethod(GetType().GetMethod("zed"), mockSandbox.Object),
                new HookMethod(GetType().GetMethod("blah"), mockSandbox.Object),
                new HookMethod(GetType().GetMethod("baz"), mockSandbox.Object)
            };
        }

        [Test]
        public void ShouldFetchTaggedHooksAfterUntaggedHooks()
        {
            var applicableHooks = new UntaggedHooksFirstStrategy().GetApplicableHooks(new List<string> {"foo"}, _hookMethods).ToList();

            AssertEx.ContainsMethods(new[] { applicableHooks[0], applicableHooks[1] }, "blah", "zed");
        }

        [Test]
        public void ShouldFetchTaggedHooksInSortedOrder()
        {
            var applicableHooks = new UntaggedHooksFirstStrategy().GetApplicableHooks(new List<string> {"foo"}, _hookMethods).ToList();

            Assert.That(applicableHooks[0].Name, Is.EqualTo("blah"));
            Assert.That(applicableHooks[1].Name, Is.EqualTo("zed"));
        }


        [Test]
        public void ShouldFetchUntaggedHooksInSortedOrder()
        {
            var applicableHooks = new UntaggedHooksFirstStrategy().GetApplicableHooks(new List<string> { "foo" }, _hookMethods).ToList();

            Assert.That(applicableHooks[2].Name, Is.EqualTo("baz"));
            Assert.That(applicableHooks[3].Name, Is.EqualTo("foo"));
        }
    }
}