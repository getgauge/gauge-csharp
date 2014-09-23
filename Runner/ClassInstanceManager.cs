using System;
using System.Collections;

namespace Gauge.CSharp.Runner
{
    internal static class ClassInstanceManager
    {
        private static readonly Hashtable ClassInstanceMap = new Hashtable();

        public static object Get(Type declaringType)
        {
            if (ClassInstanceMap.ContainsKey(declaringType))
            {
                return ClassInstanceMap[declaringType];
            }
            object instance = Activator.CreateInstance(declaringType);
            ClassInstanceMap.Add(declaringType, instance);
            return instance;
        }
    }
}