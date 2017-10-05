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
using Gauge.CSharp.Core;
using Gauge.CSharp.Runner.Models;
using NLog;

namespace Gauge.CSharp.Runner
{
    public class MethodScanner : IMethodScanner
    {
        private static readonly Logger Logger = LogManager.GetLogger("MethodScanner");
        private readonly GaugeApiConnection _apiConnection;

        private readonly ISandbox _sandbox;

        public MethodScanner(GaugeApiConnection apiConnection, ISandbox sandbox)
        {
            _apiConnection = apiConnection;
            _sandbox = sandbox;
        }

        public IStepRegistry GetStepRegistry()
        {
            var stepImplementations = new List<KeyValuePair<string, GaugeMethod>>();
            var aliases = new Dictionary<string, bool>();
            var stepTextMap = new Dictionary<string, string>();
            try
            {
                var stepMethods = _sandbox.GetStepMethods();
                foreach (var stepMethod in stepMethods)
                {
                    // HasTable is set to false here, table parameter is interpreted using the Step text.
                    var stepTexts = _sandbox.GetStepTexts(stepMethod).ToList();
                    var stepValues = _apiConnection.GetStepValues(stepTexts, false).ToList();

                    for (var i = 0; i < stepTexts.Count; i++)
                        if (!stepTextMap.ContainsKey(stepValues[i]))
                            stepTextMap.Add(stepValues[i], stepTexts[i]);

                    stepImplementations.AddRange(stepValues.Select(stepValue =>
                        new KeyValuePair<string, GaugeMethod>(stepValue, stepMethod)));

                    if (stepValues.Count <= 1) continue;

                    foreach (var stepValue in stepValues)
                        if (!aliases.ContainsKey(stepValue))
                            aliases.Add(stepValue, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.InnerException);
                Logger.Warn(ex, "Steps Fetch failed, Failed to connect to Gauge API");
            }
            return new StepRegistry(stepImplementations, stepTextMap, aliases);
        }

        public IEnumerable<string> GetStepTexts()
        {
            return _sandbox.GetAllStepTexts();
        }
    }
}