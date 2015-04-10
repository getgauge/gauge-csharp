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

using System;
using System.Collections.Generic;

namespace Gauge.CSharp.Lib
{
    public class Table
    {
        private readonly List<String> _headers;
        private readonly List<List<String>> _rows;

        public Table(List<String> headers)
        {
            _headers = headers;
            _rows = new List<List<String>>();
        }

        public void AddRow(List<String> row)
        {
            _rows.Add(row);
        }

        public List<String> GetColumnNames()
        {
            return _headers;
        }

        public List<List<String>> GetRows()
        {
            return _rows;
        }
    }
}