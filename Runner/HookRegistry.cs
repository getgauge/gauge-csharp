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
    public class HookRegistry : IHookRegistry
    {
        private readonly Assembly _targetLibAssembly;

		private readonly IDictionary<string, HashSet<HookMethod>> _hooks;

        private readonly IDictionary<string, MethodInfo> _methodMap = new Dictionary<string, MethodInfo>();

        public HookRegistry(IAssemblyLoader assemblyLoader)
        {
            _hooks = new Dictionary<string, HashSet<HookMethod>>
            {
                {"BeforeSuite", new HashSet<HookMethod>()},
                {"AfterSuite", new HashSet<HookMethod>()},
                {"BeforeSpec", new HashSet<HookMethod>()},
                {"AfterSpec", new HashSet<HookMethod>()},
                {"BeforeScenario", new HashSet<HookMethod>()},
                {"AfterScenario", new HashSet<HookMethod>()},
                {"BeforeStep", new HashSet<HookMethod>()},
                {"AfterStep", new HashSet<HookMethod>()}
            };

            _targetLibAssembly = assemblyLoader.GetTargetLibAssembly();
            foreach (var type in _hooks.Keys)
            {
                AddHookOfType(type, assemblyLoader.GetMethods(string.Format("Gauge.CSharp.Lib.Attribute.{0}", type)));
            }
        }

        public HashSet<HookMethod> BeforeSuiteHooks
        {
            get { return _hooks["BeforeSuite"]; }
        }
 
        public HashSet<HookMethod> AfterSuiteHooks
        {
            get { return _hooks["AfterSuite"]; }
        }

        public HashSet<HookMethod> BeforeSpecHooks
        {
            get { return _hooks["BeforeSpec"]; }
        }

        public HashSet<HookMethod> AfterSpecHooks
        {
            get { return _hooks["AfterSpec"]; }
        }

        public HashSet<HookMethod> BeforeScenarioHooks
        {
            get { return _hooks["BeforeScenario"]; }
        }

        public HashSet<HookMethod> AfterScenarioHooks
        {
            get { return _hooks["AfterScenario"]; }
        }

        public HashSet<HookMethod> BeforeStepHooks
        {
            get { return _hooks["BeforeStep"]; }
        }

        public HashSet<HookMethod> AfterStepHooks
        {
            get { return _hooks["AfterStep"]; }
        }

        private void AddHookOfType(string hookType, IEnumerable<MethodInfo> hooks)
        {
            foreach (var methodInfo in hooks)
            {
                _methodMap.Add(methodInfo.FullyQuallifiedName(), methodInfo);
            }
            _hooks[hookType].UnionWith(hooks.Select(info => new HookMethod(info, _targetLibAssembly)));
        }

        public MethodInfo MethodFor(string method)
        {
            return _methodMap[method];
        }
    }
}