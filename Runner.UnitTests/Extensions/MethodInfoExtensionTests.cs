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

using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Extensions;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Extensions
{
    internal class MethodInfoExtensionTests
    {
        [Step("Foo")]
        public void Foo()
        {
        }

        [Step("Bar")]
        [ContinueOnFailure]
        public void Bar(string bar)
        {
        }


        [ContinueOnFailure]
        public void Baz(string bar)
        {
        }

        [Test]
        [TestCase("Foo", "Gauge.CSharp.Runner.UnitTests.Extensions.MethodInfoExtensionTests.Foo")]
        [TestCase("Bar", "Gauge.CSharp.Runner.UnitTests.Extensions.MethodInfoExtensionTests.Bar-Stringbar")]
        public void ShouldGetFullyQualifiedName(string methodName, string expectedMethodId)
        {
            Assert.AreEqual(expectedMethodId, GetType().GetMethod(methodName).FullyQuallifiedName());
        }

        [Test]
        [TestCase("Foo", false)]
        [TestCase("Bar", true)]
        [TestCase("Baz", false, Description = "Recoverable is true only when method is a Step")]
        public void ShouldGetRecoverable(string methodName, bool expectedRecoverable)
        {
            Assert.AreEqual(expectedRecoverable, GetType().GetMethod(methodName).IsRecoverableStep());
        }
    }
}