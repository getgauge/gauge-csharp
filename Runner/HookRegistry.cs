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
using System.Reflection;
using Gauge.CSharp.Lib.Attribute;

namespace Gauge.CSharp.Runner
{
    [Serializable]
    public class HookRegistry : IHookRegistry
    {
        private readonly IDictionary<Type, HashSet<MethodInfo>> _hooks = new Dictionary<Type, HashSet<MethodInfo>>()
        {
            {typeof (BeforeSuite), new HashSet<MethodInfo>()},
            {typeof (AfterSuite), new HashSet<MethodInfo>()},
            {typeof (BeforeSpec), new HashSet<MethodInfo>()},
            {typeof (AfterSpec), new HashSet<MethodInfo>()},
            {typeof (BeforeScenario), new HashSet<MethodInfo>()},
            {typeof (AfterScenario), new HashSet<MethodInfo>()},
            {typeof (BeforeStep), new HashSet<MethodInfo>()},
            {typeof (AfterStep), new HashSet<MethodInfo>()},
        };

        public HashSet<MethodInfo> BeforeSuiteHooks
        {
            get { return GetHookOfType(typeof (BeforeSuite)); }
        }
 
        public void AddBeforeSuiteHooks(IEnumerable<MethodInfo> beforeSuiteHook)
        {
            AddHookOfType(typeof (BeforeSuite), beforeSuiteHook);
        }

        public HashSet<MethodInfo> AfterSuiteHooks
        {
            get { return GetHookOfType(typeof (AfterSuite)); }
        }

        public void AddAfterSuiteHooks(IEnumerable<MethodInfo> afterSuiteHook)
        {
            AddHookOfType(typeof (AfterSuite), afterSuiteHook);
        }

        public HashSet<MethodInfo> BeforeSpecHooks
        {
            get { return GetHookOfType(typeof (BeforeSpec)); }
        }

        public void AddBeforeSpecHooks(IEnumerable<MethodInfo> beforeSpecHook)
        {
            AddHookOfType(typeof (BeforeSpec), beforeSpecHook);
        }

        public HashSet<MethodInfo> AfterSpecHooks
        {
            get { return GetHookOfType(typeof (AfterSpec)); }
        }

        public void AddAfterSpecHooks(IEnumerable<MethodInfo> afterSpecHook)
        {
            AddHookOfType(typeof (AfterSpec), afterSpecHook);
        }

        public HashSet<MethodInfo> BeforeScenarioHooks
        {
            get { return GetHookOfType(typeof (BeforeScenario)); }
        }

        public void AddBeforeScenarioHooks(IEnumerable<MethodInfo> beforeScenarioHook)
        {
            AddHookOfType(typeof (BeforeScenario), beforeScenarioHook);
        }

        public HashSet<MethodInfo> AfterScenarioHooks
        {
            get { return GetHookOfType(typeof (AfterScenario)); }
        }

        public void AddAfterScenarioHooks(IEnumerable<MethodInfo> afterScenarioHook)
        {
            AddHookOfType(typeof (AfterScenario), afterScenarioHook);
        }

        public HashSet<MethodInfo> BeforeStepHooks
        {
            get { return GetHookOfType(typeof (BeforeStep)); }
        }

        public void AddBeforeStepHooks(IEnumerable<MethodInfo> beforeStepHook)
        {
            AddHookOfType(typeof (BeforeStep), beforeStepHook);
        }

        public HashSet<MethodInfo> AfterStepHooks
        {
            get { return GetHookOfType(typeof (AfterStep)); }
        }

        public void AddAfterStepHooks(IEnumerable<MethodInfo> afterStepHook)
        {
            AddHookOfType(typeof (AfterStep), afterStepHook);
        }

        private void AddHookOfType(Type hookType, IEnumerable<MethodInfo> hook)
        {
            _hooks[hookType].UnionWith(hook);
        }
        private HashSet<MethodInfo> GetHookOfType(Type type)
        {
            return new HashSet<MethodInfo>(_hooks[type]);
        }
    }
}