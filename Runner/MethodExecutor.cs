using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using Gauge.Messages;
using Google.ProtocolBuffers;

namespace Gauge.CSharp.Runner
{
    public class MethodExecutor : IMethodExecutor
    {
        private readonly ISandbox _sandbox;

        public MethodExecutor(ISandbox sandbox)
        {
            _sandbox = sandbox;
        }

        public MethodExecutor() : this(Sandbox.Instance)
        {
        }

        public ProtoExecutionResult Execute(MethodInfo method, params object[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                try
                {
                    _sandbox.ExecuteMethod(method, args);
                }
                catch (TargetInvocationException e)
                {
                    // Throw inner exception, which is the exception that matters to the user
                    // This is the exception that is thrown by the user's code
                    // and is fixable from the Step Implemented
                    ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                }
                
                return ProtoExecutionResult.CreateBuilder()
                    .SetFailed(false)
                    .SetExecutionTime(stopwatch.ElapsedMilliseconds)
                    .Build();
            }
            catch (Exception e)
            {
                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                var builder = ProtoExecutionResult.CreateBuilder().SetFailed(true);
                var isScreenShotEnabled = Environment.GetEnvironmentVariable("screenshot_enabled");
                if (isScreenShotEnabled == null || isScreenShotEnabled.ToLower() != "false")
                {
                    builder.SetScreenShot(TakeScreenshot());
                }
                builder.SetErrorMessage(e.Message);
                builder.SetStackTrace(e.StackTrace);
                builder.SetRecoverableError(false);
                builder.SetExecutionTime(elapsedMilliseconds);
                return builder.Build();
            }
        }

        private static ByteString TakeScreenshot()
        {
            try
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
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return ByteString.Empty;
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