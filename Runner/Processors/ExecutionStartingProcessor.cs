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

using System.Diagnostics;
using System.Threading;
using Gauge.CSharp.Core;
using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public class ExecutionStartingProcessor : HookExecutionProcessor
    {
        public ExecutionStartingProcessor(IMethodExecutor methodExecutor) : base(methodExecutor)
        {
        }

        protected override string HookType => "BeforeSuite";

        protected override ExecutionInfo GetExecutionInfo(Message request)
        {
            return request.ExecutionStartingRequest.CurrentExecutionInfo;
        }

        public override Message Process(Message request)
        {
            var debuggingEnv = Utils.TryReadEnvValue("DEBUGGING");
            if (debuggingEnv != null && debuggingEnv.ToLower().Equals("true"))
            {
                // if the runner is launched in DEBUG mode, let the debugger attach.
                var j = 0;
                while (!Debugger.IsAttached)
                {
                    j++;
                    //Trying to debug, wait for a debugger to attach
                    Thread.Sleep(100);
                    //Timeout, no debugger connected, break out into a normal execution.
                    if (j == 300)
                        break;
                }
            }
            return base.Process(request);
        }
    }
}