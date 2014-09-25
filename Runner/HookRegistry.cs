using System;
using System.Collections.Generic;
using System.Reflection;
using Gauge.CSharp.Lib.Attribute;

namespace Gauge.CSharp.Runner
{
    public class HookRegistry
    {
        private static readonly IDictionary<Type, HashSet<MethodInfo>> Hooks = new Dictionary<Type, HashSet<MethodInfo>>();

        public static HashSet<MethodInfo> BeforeSuiteHooks
        {
            get { return GetHookOfType(typeof (BeforeSuite)); }
        }

        private static HashSet<MethodInfo> GetHookOfType(Type type)
        {
            return new HashSet<MethodInfo>(Hooks[type]);
        }

        public static void AddBeforeSuiteHooks(IEnumerable<MethodInfo> beforeSuiteHook)
        {
            AddHookOfType(typeof (BeforeSuite), beforeSuiteHook);
        }

        private static void AddHookOfType(Type hookType, IEnumerable<MethodInfo> hook)
        {
            Hooks[hookType].UnionWith(hook);
        }

        public static HashSet<MethodInfo> AfterSuiteHooks
        {
            get { return GetHookOfType(typeof (AfterSuite)); }
        }

        public static void AddAfterSuiteHooks(IEnumerable<MethodInfo> afterSuiteHook)
        {
            AddHookOfType(typeof (AfterSuite), afterSuiteHook);
        }

        public static HashSet<MethodInfo> BeforeSpecHooks
        {
            get { return GetHookOfType(typeof (BeforeSpec)); }
        }

        public static void AddBeforeSpecHooks(IEnumerable<MethodInfo> beforeSpecHook)
        {
            AddHookOfType(typeof (BeforeSpec), beforeSpecHook);
        }

        public static HashSet<MethodInfo> AfterSpecHooks
        {
            get { return GetHookOfType(typeof (AfterSpec)); }
        }

        public static void AddAfterSpecHooks(IEnumerable<MethodInfo> afterSpecHook)
        {
            AddHookOfType(typeof (AfterSpec), afterSpecHook);
        }

        public static HashSet<MethodInfo> BeforeScenarioHooks
        {
            get { return GetHookOfType(typeof (BeforeScenario)); }
        }

        public static void AddBeforeScenarioHooks(IEnumerable<MethodInfo> beforeScenarioHook)
        {
            AddHookOfType(typeof (BeforeScenario), beforeScenarioHook);
        }

        public static HashSet<MethodInfo> AfterScenarioHooks
        {
            get { return GetHookOfType(typeof (AfterScenario)); }
        }

        public static void AddAfterScenarioHooks(IEnumerable<MethodInfo> afterScenarioHook)
        {
            AddHookOfType(typeof (AfterScenario), afterScenarioHook);
        }

        public static HashSet<MethodInfo> BeforeStepHooks
        {
            get { return GetHookOfType(typeof (BeforeStep)); }
        }

        public static void AddBeforeStepHooks(IEnumerable<MethodInfo> beforeStepHook)
        {
            AddHookOfType(typeof (BeforeStep), beforeStepHook);
        }

        public static HashSet<MethodInfo> AfterStepHooks
        {
            get { return GetHookOfType(typeof (AfterStep)); }
        }

        public static void AddAfterStepHooks(IEnumerable<MethodInfo> afterStepHook)
        {
            AddHookOfType(typeof (AfterStep), afterStepHook);
        }
    }
}