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

namespace gauge_csharp
{
    internal static class Program
    {
        const string GAUGE_PORT_ENV = "GAUGE_INTERNAL_PORT";
        const string GAUGE_API_PORT_ENV = "GAUGE_API_PORT";


        private static void Main(string[] args)
        {
            var port = readEnvValue(GAUGE_PORT_ENV);
            string apiPort = readEnvValue(GAUGE_API_PORT_ENV);

            var apiConnection = new GaugeConnection(Convert.ToInt32(apiPort));
            var stepRegistry = new StepRegistry(ScanSteps(apiConnection));
            Dictionary<Message.Types.MessageType, IMessageProcessor> messageDispacher =
                initializeMessageHandlers(stepRegistry);
            using (var tcpClient = new TcpClient())
            {
                tcpClient.Connect(new IPEndPoint(IPAddress.Loopback, Convert.ToInt32(port)));
                using (NetworkStream networkStream = tcpClient.GetStream())
                {
                    keepProcessing(tcpClient, networkStream, messageDispacher);
                }
            }
        }

        private static void keepProcessing(TcpClient tcpClient, NetworkStream networkStream, Dictionary<Message.Types.MessageType, IMessageProcessor> messageDispacher)
        {
            while (tcpClient.Connected)
            {
                byte[] messageBytes = ReadFromStream(networkStream);
                Message message = Message.ParseFrom(messageBytes);
                if (messageDispacher.ContainsKey(message.MessageType))
                {
                    Message response = messageDispacher[message.MessageType].Process(message);
                    WriteResponse(response, networkStream);
                    if (message.MessageType == Message.Types.MessageType.KillProcessRequest)
                    {
                        return;
                    }
                }
                else
                {
                    Message response = new DefaultProcessor().Process(message);
                    WriteResponse(response,networkStream);
                }
            }
        }

        private static string readEnvValue(string env)
        {
            string port = Environment.GetEnvironmentVariable(env);
            if (string.IsNullOrEmpty(port))
            {
                throw new Exception(env + " is not set");
            }
            return port;
        }

        private static Dictionary<Message.Types.MessageType, IMessageProcessor> initializeMessageHandlers(
            StepRegistry stepRegistry)
        {
            var messageHandlers = new Dictionary<Message.Types.MessageType, IMessageProcessor>();
            messageHandlers.Add(Message.Types.MessageType.ExecuteStep, new ExecuteStepProcessor(stepRegistry));
            messageHandlers.Add(Message.Types.MessageType.KillProcessRequest, new KillProcessProcessor());
            messageHandlers.Add(Message.Types.MessageType.StepNamesRequest, new StepNamesProcessor(stepRegistry));
            messageHandlers.Add(Message.Types.MessageType.StepValidateRequest, new StepValidationProcessor(stepRegistry));
            return messageHandlers;
        }

        private static Hashtable ScanSteps(GaugeConnection apiConnection)
        {
            var hashtable = new Hashtable();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    foreach (MethodInfo method in type.GetMethods())
                    {
                        IEnumerable<Step> step = method.GetCustomAttributes(false).OfType<Step>();
                        foreach (string stepValue in step.Select(s => apiConnection.getStepValue(s.Name, false)))
                        {
                            hashtable.Add(stepValue, method);
                        }
                    }
                }
            }
            return hashtable;
        }


        private static void WriteResponse(Message responseMessage, NetworkStream networkStream)
        {
            byte[] byteArray = responseMessage.ToByteArray();
            CodedOutputStream cos = CodedOutputStream.CreateInstance(networkStream);
            cos.WriteRawVarint64((ulong) byteArray.Length);
            cos.Flush();
            networkStream.Write(byteArray, 0, byteArray.Length);
            networkStream.Flush();
        }


        private static byte[] ReadFromStream(NetworkStream networkStream)
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