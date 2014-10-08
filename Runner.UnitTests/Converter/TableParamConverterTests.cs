using System.Linq;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Runner.Converters;
using main;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Converter
{
    [TestFixture]
    public class TableParamConverterTests
    {
        [Test]
        public void ShouldConvertToTableFromParameter()
        {
            var headers = new ProtoTableRow.Builder().AddRangeCells(new[] {"header"}).Build();
            var cells = new ProtoTableRow.Builder().AddRangeCells(new[] {"foo"}).Build();
            var table = new ProtoTable.Builder().SetHeaders(headers).AddRangeRows(new[] {cells});
            var parameter = new Parameter.Builder()
                .SetParameterType(Parameter.Types.ParameterType.Table)
                .SetTable(table).Build();
            
            var actual = new TableParamConverter().Convert(parameter) as Table;
            
            Assert.NotNull(actual);
            Assert.That(actual.GetColumnNames(), Contains.Item("header"));
            Assert.That(actual.GetRows().First(), Contains.Item("foo"));
        }
    }
}