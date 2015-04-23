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
using System.Reflection;

namespace Gauge.CSharp.Runner
{
    public interface IHookRegistry
    {
        HashSet<MethodInfo> BeforeSuiteHooks { get; }
        HashSet<MethodInfo> AfterSuiteHooks { get; }
        HashSet<MethodInfo> BeforeSpecHooks { get; }
        HashSet<MethodInfo> AfterSpecHooks { get; }
        HashSet<MethodInfo> BeforeScenarioHooks { get; }
        HashSet<MethodInfo> AfterScenarioHooks { get; }
        HashSet<MethodInfo> BeforeStepHooks { get; }
        HashSet<MethodInfo> AfterStepHooks { get; }
        void AddBeforeSuiteHooks(IEnumerable<MethodInfo> beforeSuiteHook);
        void AddAfterSuiteHooks(IEnumerable<MethodInfo> afterSuiteHook);
        void AddBeforeSpecHooks(IEnumerable<MethodInfo> beforeSpecHook);
        void AddAfterSpecHooks(IEnumerable<MethodInfo> afterSpecHook);
        void AddBeforeScenarioHooks(IEnumerable<MethodInfo> beforeScenarioHook);
        void AddAfterScenarioHooks(IEnumerable<MethodInfo> afterScenarioHook);
        void AddBeforeStepHooks(IEnumerable<MethodInfo> beforeStepHook);
        void AddAfterStepHooks(IEnumerable<MethodInfo> afterStepHook);
    }
}