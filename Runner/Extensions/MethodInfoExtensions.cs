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

using System.Linq;
using System.Reflection;

namespace Gauge.CSharp.Runner.Extensions
{
    public static class MethodInfoExtensions
    {
        public static string FullyQuallifiedName(this MethodInfo info)
        {
            var parameters = info.GetParameters();
            var parameterText = parameters.Length > 0
                ? "-" + parameters.Select(parameterInfo => string.Concat(parameterInfo.ParameterType.Name, parameterInfo.Name))
                    .Aggregate(string.Concat)
                : string.Empty;

            return info.DeclaringType == null
                ? info.Name
                : string.Format("{0}.{1}{2}", info.DeclaringType.FullName, info.Name, parameterText);
        }

        public static bool IsRecoverableStep(this MethodInfo info)
        {
            var customAttributes = info.GetCustomAttributes().ToList();
            return customAttributes.Any(attribute => attribute.GetType().FullName == "Gauge.CSharp.Lib.Attribute.Step") &&
            customAttributes.Any(attribute => attribute.GetType().FullName == "Gauge.CSharp.Lib.Attribute.ContinueOnFailure");
        }
    }
}
