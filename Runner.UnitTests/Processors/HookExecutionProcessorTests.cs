using System.Collections.Generic;
using System.Linq;
using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Processors;
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
            _hookMethods = new List<HookMethod>
            {
                new HookMethod(GetType().GetMethod("foo")),
                new HookMethod(GetType().GetMethod("bar")),
                new HookMethod(GetType().GetMethod("baz")),
                new HookMethod(GetType().GetMethod("blah"))
            };
        }
        [Test]
        public void ShouldFetchAllHooksWhenNoTagsSpecified()
        {
            var applicableHooks = HookExecutionProcessor.GetApplicableHooks(new List<string>(), _hookMethods);

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(1, applicableHooks.Count());
        }

        [Test]
        public void ShouldFetchAllHooksWithSpecifiedTags()
        {
            var applicableHooks = HookExecutionProcessor.GetApplicableHooks(new List<string> {"foo"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(3, applicableHooks.Count());
            Assert.That(applicableHooks.Any(info => info.Name=="foo"), Is.True);
            Assert.That(applicableHooks.Any(info => info.Name=="baz"), Is.True);
        }

        [Test]
        public void ShouldFetchAllHooksWithSpecifiedTagsWhenDoingAnd()
        {
            var applicableHooks = HookExecutionProcessor.GetApplicableHooks(new List<string> {"bar"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(1, applicableHooks.Count());
        }

        [Test]
        public void ShouldFetchAnyHooksWithSpecifiedTagsWhenDoingOr()
        {
            var applicableHooks = HookExecutionProcessor.GetApplicableHooks(new List<string> {"baz"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(2, applicableHooks.Count());
            Assert.That(applicableHooks.Any(info => info.Name=="baz"), Is.True);
        }

        [Test]
        public void ShouldFetchAHooksWithSpecifiedTagsWhenDoingAnd()
        {
            var applicableHooks = HookExecutionProcessor.GetApplicableHooks(new List<string> {"baz", "bar"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(3, applicableHooks.Count());
            Assert.That(applicableHooks.Any(info => info.Name=="bar"), Is.True);
            Assert.That(applicableHooks.Any(info => info.Name=="baz"), Is.True);
        }

        [Test]
        public void ShouldFetchAHooksWithSpecifiedTagsWhenDoingOr()
        {
            var applicableHooks = HookExecutionProcessor.GetApplicableHooks(new List<string> {"baz", "foo"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(3, applicableHooks.Count());
            Assert.That(applicableHooks.Any(info => info.Name=="baz"), Is.True);
            Assert.That(applicableHooks.Any(info => info.Name=="foo"), Is.True);
        }

        [Test]
        public void ShouldNotFetchAnyFilteredHooksWhenTagsAreASuperSet()
        {
            var applicableHooks = HookExecutionProcessor.GetApplicableHooks(new List<string> {"bar",  "blah"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            // The blah hook is still called before step.
            Assert.AreEqual(1, applicableHooks.Count());
        }
    }
}
