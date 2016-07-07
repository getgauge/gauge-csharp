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
using System.Linq;
using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Extensions;
using Gauge.CSharp.Runner.Models;
using Gauge.CSharp.Runner.Strategy;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    [TestFixture]
    public class TaggedHooksFirstStrategyTests
    {
        [AfterScenario("Foo")]
        public void Foo()
        {
        }

        [AfterScenario("Bar", "Baz")]
        public void Bar()
        {
        }

        [AfterScenario("Foo", "Baz")]
        [TagAggregationBehaviour(TagAggregation.Or)]
        public void Baz()
        {
        }

        [AfterScenario]
        public void Blah()
        {
        }

        [AfterScenario]
        public void Zed()
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
         * After hooks should execute tagged hooks prior to untagged
         */
        private HashSet<HookMethod> _hookMethods;

        [SetUp]
        public void Setup()
        {
            _hookMethods = new HashSet<HookMethod>
            {
                new HookMethod(GetType().GetMethod("Foo"), typeof(Step).Assembly),
                new HookMethod(GetType().GetMethod("Bar"), typeof(Step).Assembly),
                new HookMethod(GetType().GetMethod("Zed"), typeof(Step).Assembly),
                new HookMethod(GetType().GetMethod("Blah"), typeof(Step).Assembly),
                new HookMethod(GetType().GetMethod("Baz"), typeof(Step).Assembly)
            };
        }

        [Test]
        public void ShouldFetchUntaggedHooksAfterTaggedHooks()
        {
            var taggedHooks = new [] {"Baz", "Foo"} ;
            var untaggedHooks = new [] {"Blah", "Zed"} ;
            var expected = taggedHooks.Concat(untaggedHooks).Select(s => GetType().GetMethod(s).FullyQuallifiedName());

            var applicableHooks = new TaggedHooksFirstStrategy().GetApplicableHooks(new List<string> {"Foo"}, _hookMethods).ToList();

            Assert.AreEqual(expected, applicableHooks);
        }


        [Test]
        public void ShouldFetchTaggedHooksInSortedOrder()
        {
            var untaggedHooks = new[] { "Blah", "Zed" }.Select(s => GetType().GetMethod(s).FullyQuallifiedName());

            var applicableHooks = new TaggedHooksFirstStrategy().GetApplicableHooks(new List<string> { "Foo" }, _hookMethods).ToArray();
            var actual = new ArraySegment<string>(applicableHooks, 2, untaggedHooks.Count());

            Assert.AreEqual(untaggedHooks, actual);
        }


        [Test]
        public void ShouldFetchUntaggedHooksInSortedOrder()
        {
            var applicableHooks = new TaggedHooksFirstStrategy().GetApplicableHooks(new List<string> { "Foo" }, _hookMethods).ToList();

            Assert.That(applicableHooks[0], Is.EqualTo(GetType().GetMethod("Baz").FullyQuallifiedName()));
            Assert.That(applicableHooks[1], Is.EqualTo(GetType().GetMethod("Foo").FullyQuallifiedName()));
        }
    }
}