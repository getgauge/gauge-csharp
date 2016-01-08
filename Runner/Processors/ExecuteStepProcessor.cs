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
using Gauge.CSharp.Lib;
using Gauge.CSharp.Runner.Converters;
using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public class ExecuteStepProcessor : ExecutionProcessor, IMessageProcessor
    {
        private readonly IStepRegistry _stepRegistry;
        private readonly IMethodExecutor _methodExecutor;
        private Dictionary<string, IParamConverter> _paramConverters;


        public ExecuteStepProcessor(IStepRegistry stepRegistry, IMethodExecutor methodExecutor)
        {
            _stepRegistry = stepRegistry;
            _methodExecutor = methodExecutor;
            InitializeConverter();
        }

        [DebuggerHidden]
        public Message Process(Message request)
        {
            var executeStepRequest = request.ExecuteStepRequest;
            if (!_stepRegistry.ContainsStep(executeStepRequest.ParsedStepText))
                return ExecutionError("Step Implementation not found", request);

            var method = _stepRegistry.MethodFor(executeStepRequest.ParsedStepText);

            var parameters = method.GetParameters();
            var args = new Object[parameters.Length];
            var stepParameter = executeStepRequest.ParametersList;
            if (parameters.Length != stepParameter.Count)
            {
                var argumentMismatchError = String.Format("Argument length mismatch for {0}. Actual Count: {1}, Expected Count: {2}",
                    executeStepRequest.ActualStepText,
                    stepParameter.Count, parameters.Length);
                return ExecutionError(argumentMismatchError, request);
            }
            for (var i = 0; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;
                if (_paramConverters.ContainsKey(paramType.ToString()))
                {
                    args[i] = _paramConverters[paramType.ToString()].Convert(stepParameter[i]);
                }
                else
                {
                    args[i] = stepParameter[i].Value;
                }
            }
            var protoExecutionResult = ExecuteMethod(method, args);
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
        private ProtoExecutionResult ExecuteMethod(MethodInfo method, object[] args)
        {
            return _methodExecutor.Execute(method, args);
        }
    }
}