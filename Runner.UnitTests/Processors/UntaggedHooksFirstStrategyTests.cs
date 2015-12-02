using System;
using System.Collections.Generic;
using System.Linq;
using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Processors;
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
        public void ShouldFetchUntaggedHooksAfterTaggedHooks()
        {
            var applicableHooks = new UntaggedHooksFirstStrategy().GetApplicableHooks(new List<string> {"foo"}, _hookMethods).ToList();

            var untaggedHookNames = new[] {applicableHooks[3], applicableHooks[2]}.Select(info => info.Name).ToArray();

            Assert.Contains("blah",untaggedHookNames);
            Assert.Contains("zed", untaggedHookNames);
        }
    }
}