using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Google.ProtocolBuffers;

namespace Gauge.CSharp.Lib
{
    public abstract class AbstractGaugeConnection : IDisposable
    {
        protected readonly ITcpClientWrapper TcpClientWrapper;

        protected AbstractGaugeConnection(ITcpClientWrapper tcpClientWrapper)
        {
            TcpClientWrapper = tcpClientWrapper;
        }

        public bool Connected
        {
            get { return TcpClientWrapper.Connected; }
        }

        public void WriteMessage(IMessageLite request)
        {
            var bytes = request.ToByteArray();
            var cos = CodedOutputStream.CreateInstance(TcpClientWrapper.GetStream());
            cos.WriteRawVarint64((ulong) bytes.Length);
            cos.Flush();
            TcpClientWrapper.GetStream().Write(bytes, 0, bytes.Length);
            TcpClientWrapper.GetStream().Flush();
        }

        public IEnumerable<byte> ReadBytes()
        {
            var networkStream = TcpClientWrapper.GetStream();
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
            TcpClientWrapper.Close();
        }
    }
}