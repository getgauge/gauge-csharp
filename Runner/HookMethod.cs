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
using System.Reflection;
using Gauge.CSharp.Lib.Attribute;

namespace Gauge.CSharp.Runner
{
    [Serializable]
    public class HookMethod
    {
        private readonly MethodInfo _methodInfo;

        public readonly TagAggregation TagAggregation = TagAggregation.And;

        public readonly IEnumerable<string> FilterTags = Enumerable.Empty<string>();

        public HookMethod(MethodInfo methodInfo, ISandbox sandbox)
        {
            var targetHookType = GetTargetType(typeof(FilteredHookAttribute), sandbox);
            _methodInfo = methodInfo;
            dynamic filteredHookAttribute = _methodInfo.GetCustomAttribute(targetHookType);
            if (filteredHookAttribute == null) return;

            FilterTags = filteredHookAttribute.FilterTags;
            var targetTagBehaviourType = GetTargetType(typeof(TagAggregationBehaviourAttribute), sandbox);
            dynamic tagAggregationBehaviourAttribute = _methodInfo.GetCustomAttribute(targetTagBehaviourType);
            TagAggregation setTagAggregation;
            var parseResult = Enum.TryParse((string) tagAggregationBehaviourAttribute.TagAggregation.ToString(), true, out setTagAggregation);
            TagAggregation = parseResult ? setTagAggregation : TagAggregation.And;
        }

        private static Type GetTargetType(Type type, ISandbox sandbox)
        {
            var targetLibAssembly = sandbox.TargetLibAssembly;
            var targetHookType = targetLibAssembly.GetType(type.FullName);
            return targetHookType;
        }

        public MethodInfo Method
        {
            get { return _methodInfo; }
        }
    }
}