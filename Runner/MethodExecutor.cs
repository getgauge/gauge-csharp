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

using System.Collections.Generic;
using System.Diagnostics;
using Gauge.Messages;
using Google.Protobuf;
using NLog;
using Gauge.CSharp.Core;
using Gauge.CSharp.Runner.Models;
using Gauge.CSharp.Runner.Strategy;

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
        public ProtoExecutionResult Execute(GaugeMethod method, params string[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            var builder = new ProtoExecutionResult()
            {
                Failed = false,
            };
            var executionResult = _sandbox.ExecuteMethod(method, args);

            builder.ExecutionTime = stopwatch.ElapsedMilliseconds;
            if (!executionResult.Success)
            {
                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                builder.Failed = true;
                var isScreenShotEnabled = Utils.TryReadEnvValue("SCREENSHOT_ENABLED");
                if (isScreenShotEnabled == null || isScreenShotEnabled.ToLower() != "false")
                {
                    builder.ScreenShot = TakeScreenshot();
                }
                builder.ErrorMessage = executionResult.ExceptionMessage;
                builder.StackTrace = executionResult.StackTrace;
                builder.RecoverableError = executionResult.Recoverable;
                builder.ExecutionTime = elapsedMilliseconds;
            }
            return builder;
        }

        [DebuggerHidden]
        public ProtoExecutionResult ExecuteHooks(string hookType, HooksStrategy strategy, IList<string> applicableTags)
        {
            var stopwatch = Stopwatch.StartNew();
            var builder = new ProtoExecutionResult()
            {
                Failed = false
            };
            var executionResult = _sandbox.ExecuteHooks(hookType, strategy, applicableTags);

            builder.ExecutionTime = stopwatch.ElapsedMilliseconds;
            if (!executionResult.Success)
            {
                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                builder.Failed = true;
                var isScreenShotEnabled = Utils.TryReadEnvValue("SCREENSHOT_ENABLED");
                if (isScreenShotEnabled == null || isScreenShotEnabled.ToLower() != "false")
                {
                    builder.ScreenShot = TakeScreenshot();
                }
                builder.ErrorMessage = executionResult.ExceptionMessage;
                builder.StackTrace = executionResult.StackTrace;
                builder.RecoverableError = executionResult.Recoverable;
                builder.ExecutionTime = elapsedMilliseconds;
            }
            return builder;
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
            return _sandbox.TryScreenCapture(out screenShotBytes)
                ? ByteString.CopyFrom(screenShotBytes)
                : ByteString.Empty;
        }
    }
}