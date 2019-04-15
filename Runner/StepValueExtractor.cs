// Copyright 2015 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Gauge.CSharp.Runner
{
    public class StepValueExtractor
    {
        public IEnumerable<string> ExtractFrom(IEnumerable<string> stepTexts)
        {
            var stepValues = new List<string>();
            foreach (var stepText in stepTexts)
            {
                stepValues.Add(Regex.Replace(stepText, @"<.*?>", "{}"));
            }
            return stepValues;
        }
        
    }
}