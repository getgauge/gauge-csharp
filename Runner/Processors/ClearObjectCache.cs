using System;

namespace Gauge.CSharp.Runner.Processors
{
    public class ClearObjectCache
    {
        public static string SuiteLevel = "suite";
        public static string SpecLevel = "spec";
        public static string ScenarioLevel = "scenario";
        public static string ClearStateFlag = "gauge_clear_state_level";


        public static void ClearCache(string currentLevel)
        {
            var flag = Environment.GetEnvironmentVariable(ClearStateFlag);
            if (!string.IsNullOrEmpty(flag) && flag.Trim().Equals(currentLevel))
                ClassInstanceManager.ClearCache();
        }
    }
}