using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Gauge.CSharp.Lib
{
    public class TableRow
    {
        private readonly Dictionary<string, string> _cells = new Dictionary<string, string>();

        public void AddCell(string columnName, string cellValue)
        {
            _cells.Add(columnName, cellValue);
        }

        public string GetCell(string columnName)
        {
            return _cells.ContainsKey(columnName) ? _cells[columnName] : "";
        }

        public override string ToString()
        {
            var allCells = _cells.Aggregate("", (current, pair) => current + (pair.Key + " = " + pair.Value + ", ")).Trim();
            return string.Format("TableRow: cells: [{0}] ", allCells.Substring(0, allCells.Length-1).Trim());
        }
    }
}