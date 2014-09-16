using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Google.ProtocolBuffers;
using main;

namespace testApplication
{
    
    class Program
    {
        static void Main(string[] args)
        {
            var port = System.Environment.GetEnvironmentVariable("GAUGE_INTERNAL_PORT");
            using (var tcpClient = new TcpClient())
            {
                tcpClient.Connect(new IPEndPoint(IPAddress.Loopback, Convert.ToInt32(port)));
                using (var networkStream = tcpClient.GetStream())
                {
                    while (tcpClient.Connected)
                    {
                        Console.Out.WriteLine("getting message");
                        var messageBytes = getBytes(networkStream);
                        var message = Message.ParseFrom(messageBytes);
                                                Console.Out.WriteLine(message.MessageType);
                                                Message responseMessage;

                        switch (message.MessageType)
                        {
                            case Message.Types.MessageType.StepNamesRequest:
                                responseMessage = getStepNamesResponseMessage();
                                break;
                            case Message.Types.MessageType.StepValidateRequest:
                                responseMessage = getStepValidateResponseMessage();
                                break;
                            default:
                                responseMessage = getResponseMessage();
                                break;
                        }

                        writeResponse(responseMessage, networkStream);
                    }
                }
            }
            
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
            var executionStatusResponseBuilder = ExecutionStatusResponse.CreateBuilder();
            var executionStatusResponse =
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
            var codedInputStream = CodedInputStream.CreateInstance(networkStream);
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
