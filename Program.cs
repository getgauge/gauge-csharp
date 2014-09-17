using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using gauge_csharp_lib;
using Google.ProtocolBuffers;
using main;

namespace testApplication
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string port = Environment.GetEnvironmentVariable("GAUGE_INTERNAL_PORT");
            string apiPort = Environment.GetEnvironmentVariable("GAUGE_API_PORT");
            var apiConnection = new GaugeConnection(Convert.ToInt32(apiPort));
            Hashtable stepMethodTable = scanSteps(apiConnection);
            using (var tcpClient = new TcpClient())
            {
                tcpClient.Connect(new IPEndPoint(IPAddress.Loopback, Convert.ToInt32(port)));
                using (NetworkStream networkStream = tcpClient.GetStream())
                {
                    while (tcpClient.Connected)
                    {
                        byte[] messageBytes = getBytes(networkStream);
                        Message message = Message.ParseFrom(messageBytes);
                        Message responseMessage;

                        switch (message.MessageType)
                        {
                            case Message.Types.MessageType.StepNamesRequest:
                                responseMessage = getStepNamesResponseMessage();
                                break;
                            case Message.Types.MessageType.StepValidateRequest:
                                responseMessage = getStepValidateResponseMessage();
                                break;
                            case Message.Types.MessageType.KillProcessRequest:
                                return;
                            case Message.Types.MessageType.ExecuteStep:
                                responseMessage = excuteStep(message.ExecuteStepRequest, stepMethodTable);
                                break;
                            default:
                                responseMessage = getResponseMessage();
                                break;
                        }

                        writeResponse(responseMessage, networkStream);
                    }
                }
            }
            Console.ReadLine();
        }

        private static Message excuteStep(ExecuteStepRequest executeStepRequest, Hashtable stepMethodDictionary)
        {
            string parsedStepText = executeStepRequest.ParsedStepText;
            Console.Out.WriteLine("executing step {0}",parsedStepText);
            if (stepMethodDictionary.ContainsKey(parsedStepText))
            {
                Console.Out.WriteLine("method was found");
                var stepImpl = (MethodInfo) stepMethodDictionary[parsedStepText];
                object instance = Activator.CreateInstance(stepImpl.DeclaringType);
                stepImpl.Invoke(instance, null);
            }
            return getResponseMessage();
        }

        private static Hashtable scanSteps(GaugeConnection apiConnection)
        {
            var hashtable = new Hashtable();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    foreach (MethodInfo method in type.GetMethods())
                    {
                        IEnumerable<Step> step = method.GetCustomAttributes(false).OfType<Step>();
                        foreach (Step s in step)
                        {
                            string stepValue = apiConnection.getStepValue(s.Name, false);
                            Console.Out.WriteLine("adding {0}",stepValue);
                            hashtable.Add(stepValue, method);
                        }
                    }
                }
            }
            return hashtable;
        }

        private static Message getStepValidateResponseMessage()
        {
            StepValidateResponse stepValidateResponse = StepValidateResponse.CreateBuilder().SetIsValid(true).Build();
            return
                Message.CreateBuilder()
                    .SetMessageId(1)
                    .SetMessageType(Message.Types.MessageType.StepValidateResponse)
                    .SetStepValidateResponse(stepValidateResponse)
                    .Build();
        }

        private static Message getStepNamesResponseMessage()
        {
            StepNamesResponse stepNamesResponse = StepNamesResponse.CreateBuilder().AddSteps("foo").Build();
            return Message.CreateBuilder()
                .SetMessageId(1)
                .SetMessageType(Message.Types.MessageType.StepNamesResponse)
                .SetStepNamesResponse(stepNamesResponse).Build();
        }

        private static void writeResponse(Message responseMessage, NetworkStream networkStream)
        {
            byte[] byteArray = responseMessage.ToByteArray();
            CodedOutputStream cos = CodedOutputStream.CreateInstance(networkStream);
            cos.WriteRawVarint64((ulong) byteArray.Length);
            cos.Flush();
            networkStream.Write(byteArray, 0, byteArray.Length);
            networkStream.Flush();
        }

        private static Message getResponseMessage()
        {
            ExecutionStatusResponse.Builder executionStatusResponseBuilder = ExecutionStatusResponse.CreateBuilder();
            ExecutionStatusResponse executionStatusResponse =
                executionStatusResponseBuilder.SetExecutionResult(
                    ProtoExecutionResult.CreateBuilder().SetFailed(false).SetExecutionTime(0)).Build();
            return Message.CreateBuilder()
                .SetMessageId(1)
                .SetMessageType(Message.Types.MessageType.ExecutionStatusResponse)
                .SetExecutionStatusResponse(executionStatusResponse)
                .Build();
        }

        private static byte[] getBytes(NetworkStream networkStream)
        {
            CodedInputStream codedInputStream = CodedInputStream.CreateInstance(networkStream);
            ulong messageLength = codedInputStream.ReadRawVarint64();
            var bytes = new List<byte>();
            for (ulong i = 0; i < messageLength; i++)
            {
                bytes.Add(codedInputStream.ReadRawByte());
            }
            return bytes.ToArray();
        }
    }
}