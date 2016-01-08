﻿// Copyright 2015 ThoughtWorks, Inc.
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

namespace Gauge.CSharp.Runner.Processors
{
    public class ClearObjectCache
    {
        public static string SuiteLevel = "suite";
        public static string SpecLevel = "spec";
        public static string ScenarioLevel = "scenario";
        public static string ClearStateFlag = "gauge_clear_state_level";

        public static bool ShouldClearObjectCache(string currentLevel)
        {
            var flag = Environment.GetEnvironmentVariable(ClearStateFlag);
            return !string.IsNullOrEmpty(flag) && flag.Trim().Equals(currentLevel);
        }
    }
}