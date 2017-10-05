using System;
using System.Collections.Generic;
using System.Reflection;

namespace Gauge.CSharp.Lib
{
    public interface IClassInstanceManager
    {
        void Initialize(List<Assembly> assemblies);


        object Get(Type declaringType);


        void StartScope(string tag);


        void CloseScope();


        void ClearCache();
    }
}