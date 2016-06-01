using System;


namespace Gauge.CSharp.Runner.InstanceManagement
{
    public interface IClassInstanceManager
    {

        object Get(Type declaringType);


        void StartScope(string tag);


        void CloseScope();


        void ClearCache();
    }
}
