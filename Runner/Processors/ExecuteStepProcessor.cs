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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Runner.Converters;
using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public class ExecuteStepProcessor : ExecutionProcessor, IMessageProcessor
    {
        private readonly IStepRegistry _stepRegistry;
        private readonly IMethodExecutor _methodExecutor;
        private readonly ISandbox _sandbox;
        private Dictionary<string, IParamConverter> _paramConverters;


        public ExecuteStepProcessor(IStepRegistry stepRegistry, IMethodExecutor methodExecutor, ISandbox sandbox)
        {
            _stepRegistry = stepRegistry;
            _methodExecutor = methodExecutor;
            _sandbox = sandbox;
            InitializeConverter();
        }

        [DebuggerHidden]
        public Message Process(Message request)
        {
            var executeStepRequest = request.ExecuteStepRequest;
            if (!_stepRegistry.ContainsStep(executeStepRequest.ParsedStepText))
                return ExecutionError("Step Implementation not found", request);

            var method = _stepRegistry.MethodFor(executeStepRequest.ParsedStepText);

            var parameters = method.ParameterCount;
            var args = new List<KeyValuePair<string, string>>(parameters);
            var stepParameter = executeStepRequest.ParametersList;
            if (parameters != stepParameter.Count)
            {
                var argumentMismatchError = string.Format("Argument length mismatch for {0}. Actual Count: {1}, Expected Count: {2}",
                    executeStepRequest.ActualStepText,
                    stepParameter.Count, parameters);
                return ExecutionError(argumentMismatchError, request);
            }
            for (var i = 0; i < parameters; i++)
            {
                args.Add(stepParameter[i].ParameterType == Parameter.Types.ParameterType.Table
                    ? new KeyValuePair<string, string>(stepParameter[i].Value, "Table")
                    : new KeyValuePair<string, string>(stepParameter[i].Value, "String"));
            }
            var protoExecutionResult = ExecuteMethod(method, args.ToArray());
            return WrapInMessage(protoExecutionResult, request);
        }

        private void InitializeConverter()
        {
            _paramConverters = new Dictionary<string, IParamConverter>
            {
                {typeof (string).ToString(), new StringParamConverter()},
                {typeof (Table).ToString(), new TableParamConverter()}
            };
        }

        private static Message ExecutionError(string errorMessage, Message request)
        {
            var builder = ProtoExecutionResult.CreateBuilder().SetFailed(true)
                .SetErrorMessage(errorMessage)
                .SetRecoverableError(false)
                .SetExecutionTime(0);
            return WrapInMessage(builder.Build(), request);
        }

        [DebuggerHidden]
        private ProtoExecutionResult ExecuteMethod(GaugeMethod method, params KeyValuePair<string, string>[] args)
        {
            return _methodExecutor.Execute(method, args);
        }
    }
}