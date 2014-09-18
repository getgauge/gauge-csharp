using System;
using System.Collections.Generic;
using System.Reflection;
using gauge_csharp_lib;
using main;

namespace gauge_csharp
{
    public class ExecuteStepProcessor : IMessageProcessor
    {
        private readonly StepRegistry _stepMethodHashtable;
        private Dictionary<Type, IParamConverter> _paramConverters;


        public ExecuteStepProcessor(StepRegistry stepMethodHashtable)
        {
            _stepMethodHashtable = stepMethodHashtable;
            initializeConverter();
        }

        public Message Process(Message request)
        {
            ExecuteStepRequest executeStepRequest = request.ExecuteStepRequest;
            if (_stepMethodHashtable.ContainsStep(executeStepRequest.ParsedStepText))
            {
                MethodInfo method = _stepMethodHashtable.MethodFor(executeStepRequest.ParsedStepText);

                ParameterInfo[] parameters = method.GetParameters();
                var args = new Object[parameters.Length];
                IList<Parameter> stepParameter = executeStepRequest.ParametersList;
                for (int i = 0; i < parameters.Length; i++)
                {
                    Type paramType = parameters[i].ParameterType;
                    if (_paramConverters.ContainsKey(paramType))
                    {
                        args[i] = _paramConverters[paramType].Convert(stepParameter[i]);
                    }
                    else
                    {
                        args[i] = stepParameter[i].Value;
                    }
                }
                ProtoExecutionResult protoExecutionResult = executeMethod(method, args);
                return wrapInMessage(protoExecutionResult, request);
            }
            return executionError("Step Implementation not found", request);
        }

        private void initializeConverter()
        {
            _paramConverters = new Dictionary<Type, IParamConverter>
            {
                {typeof (string), new StringParamConverter()},
                {typeof (Table), new TableParamConverter()}
            };
        }

        private Message executionError(string errorMessage, Message request)
        {
            ProtoExecutionResult.Builder builder = ProtoExecutionResult.CreateBuilder().SetFailed(true);
            builder.SetErrorMessage(errorMessage);
            builder.SetRecoverableError(false);
            builder.SetExecutionTime(0);
            return wrapInMessage(builder.Build(), request);
        }

        private ProtoExecutionResult executeMethod(MethodInfo method, object[] args)
        {
            var methodExecutor = new MethodExecutor();
            return methodExecutor.execute(method, args);
        }

        private Message wrapInMessage(ProtoExecutionResult executionResult, Message request)
        {
            ExecutionStatusResponse executionStatusResponse =
                ExecutionStatusResponse.CreateBuilder().SetExecutionResult(executionResult).Build();
            return
                Message.CreateBuilder()
                    .SetMessageId(request.MessageId)
                    .SetMessageType(Message.Types.MessageType.ExecutionStatusResponse)
                    .SetExecutionStatusResponse(executionStatusResponse)
                    .Build();
        }
    }
}