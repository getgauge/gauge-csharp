using System.Reflection;

namespace Gauge.CSharp.Runner
{
    public interface ISandbox
    {
        void ExecuteMethod(MethodInfo method, params object[] args);
    }
}