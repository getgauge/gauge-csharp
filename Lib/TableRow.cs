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
    ///     Holds data of a row in Table.
    /// </summary>
    [Serializable]
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

        public int Size()
        {
            return _cells.Count;
        }

        public override string ToString()
        {
            var allCells = _cells.Aggregate("", (current, pair) => current + pair.Key + " = " + pair.Value + ", ")
                .Trim();
            return string.Format("TableRow: cells: [{0}] ", allCells.Substring(0, allCells.Length - 1).Trim());
        }
    }
}