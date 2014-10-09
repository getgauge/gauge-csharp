using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Google.ProtocolBuffers;
using main;

namespace Gauge.CSharp.Runner
{
    public class MethodExecutor : IMethodExecutor
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
                var builder = ProtoExecutionResult.CreateBuilder().SetFailed(true);
                builder.SetScreenShot(TakeScreenshot());
                builder.SetErrorMessage(e.Message);
                builder.SetStackTrace(e.StackTrace);
                builder.SetRecoverableError(false);
                builder.SetExecutionTime(elapsedMilliseconds);
                return builder.Build();
            }
        }

        private static ByteString TakeScreenshot()
        {
            var bounds = Screen.GetBounds(Point.Empty);
            using (var bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }
                var memoryStream = new MemoryStream();
                bitmap.Save(memoryStream, ImageFormat.Png);
                var takeScreenshot = ByteString.CopyFrom(memoryStream.ToArray());
                return takeScreenshot;
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

        private static bool HasArguments(MethodInfo method, object[] args)
        {
            if (method.GetParameters().Length != args.Length)
            {
                return false;
            }
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].GetType() != method.GetParameters()[i].ParameterType)
                {
                    return false;
                }
            }
            return true;
        }
    }
}