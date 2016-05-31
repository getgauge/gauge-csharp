// Copyright 2015 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Gauge.CSharp.Runner.Converters;
using Gauge.Messages;
using Google.ProtocolBuffers;
using NLog;
using Gauge.CSharp.Core;

namespace Gauge.CSharp.Runner
{
    public class MethodExecutor : IMethodExecutor
    {
        private readonly ISandbox _sandbox;
        private static readonly Logger Logger = LogManager.GetLogger("Sandbox");

        public MethodExecutor(ISandbox sandbox)
        {
            _sandbox = sandbox;
        }

        [DebuggerHidden]
        public ProtoExecutionResult Execute(MethodInfo method, params object[] args)
        {
            Logger.Debug("Execution method: {0}.{1}", method.DeclaringType.FullName, method.Name);
            var stopwatch = Stopwatch.StartNew();
            var builder = ProtoExecutionResult.CreateBuilder().SetFailed(false);
            var executionResult = _sandbox.ExecuteMethod(method, StringParamConverter.TryConvertParams(method, args));
                
            builder.SetExecutionTime(stopwatch.ElapsedMilliseconds);
            if (!executionResult.Success)
            {
                Logger.Error("Error executing {0}.{1}", method.DeclaringType.FullName, method.Name);

                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                builder.SetFailed(true);
                var isScreenShotEnabled = Utils.TryReadEnvValue("SCREENSHOT_ENABLED");
                if (isScreenShotEnabled == null || isScreenShotEnabled.ToLower() != "false")
                {
                    builder.SetScreenShot(TakeScreenshot());
                }
                builder.SetErrorMessage(executionResult.ExceptionMessage);
                builder.SetStackTrace(executionResult.StackTrace);
                builder.SetRecoverableError(false);
                builder.SetExecutionTime(elapsedMilliseconds);
            }
            return builder.Build();
        }

        public void ClearCache()
        {
            _sandbox.ClearObjectCache();
        }

        public IEnumerable<string> GetAllPendingMessages()
        {
            return _sandbox.GetAllPendingMessages();
        }

        private ByteString TakeScreenshot()
        {
            byte[] screenShotBytes;
            return _sandbox.TryScreenCapture(out screenShotBytes) ? ByteString.CopyFrom(screenShotBytes) : ByteString.Empty;
        }

        [DebuggerHidden]
        public ProtoExecutionResult.Builder ExecuteHooks(IEnumerable<MethodInfo> methods, ExecutionInfo executionInfo)
        {
            var stopwatch = Stopwatch.StartNew();
            foreach (var method in methods)
            {
                var executionResult = ExecuteHook(method, new object[] {executionInfo});
                if (!executionResult.Failed) continue;

                Logger.Debug("Hook execution failed : {0}.{1}", method.DeclaringType.FullName, method.Name);
                return ProtoExecutionResult.CreateBuilder(executionResult)
                    .SetFailed(true)
                    .SetRecoverableError(false)
                    .SetErrorMessage(executionResult.ErrorMessage)
                    .SetScreenShot(executionResult.ScreenShot)
                    .SetStackTrace(executionResult.StackTrace)
                    .SetExecutionTime(stopwatch.ElapsedMilliseconds);
            }
            return ProtoExecutionResult.CreateBuilder()
                .SetFailed(false)
                .SetExecutionTime(stopwatch.ElapsedMilliseconds);
        }

        [DebuggerHidden]
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