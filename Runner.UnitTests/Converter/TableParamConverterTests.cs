// Copyright 2015 ThoughtWorks, Inc.

// This file is part of Gauge-CSharp.

// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// Gauge-Ruby is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System.Linq;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Runner.Converters;
using Gauge.Messages;
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