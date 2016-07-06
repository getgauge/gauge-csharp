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

using System.Linq;
using Gauge.CSharp.Runner.Converters;
using Gauge.Messages;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Converter
{
    [TestFixture]
    public class StringParamConverterTests
    {
        private class TestTypeConversion
        {
            public void Int(int i)
            {
            }

            public void Float(float j)
            {
            }

            public void Bool(bool b)
            {
            }

            public void String(string s)
            {
            }
        }

        [Test]
        public void ShouldConvertFromParameterToString()
        {
            const string expected = "Foo";
            var parameter = new Parameter.Builder().SetParameterType(Parameter.Types.ParameterType.Static).SetValue(expected).Build();
            
            var actual = new StringParamConverter().Convert(parameter);
            
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ShouldTryToConvertStringParameterToInt()
        {
            var type = new TestTypeConversion().GetType();
            var method = type.GetMethod("Int");

            var getParams = StringParamConverter.TryConvertParams(method, new object[] { "1" });
            Assert.AreEqual(typeof(int), getParams.First().GetType());
        }

        [Test]
        public void ShouldTryToConvertStringParameterToBool()
        {
            var type = new TestTypeConversion().GetType();
            var method = type.GetMethod("Bool");

            var getParams = StringParamConverter.TryConvertParams(method, new object[] { "false" });
            Assert.AreEqual(typeof(bool), getParams.First().GetType());
        }

        [Test]
        public void ShouldTryToConvertStringParameterToFloat()
        {
            var type = new TestTypeConversion().GetType();
            var method = type.GetMethod("Float");

            var getParams = StringParamConverter.TryConvertParams(method, new object[] { "3.1412" });
            Assert.AreEqual(typeof(float), getParams.First().GetType());
        }

        [Test]
        public void ShouldTryToConvertStringParameterToString()
        {
            var type = new TestTypeConversion().GetType();
            var method = type.GetMethod("Int");

            var getParams = StringParamConverter.TryConvertParams(method, new object[] { "hahaha" });
            Assert.AreEqual(typeof(string), getParams.First().GetType());
        }
    }
}