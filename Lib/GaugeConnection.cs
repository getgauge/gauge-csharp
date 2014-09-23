using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Xml;
using Google.ProtocolBuffers;

namespace Gauge.CSharp.Lib
{
    public class GaugeConnection : AbstractGaugeConnection
    {
        public GaugeConnection(int port) : base(port)
        {
        }

        public byte[] ReadFromStream()
        {
            var networkStream = TcpCilent.GetStream();
            var codedInputStream = CodedInputStream.CreateInstance(networkStream);
            var messageLength = codedInputStream.ReadRawVarint64();
            var bytes = new List<byte>();
            for (ulong i = 0; i < messageLength; i++)
            {
                bytes.Add(codedInputStream.ReadRawByte());
            }
            return bytes.ToArray();
        }

        public void WriteResponse(IMessageLite responseMessage)
        {
            var networkStream = TcpCilent.GetStream();
            var byteArray = responseMessage.ToByteArray();
            var cos = CodedOutputStream.CreateInstance(networkStream);
            cos.WriteRawVarint64((ulong)byteArray.Length);
            cos.Flush();
            networkStream.Write(byteArray, 0, byteArray.Length);
            networkStream.Flush();
        }
    }
}