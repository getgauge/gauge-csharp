using System;
using System.IO;

namespace Gauge.CSharp.Lib
{
    public class Utils
    {
        private const string GaugePortEnv = "GAUGE_INTERNAL_PORT";
        private const string GaugeApiPortEnv = "GAUGE_API_PORT";
        private const string GaugeProjectRootEnv = "GAUGE_PROJECT_ROOT";

        public static int GaugePort {
            get { return Convert.ToInt32(ReadEnvValue(GaugePortEnv)); }
        }
        public static string GaugeProjectRoot
        {
            get { return ReadEnvValue(GaugeProjectRootEnv); }
        }

        public static int GaugeApiPort
        {
            get { return Convert.ToInt32(ReadEnvValue(GaugeApiPortEnv)); }
        }

        public static string GaugeBinDir
        {
            get { return Path.Combine(GaugeProjectRoot, "gauge-bin"); }
        }

        private static string ReadEnvValue(string env)
        {
            var envValue = Environment.GetEnvironmentVariable(env);
            if (string.IsNullOrEmpty(envValue))
            {
                throw new Exception(env + " is not set");
            }
            return envValue;
        }
    }
}