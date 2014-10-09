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