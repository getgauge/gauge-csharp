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
using System.IO;

namespace Gauge.CSharp.Core
{
    public class Utils
    {
        private const string GaugePortEnv = "GAUGE_INTERNAL_PORT";
        private const string GaugeApiPortEnv = "GAUGE_API_PORT";
        private const string GaugeProjectRootEnv = "GAUGE_PROJECT_ROOT";
        private const string GaugeCustomBuildPath = "GAUGE_CUSTOM_BUILD_PATH";
            

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

        public static string ReadEnvValue(string env)
        {
			string envValue = TryReadEnvValue(env);
			if(envValue == null)
				throw new Exception (env + " is not set");
			return envValue;
        }

		public static string TryReadEnvValue(string env)
		{
			if (env == null)
				throw new ArgumentNullException ("env");
			
			var envValue = Environment.GetEnvironmentVariable(env.ToUpper());
			if (string.IsNullOrEmpty(envValue))
			{
				envValue = Environment.GetEnvironmentVariable(env.ToLower());
				if (string.IsNullOrEmpty (envValue)) {
					return null;
				}
			}
			return envValue;
		}

        public static string GetGaugeBinDir()
        {
			var customBuildPath = Utils.TryReadEnvValue(GaugeCustomBuildPath);
            if (string.IsNullOrEmpty(customBuildPath))
            {
                return Path.Combine(GaugeProjectRoot, "gauge-bin");
            }
            try
            {
                return IsAbsoluteUrl(customBuildPath) ? customBuildPath : Path.Combine(GaugeProjectRoot, customBuildPath);
            }
            catch (Exception)
            {
                return Path.Combine(GaugeProjectRoot, "gauge-bin");
            }
        }

        public static bool IsAbsoluteUrl(string url)
        {
            Uri result;
            return Uri.TryCreate(url, UriKind.Absolute, out result);
        }
    }
}