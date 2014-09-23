using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Google.ProtocolBuffers;

namespace Gauge.CSharp.Lib
{
    public abstract class AbstractGaugeConnection : IDisposable
    {
        protected readonly TcpClient TcpCilent;

        protected AbstractGaugeConnection(int port)
        {
            var tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(new IPEndPoint(IPAddress.Loopback, port));
            }
            catch (Exception e)
            {
                throw new Exception("Could not connect", e);
            }
            TcpCilent = tcpClient;
        }

        public bool Connected
        {
            get { return TcpCilent.Connected; }
        }

        public void WriteMessage(IMessageLite request)
        {
            var bytes = request.ToByteArray();
            var cos = CodedOutputStream.CreateInstance(TcpCilent.GetStream());
            cos.WriteRawVarint64((ulong) bytes.Length);
            cos.Flush();
            TcpCilent.GetStream().Write(bytes, 0, bytes.Length);
            TcpCilent.GetStream().Flush();
        }

        public IEnumerable<byte> ReadBytes()
        {
            var networkStream = TcpCilent.GetStream();
            var codedInputStream = CodedInputStream.CreateInstance(networkStream);
            var messageLength = codedInputStream.ReadRawVarint64();
            for (ulong i = 0; i < messageLength; i++)
            {
                yield return codedInputStream.ReadRawByte();
            }
        }

        protected static long GenerateMessageId()
        {
            return DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond;
        }

        public void Dispose()
        {
            TcpCilent.Close();
        }
    }
}