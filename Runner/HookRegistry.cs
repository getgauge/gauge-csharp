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
using Gauge.CSharp.Lib.Attribute;

namespace Gauge.CSharp.Runner
{
    [Serializable]
    public class HookRegistry : IHookRegistry
    {
        private readonly ISandbox _sandbox;

        private readonly IDictionary<Type, HashSet<HookMethod>> _hooks = new Dictionary<Type, HashSet<HookMethod>>()
        {
            {typeof (BeforeSuite), new HashSet<HookMethod>()},
            {typeof (AfterSuite), new HashSet<HookMethod>()},
            {typeof (BeforeSpec), new HashSet<HookMethod>()},
            {typeof (AfterSpec), new HashSet<HookMethod>()},
            {typeof (BeforeScenario), new HashSet<HookMethod>()},
            {typeof (AfterScenario), new HashSet<HookMethod>()},
            {typeof (BeforeStep), new HashSet<HookMethod>()},
            {typeof (AfterStep), new HashSet<HookMethod>()},
        };

        public HookRegistry(ISandbox sandbox)
        {
            _sandbox = sandbox;
        }

        public HashSet<HookMethod> BeforeSuiteHooks
        {
            get { return GetHookOfType(typeof (BeforeSuite)); }
        }
 
        public void AddBeforeSuiteHooks(IEnumerable<MethodInfo> beforeSuiteHook)
        {
            AddHookOfType(typeof (BeforeSuite), beforeSuiteHook);
        }

        public HashSet<HookMethod> AfterSuiteHooks
        {
            get { return GetHookOfType(typeof (AfterSuite)); }
        }

        public void AddAfterSuiteHooks(IEnumerable<MethodInfo> afterSuiteHook)
        {
            AddHookOfType(typeof (AfterSuite), afterSuiteHook);
        }

        public HashSet<HookMethod> BeforeSpecHooks
        {
            get { return GetHookOfType(typeof (BeforeSpec)); }
        }

        public void AddBeforeSpecHooks(IEnumerable<MethodInfo> beforeSpecHook)
        {
            AddHookOfType(typeof (BeforeSpec), beforeSpecHook);
        }

        public HashSet<HookMethod> AfterSpecHooks
        {
            get { return GetHookOfType(typeof (AfterSpec)); }
        }

        public void AddAfterSpecHooks(IEnumerable<MethodInfo> afterSpecHook)
        {
            AddHookOfType(typeof (AfterSpec), afterSpecHook);
        }

        public HashSet<HookMethod> BeforeScenarioHooks
        {
            get { return GetHookOfType(typeof (BeforeScenario)); }
        }

        public void AddBeforeScenarioHooks(IEnumerable<MethodInfo> beforeScenarioHook)
        {
            AddHookOfType(typeof (BeforeScenario), beforeScenarioHook);
        }

        public HashSet<HookMethod> AfterScenarioHooks
        {
            get { return GetHookOfType(typeof (AfterScenario)); }
        }

        public void AddAfterScenarioHooks(IEnumerable<MethodInfo> afterScenarioHook)
        {
            AddHookOfType(typeof (AfterScenario), afterScenarioHook);
        }

        public HashSet<HookMethod> BeforeStepHooks
        {
            get { return GetHookOfType(typeof (BeforeStep)); }
        }

        public void AddBeforeStepHooks(IEnumerable<MethodInfo> beforeStepHook)
        {
            AddHookOfType(typeof (BeforeStep), beforeStepHook);
        }

        public HashSet<HookMethod> AfterStepHooks
        {
            get { return GetHookOfType(typeof (AfterStep)); }
        }

        public void AddAfterStepHooks(IEnumerable<MethodInfo> afterStepHook)
        {
            AddHookOfType(typeof (AfterStep), afterStepHook);
        }

        private void AddHookOfType(Type hookType, IEnumerable<MethodInfo> hooks)
        {
            _hooks[hookType].UnionWith(hooks.Select(info => new HookMethod(info, _sandbox)));
        }

        private HashSet<HookMethod> GetHookOfType(Type type)
        {
            return _hooks[type];
        }
    }
}