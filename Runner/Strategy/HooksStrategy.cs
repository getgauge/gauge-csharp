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
using System.Linq;
using System.Reflection;
using Gauge.CSharp.Lib.Attribute;

namespace Gauge.CSharp.Runner.Strategy
{
    public class HooksStrategy
    {
        public IEnumerable<MethodInfo> GetTaggedHooks(IEnumerable<string> applicableTags, IEnumerable<HookMethod> hooks)
        {
            var tagsList = applicableTags.ToList();
            return from hookMethod in hooks.ToList()
                where hookMethod.FilterTags != null
                where
                    hookMethod.TagAggregation == TagAggregation.Or && hookMethod.FilterTags.Intersect(tagsList).Any() ||
                    hookMethod.TagAggregation == TagAggregation.And && hookMethod.FilterTags.All(tagsList.Contains)
                select hookMethod.Method;
        }

        protected IEnumerable<MethodInfo> GetUntaggedHooks(IEnumerable<HookMethod> hookMethods)
        {
            return hookMethods.Where(method => method.FilterTags == null || !method.FilterTags.Any() ).Select(method => method.Method);
        }


        public virtual IEnumerable<MethodInfo> GetApplicableHooks(List<string> applicableTags, IEnumerable<HookMethod> hooks)
        {
            return GetUntaggedHooks(hooks);
        }
    }
}