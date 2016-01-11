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
using Gauge.CSharp.Runner.Processors;
using Gauge.CSharp.Runner.Strategy;
using Gauge.CSharp.Runner.UnitTests.Processors.Stubs;
using Gauge.Messages;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    [TestFixture]
    public class HookExecutionProcessorTests
    {
        [BeforeScenario("foo")]
        public void foo()
        {
        }

        [BeforeScenario("bar", "baz")]
        public void bar()
        {
        }

        [BeforeScenario("foo", "baz")]
        [TagAggregationBehaviour(TagAggregation.Or)]
        public void baz()
        {
        }

        [BeforeScenario]
        public void blah()
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
         */

        private List<HookMethod> _hookMethods;

        [SetUp]
        public void Setup()
        {
            var mockSandbox = new Mock<ISandbox>();
            mockSandbox.Setup(sandbox => sandbox.TargetLibAssembly).Returns(typeof (Step).Assembly);

            _hookMethods = new List<HookMethod>
            {
                new HookMethod(GetType().GetMethod("foo"), mockSandbox.Object),
                new HookMethod(GetType().GetMethod("bar"), mockSandbox.Object),
                new HookMethod(GetType().GetMethod("baz"), mockSandbox.Object),
                new HookMethod(GetType().GetMethod("blah"), mockSandbox.Object)
            };
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
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"foo"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(2, applicableHooks.Count);
            AssertEx.ContainsMethods(applicableHooks, "baz", "foo");
        }

        [Test]
        public void ShouldFetchAllHooksWithSpecifiedTagsWhenDoingAnd()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"bar"}, _hookMethods);

            Assert.IsNotNull(applicableHooks);
            Assert.IsEmpty(applicableHooks);
        }

        [Test]
        public void ShouldFetchAnyHooksWithSpecifiedTagsWhenDoingOr()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"baz"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(1, applicableHooks.Count);
            AssertEx.ContainsMethods(applicableHooks, "baz");
        }

        [Test]
        public void ShouldFetchAHooksWithSpecifiedTagsWhenDoingAnd()
        {
            var applicableHooks =
                new HooksStrategy().GetTaggedHooks(new List<string> {"baz", "bar"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(2, applicableHooks.Count);
            AssertEx.ContainsMethods(applicableHooks, "baz", "bar");
        }

        [Test]
        public void ShouldFetchAHooksWithSpecifiedTagsWhenDoingOr()
        {
            var applicableHooks =
                new HooksStrategy().GetTaggedHooks(new List<string> {"baz", "foo"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(2, applicableHooks.Count);
            AssertEx.ContainsMethods(applicableHooks, "baz", "foo");
        }

        [Test]
        public void ShouldNotFetchAnyTaggedHooksWhenTagsAreASuperSet()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"bar", "blah"}, _hookMethods);

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
        public void ShouldUseUntaggedHooksFirstStrategy()
        {
            var hooksStrategy = new TestUntaggedHooksFirstExecutionProcessor().GetHooksStrategy();

            Assert.IsInstanceOf<UntaggedHooksFirstStrategy>(hooksStrategy);
        }

        [Test]
        public void ShouldUseTaggedHooksFirstStrategy()
        {
            var hooksStrategy = new TestTaggedHooksFirstExecutionProcessor().GetHooksStrategy();

            Assert.IsInstanceOf<TaggedHooksFirstStrategy>(hooksStrategy);
        }

        [Test]
        public void ShouldSetReadMessageToFalseByDefault()
        {
            Assert.False(new TestHooksExecutionProcessor().ShouldReadMessages());
        }
    }
}
