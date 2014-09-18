using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace gauge_csharp
{
    internal class ClassInstanceManager
    {
        private static Hashtable classInstanceMap = new Hashtable();
        public static object get(Type declaringType)
        {
            if (classInstanceMap.ContainsKey(declaringType))
            {
                return classInstanceMap[declaringType];
            }
            object instance = Activator.CreateInstance(declaringType);
            classInstanceMap.Add(declaringType,instance);
            return instance;
        }
    }
}