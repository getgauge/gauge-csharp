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
using System.Text;

namespace Gauge.CSharp.Runner
{
    [Serializable]
    public class HookRegistry : IHookRegistry
    {
        private readonly Assembly _targetLibAssembly;

		private readonly IDictionary<Type, HashSet<HookMethod>> _hooks;

		[Serializable]
		private class TypeEqualityComparerByFullName : IEqualityComparer<Type>
		{
			#region IEqualityComparer implementation
			public bool Equals (Type x, Type y)
			{
				if (x == y)
					return true;
				return x.FullName.Equals (y.FullName);
			}
			public int GetHashCode (Type obj)
			{
				return obj.FullName.GetHashCode ();
			}
			#endregion
		}

        public HookRegistry(IAssemblyLoader assemblyLoader)
        {
			/* Must use equality by full name because hook types (e.g. BeforeSuite) registered from sandbox side 
			 * MAY have different version than one in runner's domain.
			 * Then trying to get key for BeforeSuite version 0.5.3 from runner will thrown key not found,
			 * because HashSet was added for BeforeSuite version 0.5.1
			 */
			_hooks = new Dictionary<Type, HashSet<HookMethod>> (new TypeEqualityComparerByFullName());
			_hooks.Add (typeof(BeforeSuite), new HashSet<HookMethod> ());
			_hooks.Add (typeof(AfterSuite), new HashSet<HookMethod>());
			_hooks.Add (typeof(BeforeSpec), new HashSet<HookMethod>());
			_hooks.Add (typeof(AfterSpec), new HashSet<HookMethod> ());
			_hooks.Add (typeof(BeforeScenario), new HashSet<HookMethod> ());
			_hooks.Add (typeof(AfterScenario), new HashSet<HookMethod> ());
			_hooks.Add (typeof(BeforeStep), new HashSet<HookMethod> ());
			_hooks.Add (typeof(AfterStep), new HashSet<HookMethod> ());

            _targetLibAssembly = assemblyLoader.GetTargetLibAssembly();
            foreach (var type in _hooks.Keys)
            {
                AddHookOfType(type, assemblyLoader.GetMethods(type));
            }
        }

        public HashSet<HookMethod> BeforeSuiteHooks
        {
            get { return GetHooks(typeof (BeforeSuite)); }
        }
 
        public HashSet<HookMethod> AfterSuiteHooks
        {
            get { return GetHooks(typeof (AfterSuite)); }
        }

        public HashSet<HookMethod> BeforeSpecHooks
        {
            get { return GetHooks(typeof (BeforeSpec)); }
        }

        public HashSet<HookMethod> AfterSpecHooks
        {
            get { return GetHooks(typeof (AfterSpec)); }
        }

        public HashSet<HookMethod> BeforeScenarioHooks
        {
            get { return GetHooks(typeof (BeforeScenario)); }
        }

        public HashSet<HookMethod> AfterScenarioHooks
        {
            get { return GetHooks(typeof (AfterScenario)); }
        }

        public HashSet<HookMethod> BeforeStepHooks
        {
            get { return GetHooks(typeof (BeforeStep)); }
        }

        public HashSet<HookMethod> AfterStepHooks
        {
            get {
				return GetHooks (typeof(AfterStep));
			}
        }

		HashSet<HookMethod> GetHooks (Type type)
		{
			HashSet<HookMethod> list;
			if (_hooks.TryGetValue (type, out list))
				return list;
			throw new InvalidOperationException (String.Format (
				"Hook of type {0} was not registered. Other hook types {1}", 
				type.AssemblyQualifiedName, HookTypesAsString));
		}

		string HookTypesAsString {
			get{
				StringBuilder b = new StringBuilder();
				b.AppendLine ();
				foreach (var type in _hooks.Keys) {
					b.AppendLine (type.AssemblyQualifiedName);
				}
				return b.ToString ();
			}
		}

        private void AddHookOfType(Type hookType, IEnumerable<MethodInfo> hooks)
        {
            _hooks[hookType].UnionWith(hooks.Select(info => new HookMethod(info, _targetLibAssembly)));
        }
    }
}