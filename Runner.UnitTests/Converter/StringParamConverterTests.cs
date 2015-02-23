using Gauge.CSharp.Runner.Converters;
using Gauge.Messages;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Converter
{
    [TestFixture]
    public class StringParamConverterTests
    {
        [Test]
        public void ShouldConvertFromParameterToString()
        {
            const string expected = "foo";
            var parameter = new Parameter.Builder().SetParameterType(Parameter.Types.ParameterType.Static).SetValue(expected).Build();
            
            var actual = new StringParamConverter().Convert(parameter);
            
            Assert.AreEqual(expected, actual);
        }
    }
}