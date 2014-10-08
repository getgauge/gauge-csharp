using System;
using System.Collections.Generic;
using System.Reflection;
using Gauge.CSharp.Lib.Attribute;

namespace Gauge.CSharp.Runner
{
    public class HookRegistry
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