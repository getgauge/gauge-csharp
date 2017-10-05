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
using Gauge.CSharp.Runner.Extensions;
using Gauge.CSharp.Runner.Models;
using Gauge.CSharp.Runner.Strategy;
using Gauge.CSharp.Runner.UnitTests.Processors.Stubs;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    [TestFixture]
    public class HookExecutionProcessorTests
    {
        [SetUp]
        public void Setup()
        {
            _hookMethods = new List<IHookMethod>
            {
                new HookMethod("BeforeScenario", GetType().GetMethod("Foo"), typeof(Step).Assembly),
                new HookMethod("BeforeScenario", GetType().GetMethod("Bar"), typeof(Step).Assembly),
                new HookMethod("BeforeScenario", GetType().GetMethod("Baz"), typeof(Step).Assembly),
                new HookMethod("BeforeScenario", GetType().GetMethod("Blah"), typeof(Step).Assembly)
            };
        }

        [BeforeScenario("Foo")]
        public void Foo()
        {
        }

        [BeforeScenario("Bar", "Baz")]
        public void Bar()
        {
        }

        [BeforeScenario("Foo", "Baz")]
        [TagAggregationBehaviour(TagAggregation.Or)]
        public void Baz()
        {
        }

        [BeforeScenario]
        public void Blah()
        {
        }

        [BeforeSpec]
        [BeforeScenario]
        public void MultiHook()
        {
        }

        /*
         * untagged hooks are executed for all.
         * Tags     | Methods
         * Foo      | Foo, Baz
         * Bar      | NONE
         * Baz      | Baz
         * Bar, Baz | Bar, Baz
         * Foo, Baz | Baz
         */

        private IList<IHookMethod> _hookMethods;

        [Test]
        public void ShouldAllowMultipleHooksInaMethod()
        {
            var expected = GetType().GetMethod("MultiHook").FullyQuallifiedName();
            var beforeScenarioHook =
                new HookMethod("BeforeScenario", GetType().GetMethod("MultiHook"), typeof(Step).Assembly);
            Assert.AreEqual(expected, beforeScenarioHook.Method);

            var beforeSpecHook = new HookMethod("BeforeSpec", GetType().GetMethod("MultiHook"), typeof(Step).Assembly);
            Assert.AreEqual(expected, beforeSpecHook.Method);
        }

        [Test]
        public void ShouldFetchAHooksWithSpecifiedTagsWhenDoingAnd()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"Baz", "Bar"}, _hookMethods)
                .ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(2, applicableHooks.Count);
            Assert.Contains(GetType().GetMethod("Bar").FullyQuallifiedName(), applicableHooks);
            Assert.Contains(GetType().GetMethod("Baz").FullyQuallifiedName(), applicableHooks);
        }

        [Test]
        public void ShouldFetchAHooksWithSpecifiedTagsWhenDoingOr()
        {
            var applicableHooks =
                new HooksStrategy().GetTaggedHooks(new List<string> {"Baz", "Foo"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(2, applicableHooks.Count);
            Assert.Contains(GetType().GetMethod("Foo").FullyQuallifiedName(), applicableHooks);
            Assert.Contains(GetType().GetMethod("Baz").FullyQuallifiedName(), applicableHooks);
        }

        [Test]
        public void ShouldFetchAllHooksWhenNoTagsSpecified()
        {
            var applicableHooks = new HooksStrategy().GetApplicableHooks(new List<string>(), _hookMethods);

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(1, applicableHooks.Count());
        }

        [Test]
        public void ShouldFetchAllHooksWithSpecifiedTags()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"Foo"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(2, applicableHooks.Count);
            Assert.Contains(GetType().GetMethod("Foo").FullyQuallifiedName(), applicableHooks);
            Assert.Contains(GetType().GetMethod("Baz").FullyQuallifiedName(), applicableHooks);
        }

        [Test]
        public void ShouldFetchAllHooksWithSpecifiedTagsWhenDoingAnd()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"Bar"}, _hookMethods);

            Assert.IsNotNull(applicableHooks);
            Assert.IsEmpty(applicableHooks);
        }

        [Test]
        public void ShouldFetchAnyHooksWithSpecifiedTagsWhenDoingOr()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"Baz"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(1, applicableHooks.Count);
            Assert.Contains(GetType().GetMethod("Baz").FullyQuallifiedName(), applicableHooks);
        }

        [Test]
        public void ShouldNotFetchAnyTaggedHooksWhenTagsAreASuperSet()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"Bar", "Blah"}, _hookMethods);

            Assert.IsNotNull(applicableHooks);
            Assert.IsEmpty(applicableHooks);
        }

        [Test]
        public void ShouldUseDefaultHooksStrategy()
        {
            var hooksStrategy = new TestHooksExecutionProcessor().GetHooksStrategy();

            Assert.IsInstanceOf<HooksStrategy>(hooksStrategy);
        }

        [Test]
        public void ShouldUseTaggedHooksFirstStrategy()
        {
            var hooksStrategy = new TestTaggedHooksFirstExecutionProcessor().GetHooksStrategy();

            Assert.IsInstanceOf<TaggedHooksFirstStrategy>(hooksStrategy);
        }

        [Test]
        public void ShouldUseUntaggedHooksFirstStrategy()
        {
            var hooksStrategy = new TestUntaggedHooksFirstExecutionProcessor().GetHooksStrategy();

            Assert.IsInstanceOf<UntaggedHooksFirstStrategy>(hooksStrategy);
        }
    }
}