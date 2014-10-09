using System;
using System.Collections.Generic;
using System.Reflection;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Runner.Converters;
using main;

namespace Gauge.CSharp.Runner.Processors
{
    public class ExecuteStepProcessor : ExecutionProcessor, IMessageProcessor
    {
        private readonly IStepRegistry _stepRegistry;
        private readonly IMethodExecutor _methodExecutor;
        private Dictionary<Type, IParamConverter> _paramConverters;


        public ExecuteStepProcessor(IStepRegistry stepRegistry, IMethodExecutor methodExecutor)
        {
            _stepRegistry = stepRegistry;
            _methodExecutor = methodExecutor;
            InitializeConverter();
        }

        public ExecuteStepProcessor(IStepRegistry stepRegistry) : this(stepRegistry, new MethodExecutor())
        {
        }
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
                if (_paramConverters.ContainsKey(paramType))
                {
                    args[i] = _paramConverters[paramType].Convert(stepParameter[i]);
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
            _paramConverters = new Dictionary<Type, IParamConverter>
            {
                {typeof (string), new StringParamConverter()},
                {typeof (Table), new TableParamConverter()}
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

        private ProtoExecutionResult ExecuteMethod(MethodInfo method, object[] args)
        {
            return _methodExecutor.Execute(method, args);
        }
    }
}