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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gauge.CSharp.Runner.Extensions;

namespace Gauge.CSharp.Runner
{
    [Serializable]
    public class HookMethod
    {
        public readonly dynamic TagAggregation;

        public readonly IEnumerable<string> FilterTags = Enumerable.Empty<string>();

        public HookMethod(MethodInfo methodInfo, Assembly targetLibAssembly)
        {
            Method = methodInfo.FullyQuallifiedName();
            var targetHookType = targetLibAssembly.GetType("Gauge.CSharp.Lib.Attribute.FilteredHookAttribute");
            dynamic filteredHookAttribute = methodInfo.GetCustomAttribute(targetHookType);
            if (filteredHookAttribute == null) return;

            FilterTags = filteredHookAttribute.FilterTags;
            var targetTagBehaviourType = targetLibAssembly.GetType("Gauge.CSharp.Lib.Attribute.TagAggregationBehaviourAttribute");
            dynamic tagAggregationBehaviourAttribute = methodInfo.GetCustomAttribute(targetTagBehaviourType);

            var aggregationType = targetLibAssembly.GetType("Gauge.CSharp.Lib.Attribute.TagAggregation");
            var setTagAggregation = Enum.Parse(aggregationType, "And");
            if (!ReferenceEquals(tagAggregationBehaviourAttribute, null))
            {
                setTagAggregation = Enum.Parse(aggregationType, tagAggregationBehaviourAttribute.TagAggregation.ToString());
            }
            TagAggregation = setTagAggregation;
        }

        public string Method { get; private set; }
    }
}