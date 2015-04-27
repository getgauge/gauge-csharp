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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Communication;

namespace Gauge.CSharp.Runner
{
    public class MethodScanner : IMethodScanner
    {
        private readonly GaugeApiConnection _apiConnection;

        private readonly Sandbox _sandbox = Sandbox.Instance;

        public MethodScanner(GaugeApiConnection apiConnection)
        {
            _apiConnection = apiConnection;
        }

        public IStepRegistry GetStepRegistry()
        {
            return new StepRegistry(GetStepMethods());
        }

        public IEnumerable<string> GetStepTexts()
        {
            return _sandbox.GetStepMethods().SelectMany(stepMethod => stepMethod.GetCustomAttribute<Step>().Names);
        }

        private IEnumerable<KeyValuePair<string, MethodInfo>> GetStepMethods()
        {
            var retVal = new List<KeyValuePair<string, MethodInfo>>();
            try
            {
                var stepMethods = _sandbox.GetStepMethods();
                foreach (var stepMethod in stepMethods)
                {
                    // HasTable is set to false here, table parameter is interpreted using the Step text.
                    var stepValues = _apiConnection.GetStepValue(stepMethod.GetCustomAttribute<Step>().Names,
                        false);

                    retVal.AddRange(
                        stepValues.Select(stepValue => new KeyValuePair<string, MethodInfo>(stepValue, stepMethod)));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WARN] Steps Fetch failed, Failed to connect to Gauge API");
                Console.WriteLine(ex.Message);
            }
            return retVal;
        }

        public IHookRegistry GetHookRegistry()
        {
            return _sandbox.GetHookRegistry();
        }
    }
}