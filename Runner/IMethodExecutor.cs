using System.Collections.Generic;
using System.Reflection;
using main;

namespace Gauge.CSharp.Runner
{
    public interface IMethodExecutor
    {
        ProtoExecutionResult Execute(MethodInfo method, params object[] args);
        ProtoExecutionResult ExecuteHooks(IEnumerable<MethodInfo> methods, ExecutionInfo executionInfo);
    }
}