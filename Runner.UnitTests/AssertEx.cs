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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    public static class AssertEx
    {
        public static void InheritsFrom<TBase, TDerived>()
        {
            Assert.True(typeof(TDerived).IsSubclassOf(typeof(TBase)),
                string.Format("Expected {0} to be a subclass of {1}", typeof(TDerived).FullName,
                    typeof(TBase).FullName));
        }

        public static void DoesNotInheritsFrom<TBase, TDerived>()
        {
            Assert.False(typeof(TDerived).IsSubclassOf(typeof(TBase)),
                string.Format("Expected {0} to NOT be a subclass of {1}", typeof(TDerived).FullName,
                    typeof(TBase).FullName));
        }

        public static void ContainsMethods(IEnumerable<MethodInfo> methodInfos, params string[] methodNames)
        {
            var existingMethodNames = methodInfos.Select(info => info.Name).ToArray();
            foreach (var methodName in methodNames)
                Assert.Contains(methodName, existingMethodNames);
        }

        public static IEnumerable<string> ExecuteProtectedMethod<T>(string methodName, params object[] methodParams)
        {
            var uninitializedObject = FormatterServices.GetUninitializedObject(typeof(T));
            var tags = (IEnumerable<string>) uninitializedObject.GetType()
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(uninitializedObject, methodParams);
            return tags;
        }
    }
}