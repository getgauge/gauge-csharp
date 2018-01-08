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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gauge.CSharp.Lib
{
    /// <summary>
    ///     Holds a matrix of data, that is equivalent to Markdown representation of a table, or tablular data defined in a csv
    ///     file.
    /// </summary>
    [Serializable]
    public class Table
    {
        private readonly List<string> _headers;
        private readonly List<List<string>> _rows;
        private readonly List<TableRow> _tableRows;

        /// <summary>
        ///     Creates a new Table type
        /// </summary>
        /// <param name="headers">A List of string representing the headers, in order.</param>
        public Table(List<string> headers)
        {
            _headers = headers;
            _rows = new List<List<string>>();
            _tableRows = new List<TableRow>();
        }

        /// <summary>
        ///     Add a row of data to the table.
        /// </summary>
        /// <param name="row">List of string representing the tuple of a table.</param>
        /// <exception cref="RowSizeMismatchException">Throws RowSizeMismatchException if column size doesn't match row size.</exception>
        public void AddRow(List<string> row)
        {
            if (row.Count != _headers.Count)
                throw new RowSizeMismatchException(string.Format(
                    "Row size mismatch. Expected row size: {0}, Obtained row size: {1}", _headers.Count, row.Count));
            _rows.Add(row);
            var tableRow = new TableRow();
            foreach (var columnValue in _headers)
                tableRow.AddCell(columnValue, row[_headers.IndexOf(columnValue)]);
            _tableRows.Add(tableRow);
        }

        /// <summary>
        ///     Fetch all column headers of a table.
        /// </summary>
        /// <returns>List of string representing the column headers of table.</returns>
        public List<string> GetColumnNames()
        {
            return _headers;
        }

        /// <summary>
        ///     Fetch all the rows of a table, in order.
        /// </summary>
        /// <returns>List of string representing the tuples of a table.</returns>
        [Obsolete("Method GetRows is deprecated, please use GetTableRows instead.")]
        public List<List<string>> GetRows()
        {
            return _rows;
        }

        /// <summary>
        ///     Fetch all the rows of a table represented as TableRow.
        /// </summary>
        /// <returns>List of TableRow representing the tuples of a table.</returns>
        public List<TableRow> GetTableRows()
        {
            return _tableRows;
        }

        /// <summary>
        ///     Fetches all the column values defined under the given column name
        /// </summary>
        /// <param name="columnName">Name of the Column to fetch</param>
        /// <returns>IEnumerable of string containing the given column's values</returns>
        public IEnumerable<string> GetColumnValues(string columnName)
        {
            var columnIndex = _headers.IndexOf(columnName);
            return columnIndex >= 0 ? _rows.Select(list => list[columnIndex]) : Enumerable.Empty<string>();
        }

        /// <summary>
        ///     Converts the table to the Markdown equivalent string
        /// </summary>
        /// <returns>Markdown String of Table</returns>
        public override string ToString()
        {
            IEnumerable<string> columnStrings = new string[_rows.Count + 2];
            foreach (var header in GetColumnNames())
            {
                var columnValues = GetColumnValues(header).ToList();
                var columnWidth = columnValues.Concat(new[] {header}).Max(s => s.Length);
                string formatCellValue(string s) => string.Format("|{0}", s.PadRight(columnWidth, ' '));
                var paddedColumn = new[] {header, new string('-', columnWidth)}.Concat(columnValues)
                    .Select(formatCellValue);
                columnStrings = columnStrings.Zip(paddedColumn, string.Concat);
            }
            return string.Concat(columnStrings.Aggregate((s, s1) => string.Format("{0}|\n{1}", s, s1)), "|");
        }
    }
}