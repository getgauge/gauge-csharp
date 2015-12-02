using System.Collections.Generic;
using System.Linq;
using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Strategy;
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
            mockSandbox.Setup(sandbox => sandbox.TargetLibAssembly).Returns(typeof(Step).Assembly);

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
            var applicableHooks = new HooksStrategy().GetApplicableHooks(new List<string> {"foo"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(3, applicableHooks.Count);
            Assert.That(applicableHooks.Any(info => info.Name=="foo"), Is.True);
            Assert.That(applicableHooks.Any(info => info.Name=="baz"), Is.True);
        }

        [Test]
        public void ShouldFetchAllHooksWithSpecifiedTagsWhenDoingAnd()
        {
            var applicableHooks = new HooksStrategy().GetApplicableHooks(new List<string> {"bar"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(1, applicableHooks.Count);
        }

        [Test]
        public void ShouldFetchAnyHooksWithSpecifiedTagsWhenDoingOr()
        {
            var applicableHooks = new HooksStrategy().GetApplicableHooks(new List<string> {"baz"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(2, applicableHooks.Count);
            Assert.That(applicableHooks.Any(info => info.Name=="baz"), Is.True);
        }

        [Test]
        public void ShouldFetchAHooksWithSpecifiedTagsWhenDoingAnd()
        {
            var applicableHooks = new HooksStrategy().GetApplicableHooks(new List<string> {"baz", "bar"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(3, applicableHooks.Count);
            Assert.That(applicableHooks.Any(info => info.Name=="bar"), Is.True);
            Assert.That(applicableHooks.Any(info => info.Name=="baz"), Is.True);
        }

        [Test]
        public void ShouldFetchAHooksWithSpecifiedTagsWhenDoingOr()
        {
            var applicableHooks = new HooksStrategy().GetApplicableHooks(new List<string> {"baz", "foo"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(3, applicableHooks.Count);
            Assert.That(applicableHooks.Any(info => info.Name=="baz"), Is.True);
            Assert.That(applicableHooks.Any(info => info.Name=="foo"), Is.True);
        }

        [Test]
        public void ShouldNotFetchAnyFilteredHooksWhenTagsAreASuperSet()
        {
            var applicableHooks = new HooksStrategy().GetApplicableHooks(new List<string> {"bar",  "blah"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            // The blah hook is still called before step.
            Assert.AreEqual(1, applicableHooks.Count);
        }
    }
}
