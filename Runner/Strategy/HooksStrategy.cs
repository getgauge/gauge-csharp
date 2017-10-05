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
using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Models;

namespace Gauge.CSharp.Runner.Strategy
{
    [Serializable]
    public class HooksStrategy : IHooksStrategy
    {
        public IEnumerable<string> GetTaggedHooks(IEnumerable<string> applicableTags, IList<IHookMethod> hooks)
        {
            var tagsList = applicableTags.ToList();
            return from hookMethod in hooks.ToList()
                where hookMethod.FilterTags != null
                where
                    hookMethod.TagAggregation == TagAggregation.Or && hookMethod.FilterTags.Intersect(tagsList).Any() ||
                    hookMethod.TagAggregation == TagAggregation.And && hookMethod.FilterTags.All(tagsList.Contains)
                orderby hookMethod.Method
                select hookMethod.Method;
        }

        public virtual IEnumerable<string> GetApplicableHooks(IEnumerable<string> applicableTags,
            IEnumerable<IHookMethod> hooks)
        {
            return GetUntaggedHooks(hooks);
        }

        protected IOrderedEnumerable<string> GetUntaggedHooks(IEnumerable<IHookMethod> hookMethods)
        {
            return hookMethods.Where(method => method.FilterTags == null || !method.FilterTags.Any())
                .Select(method => method.Method)
                .OrderBy(info => info);
        }
    }
}