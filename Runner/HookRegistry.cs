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
        private readonly Assembly _targetLibAssembly;

        private readonly IDictionary<Type, HashSet<HookMethod>> _hooks = new Dictionary<Type, HashSet<HookMethod>>
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

        public HookRegistry(IAssemblyScanner assemblyScanner)
        {
            _targetLibAssembly = assemblyScanner.GetTargetLibAssembly();
            foreach (var type in _hooks.Keys)
            {
                AddHookOfType(type, assemblyScanner.GetMethods(type));
            }
        }

        public HashSet<HookMethod> BeforeSuiteHooks
        {
            get { return _hooks[typeof (BeforeSuite)]; }
        }
 
        public HashSet<HookMethod> AfterSuiteHooks
        {
            get { return _hooks[typeof (AfterSuite)]; }
        }

        public HashSet<HookMethod> BeforeSpecHooks
        {
            get { return _hooks[typeof (BeforeSpec)]; }
        }

        public HashSet<HookMethod> AfterSpecHooks
        {
            get { return _hooks[typeof (AfterSpec)]; }
        }

        public HashSet<HookMethod> BeforeScenarioHooks
        {
            get { return _hooks[typeof (BeforeScenario)]; }
        }

        public HashSet<HookMethod> AfterScenarioHooks
        {
            get { return _hooks[typeof (AfterScenario)]; }
        }

        public HashSet<HookMethod> BeforeStepHooks
        {
            get { return _hooks[typeof (BeforeStep)]; }
        }

        public HashSet<HookMethod> AfterStepHooks
        {
            get { return _hooks[typeof (AfterStep)]; }
        }

        private void AddHookOfType(Type hookType, IEnumerable<MethodInfo> hooks)
        {
            _hooks[hookType].UnionWith(hooks.Select(info => new HookMethod(info, _targetLibAssembly)));
        }
    }
}