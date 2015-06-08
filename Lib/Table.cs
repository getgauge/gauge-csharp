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

using System.Collections.Generic;

namespace Gauge.CSharp.Lib
{
    /// <summary>
    /// Holds a matrix of data, that is equivalent to Markdown representation of a table, or tablular data defined in a csv file.
    /// </summary>
    public class Table
    {
        private readonly List<string> _headers;
        private readonly List<List<string>> _rows;

        /// <summary>
        /// Creates a new Table type
        /// </summary>
        /// <param name="headers">A List of string representing the headers, in order.</param>
        public Table(List<string> headers)
        {
            _headers = headers;
            _rows = new List<List<string>>();
        }

        /// <summary>
        /// Add a row of data to the table.
        /// </summary>
        /// <param name="row">List of string representing the tuple of a table.</param>
        public void AddRow(List<string> row)
        {
            _rows.Add(row);
        }

        /// <summary>
        /// Fetch all column headers of a table.
        /// </summary>
        /// <returns>List of string representing the column headers of table.</returns>
        public List<string> GetColumnNames()
        {
            return _headers;
        }

        /// <summary>
        /// Fetch all the rows of a table, in order.
        /// </summary>
        /// <returns>List of string representing the tuples of a table.</returns>
        public List<List<string>> GetRows()
        {
            return _rows;
        }
    }
}