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

using System.Collections.Generic;

namespace Gauge.CSharp.Runner
{
    public class DataStore
    {
        private Dictionary<object, object> _dictionary;

        public void Initialize()
        {
            _dictionary = new Dictionary<object, object>();
        }

        public object Get(string key)
        {
            object outVal;
            var valueExists = _dictionary.TryGetValue(key, out outVal);
            return valueExists ? outVal : null;
        }

        public void Add(string key, object value)
        {// Copyright 2015 ThoughtWorks, Inc.

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


            _dictionary[key] = value;
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public int Count
        {
            get
            {
                return _dictionary.Count;
            }
        }
    }
}