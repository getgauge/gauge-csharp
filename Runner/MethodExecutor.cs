﻿// Copyright 2015 ThoughtWorks, Inc.
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
using System.Linq;
using Gauge.CSharp.Core;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Runner.Models;
using Gauge.CSharp.Runner.Strategy;
using Gauge.Messages;
using Google.Protobuf;

namespace Gauge.CSharp.Runner
{
    public class MethodExecutor : IMethodExecutor
    {

        private readonly ISandbox _sandbox;

        public MethodExecutor(ISandbox sandbox)
        {
            _sandbox = sandbox;
        }

        [DebuggerHidden]
        public ProtoExecutionResult Execute(GaugeMethod method, params string[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            var builder = new ProtoExecutionResult
            {
                Failed = false
            };
            var executionResult = _sandbox.ExecuteMethod(method, args);

            builder.ExecutionTime = stopwatch.ElapsedMilliseconds;
            if (!executionResult.Success)
            {
                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                builder.Failed = true;
                var isScreenShotEnabled = Utils.TryReadEnvValue("SCREENSHOT_ON_FAILURE");
                if (isScreenShotEnabled == null || isScreenShotEnabled.ToLower() != "false")
                {
                    var screenshot = TakeScreenshot();
                    builder.ScreenShot = screenshot;
                    builder.FailureScreenshot = screenshot;
                }

                builder.ErrorMessage = executionResult.ExceptionMessage;
                builder.StackTrace = executionResult.StackTrace;
                builder.RecoverableError = executionResult.Recoverable;
                builder.ExecutionTime = elapsedMilliseconds;
            }

            var allPendingMessages = GetAllPendingMessages().Where(m => m != null);
            builder.Message.AddRange(allPendingMessages);
            var allPendingScreenshots = GetAllPendingScreenshots().Select(ByteString.CopyFrom);
            builder.Screenshots.AddRange(allPendingScreenshots);

            return builder;
        }

        [DebuggerHidden]
        public ProtoExecutionResult ExecuteHooks(string hookType, HooksStrategy strategy, IList<string> applicableTags,
            ExecutionContext executionContext)
        {
            var stopwatch = Stopwatch.StartNew();
            var builder = new ProtoExecutionResult
            {
                Failed = false
            };
            var executionResult = _sandbox.ExecuteHooks(hookType, strategy, applicableTags, executionContext);

            builder.ExecutionTime = stopwatch.ElapsedMilliseconds;
            if (!executionResult.Success)
            {
                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                builder.Failed = true;
                var isScreenShotEnabled = Utils.TryReadEnvValue("SCREENSHOT_ON_FAILURE");
                if (isScreenShotEnabled == null || isScreenShotEnabled.ToLower() != "false")
                {
                    var screenshot = TakeScreenshot();
                    builder.ScreenShot = screenshot;
                    builder.FailureScreenshot = screenshot;
                }

                builder.ErrorMessage = executionResult.ExceptionMessage;
                builder.StackTrace = executionResult.StackTrace;
                builder.RecoverableError = executionResult.Recoverable;
                builder.ExecutionTime = elapsedMilliseconds;
            }

            var allPendingMessages = GetAllPendingMessages().Where(m => m != null);
            builder.Message.AddRange(allPendingMessages);
            var allPendingScreenshots = GetAllPendingScreenshots().Select(ByteString.CopyFrom);
            builder.Screenshots.AddRange(allPendingScreenshots);

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

        public IEnumerable<byte[]> GetAllPendingScreenshots()
        {
            return _sandbox.GetAllPendingScreenshots();
        }

        private ByteString TakeScreenshot()
        {
            return _sandbox.TryScreenCapture(out var screenShotBytes)
                ? ByteString.CopyFrom(screenShotBytes)
                : ByteString.Empty;
        }
    }
}