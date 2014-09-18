using System;
using System.Diagnostics;
using System.Reflection;
using main;

namespace gauge_csharp
{
    internal class MethodExecutor
    {
        public ProtoExecutionResult execute(MethodInfo method, Object[] args)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                object instance = ClassInstanceManager.get(method.DeclaringType);
                method.Invoke(instance, args);
                return
                    ProtoExecutionResult.CreateBuilder()
                        .SetFailed(false)
                        .SetExecutionTime(stopwatch.ElapsedMilliseconds)
                        .Build();
            }
            catch (Exception e)
            {
                long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                //todo: add screenshot
                ProtoExecutionResult.Builder builder = ProtoExecutionResult.CreateBuilder().SetFailed(true);
                builder.SetErrorMessage(e.Message);
                builder.SetStackTrace(e.StackTrace);
                builder.SetRecoverableError(false);
                builder.SetExecutionTime(elapsedMilliseconds);
                return builder.Build();
            }
        }
    }
}