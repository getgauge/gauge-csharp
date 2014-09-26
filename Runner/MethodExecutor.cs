using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using main;

namespace Gauge.CSharp.Runner
{
    internal class MethodExecutor
    {
        public ProtoExecutionResult Execute(MethodInfo method, params object[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var instance = ClassInstanceManager.Get(method.DeclaringType);
                method.Invoke(instance, args);
                return ProtoExecutionResult.CreateBuilder()
                    .SetFailed(false)
                    .SetExecutionTime(stopwatch.ElapsedMilliseconds)
                    .Build();
            }
            catch (Exception e)
            {
                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                //todo: add screenshot
                var builder = ProtoExecutionResult.CreateBuilder().SetFailed(true);
                builder.SetErrorMessage(e.Message);
                builder.SetStackTrace(e.StackTrace);
                builder.SetRecoverableError(false);
                builder.SetExecutionTime(elapsedMilliseconds);
                return builder.Build();
            }
        }

        public ProtoExecutionResult ExecuteHooks(IEnumerable<MethodInfo> methods, ExecutionInfo executionInfo)
        {
            var stopwatch = Stopwatch.StartNew();
            foreach (var method in methods)
            {
                var executionResult = ExecuteHook(method, new object[] {executionInfo});
                if (executionResult.Failed)
                {
                    return ProtoExecutionResult.CreateBuilder(executionResult).SetExecutionTime(stopwatch.ElapsedMilliseconds).Build();
                }
            }
            return ProtoExecutionResult.CreateBuilder()
                .SetFailed(false)
                .SetExecutionTime(stopwatch.ElapsedMilliseconds)
                .Build();
        }

        private ProtoExecutionResult ExecuteHook(MethodInfo method, object[] objects)
        {
            return HasArguments(method, objects) ? Execute(method, objects) : Execute(method);
        }

        private bool HasArguments(MethodInfo method, object[] args)
        {
            if (method.GetParameters().Length != args.Length)
            {
                return false;
            }
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.GetType() != method.GetParameters()[i].ParameterType)
                {
                    return false;
                }
            }
            return true;
        }
    }
}