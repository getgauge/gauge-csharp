using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        private const string GaugePortEnv = "GAUGE_INTERNAL_PORT";
        private const string GaugeApiPortEnv = "GAUGE_API_PORT";
        private const string GaugeProjectRoot = "GAUGE_PROJECT_ROOT";

        private static void Main(string[] args)
        {
            var port = ReadEnvValue(GaugePortEnv);
            var apiPort = ReadEnvValue(GaugeApiPortEnv);

            var apiConnection = new GaugeConnection(Convert.ToInt32(apiPort));
            var stepRegistry = new StepRegistry(ScanSteps(apiConnection));
            var messageDispacher = initializeMessageHandlers(stepRegistry);
            using (var tcpClient = new TcpClient())
            {
                tcpClient.Connect(new IPEndPoint(IPAddress.Loopback, Convert.ToInt32(port)));
                using (var networkStream = tcpClient.GetStream())
                {
                    ProcessTillDisconnect(tcpClient, networkStream, messageDispacher);
                }
            }
        }

        private static void ProcessTillDisconnect(TcpClient tcpClient, NetworkStream networkStream,
            IReadOnlyDictionary<Message.Types.MessageType, IMessageProcessor> messageDispacher)
        {
            while (tcpClient.Connected)
            {
                var messageBytes = ReadFromStream(networkStream);
                var message = Message.ParseFrom(messageBytes);
                if (messageDispacher.ContainsKey(message.MessageType))
                {
                    var response = messageDispacher[message.MessageType].Process(message);
                    WriteResponse(response, networkStream);
                    if (message.MessageType == Message.Types.MessageType.KillProcessRequest)
                    {
                        return;
                    }
                }
                else
                {
                    var response = new DefaultProcessor().Process(message);
                    WriteResponse(response, networkStream);
                }
            }
            
        }

        private static string ReadEnvValue(string env)
        {
            var port = Environment.GetEnvironmentVariable(env);
            if (string.IsNullOrEmpty(port))
            {
                throw new Exception(env + " is not set");
            }
            return port;
        }

        private static Dictionary<Message.Types.MessageType, IMessageProcessor> initializeMessageHandlers(
            StepRegistry stepRegistry)
        {
            var messageHandlers = new Dictionary<Message.Types.MessageType, IMessageProcessor>
            {
                {Message.Types.MessageType.ExecuteStep, new ExecuteStepProcessor(stepRegistry)},
                {Message.Types.MessageType.KillProcessRequest, new KillProcessProcessor()},
                {Message.Types.MessageType.StepNamesRequest, new StepNamesProcessor(stepRegistry)},
                {Message.Types.MessageType.StepValidateRequest, new StepValidationProcessor(stepRegistry)}
            };
            return messageHandlers;
        }

        private static Hashtable ScanSteps(GaugeConnection apiConnection)
        {
            var hashtable = new Hashtable();
            var enumerateFiles = Directory.EnumerateFiles(ReadEnvValue(GaugeProjectRoot), "*.dll", SearchOption.AllDirectories);
            foreach (var specAssembly in enumerateFiles)
            {
                var assembly = Assembly.LoadFile(specAssembly);
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var method in type.GetMethods())
                    {
                        var step = method.GetCustomAttributes<Step>(false);
                        foreach (var stepValue in step.SelectMany(s => apiConnection.GetStepValue(s.Names, false)))
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
            var byteArray = responseMessage.ToByteArray();
            var cos = CodedOutputStream.CreateInstance(networkStream);
            cos.WriteRawVarint64((ulong) byteArray.Length);
            cos.Flush();
            networkStream.Write(byteArray, 0, byteArray.Length);
            networkStream.Flush();
        }


        private static byte[] ReadFromStream(NetworkStream networkStream)
        {
            var codedInputStream = CodedInputStream.CreateInstance(networkStream);
            var messageLength = codedInputStream.ReadRawVarint64();
            var bytes = new List<byte>();
            for (ulong i = 0; i < messageLength; i++)
            {
                bytes.Add(codedInputStream.ReadRawByte());
            }
            return bytes.ToArray();
        }
    }
}