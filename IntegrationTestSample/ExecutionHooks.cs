using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gauge.CSharp.Lib.Attribute;

namespace IntegrationTestSample
{
    class ExecutionHooks
    {
        [BeforeSuite]
        public void BeforeSuite() { }

        [AfterSuite]
        public void AfterSuite() { }

        [BeforeStep]
        public void BeforeStep() { }

        [AfterStep]
        public void AfterStep() { }

        [BeforeSpec]
        public void BeforeSpec() { }

        [AfterSpec]
        public void AfterSpec() { }

        [BeforeScenario]
        public void BeforeScenario() { }

        [AfterScenario]
        public void AfterScenario() { }
    }
}
