using System;
using System.Collections.Generic;
using System.Linq;
using Gauge.CSharp.Lib;
using Gauge.Messages;

namespace Gauge.CSharp.Runner.Converters
{
    public class TableParamConverter : IParamConverter
    {
        public object Convert(Parameter parameter)
        {
            var protoTable = parameter.Table;
            if (protoTable == null || protoTable.Headers == null)
            {
                throw new Exception("Invalid table passed to step");
            }
            var protoTableRow = protoTable.Headers;
            var header = getTableRowFor(protoTableRow);
            var table = new Table(header);
            for (var i = 0; i < protoTable.RowsCount; i++)
            {
                var row = getTableRowFor(protoTable.GetRows(i));
                table.AddRow(row);
            }
            return table;
        }

        private List<string> getTableRowFor(ProtoTableRow tableRow)
        {
            return tableRow.CellsList.ToList();
        }
    }
}