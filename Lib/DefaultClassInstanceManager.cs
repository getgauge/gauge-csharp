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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Gauge.CSharp.Lib
{
    public class DefaultClassInstanceManager : IClassInstanceManager
    {
        private static readonly Hashtable ClassInstanceMap = new Hashtable();

        public void Initialize(List<Assembly> assemblies)
        {
            //nothing to do
        }

        public object Get(Type declaringType)
        {
            if (ClassInstanceMap.ContainsKey(declaringType))
                return ClassInstanceMap[declaringType];
            var instance = Activator.CreateInstance(declaringType);
            ClassInstanceMap.Add(declaringType, instance);
            return instance;
        }

        public void StartScope(string tag)
        {
            //no scope
        }

        public void CloseScope()
        {
            //no scope
        }

        public void ClearCache()
        {
            ClassInstanceMap.Clear();
        }
    }
}