using System;
using System.Collections.Generic;
using System.Linq;
using gauge_csharp_lib;
using main;

namespace gauge_csharp
{
    internal class TableParamConverter : IParamConverter
    {
        public object Convert(Parameter parameter)
        {
            ProtoTable protoTable = parameter.Table;
            return tableFromProto(protoTable);
        }

        private object tableFromProto(ProtoTable protoTable)
        {
            if (protoTable == null || protoTable.Headers == null)
            {
                throw new Exception("Invalid table passed to step");
            }
            ProtoTableRow protoTableRow = protoTable.Headers;
            List<string> header = getTableRowFor(protoTableRow);
            var table = new Table(header);
            for (int i = 0; i < protoTable.RowsCount; i++)
            {
                List<string> row = getTableRowFor(protoTable.GetRows(i));
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