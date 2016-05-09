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

using Gauge.CSharp.Runner.Extensions;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    [TestFixture]
    public class CSharpIdentifierTests
    {
        [TestCase("With Spaces", "WithSpaces", true)]
        [TestCase("Special*chars%!", "SpecialChars", true)]
        [TestCase("   begins with whitespace", "BeginsWithWhitespace", true)]
        [TestCase("ends with whitespace   ", "EndsWithWhitespace", true)]
        [TestCase("class", "Class", true)]
        [TestCase("class", "@class", false)]
        [TestCase("int", "Int", true)]
        [TestCase("abstract", "Abstract", true)]
        [TestCase("foo", "foo", true)]
        [TestCase("foo", "foo", false)]
        [Test]
        public void GeneratesValidIdentifiers(string input, string expected, bool camelCase)
        {
            Assert.AreEqual(expected, input.ToValidCSharpIdentifier(camelCase));
        }
    }
}
