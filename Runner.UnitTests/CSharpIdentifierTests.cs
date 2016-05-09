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
        [TestCase("With Spaces", "WithSpaces")]
        [TestCase("Special*chars%!", "SpecialChars")]
        [TestCase("   begins with whitespace", "BeginsWithWhitespace")]
        [TestCase("ends with whitespace   ", "EndsWithWhitespace")]
        [TestCase("class", "Class")]
        [TestCase("int", "Int")]
        [TestCase("abstract", "Abstract")]
        [TestCase("foo", "foo")]
        [Test]
        public void GeneratesValidIdentifiers(string input, string expected)
        {
            Assert.AreEqual(expected, input.ToValidCSharpIdentifier());
        }
    }
}
