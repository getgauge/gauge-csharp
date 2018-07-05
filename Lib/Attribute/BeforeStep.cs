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

namespace Gauge.CSharp.Lib.Attribute
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BeforeStep : FilteredHookAttribute
    {
        /// <summary>
        ///     Creates a hook that gets executed before every Step.
        /// </summary>
        public BeforeStep()
        {
        }

        /// <summary>
        ///     Creates a hook that gets executed before every Step.
        ///     Filter the hook execution by specifying a tag.
        ///     This hook will be executed only before the Step that has the given tag.
        ///     <para> Example:</para>
        ///     <para>
        ///         <code>[BeforeStep("some tag")]</code>
        ///     </para>
        /// </summary>
        /// <param name="filterTag">Tag to filter the hook execution by.</param>
        public BeforeStep(string filterTag)
            : base(filterTag)
        {
        }

        /// <summary>
        ///     Creates a hook that gets executed before every Step.
        ///     Filter the hook execution by specifying one (or more tags).
        ///     This hook will be executed only before the Step that matches the tag filter.
        ///     <para> Example:</para>
        ///     <para>
        ///         <code>[BeforeStep("tag1", "tag2")]</code>
        ///     </para>
        ///     <para>
        ///         You can control the filtering logic by adding another attribute
        ///         <see cref="TagAggregationBehaviourAttribute" />.
        ///     </para>
        ///     <para>
        ///         By default the hooks are executed only if all the tags specified match the tags of the target
        ///         Spec/Scenario/Step.
        ///     </para>
        /// </summary>
        /// <param name="filterTags">Tags to filter the hook execution by. Multiple tags are passed as additional parameters.</param>
        public BeforeStep(params string[] filterTags) : base(filterTags)
        {
        }
    }
}