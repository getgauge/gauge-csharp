using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Gauge.CSharp.Lib
{
    public class TcpClientWrapper : ITcpClientWrapper
    {
        readonly TcpClient _tcpClient = new TcpClient();
        public TcpClientWrapper(int port)
        {
            try
            {
                _tcpClient.Connect(new IPEndPoint(IPAddress.Loopback, port));
            }
            catch (Exception e)
            {
                throw new Exception("Could not connect", e);
            }
        }

        public bool Connected
        {
            get { return _tcpClient.Connected; }
        }

        public Stream GetStream()
        {
            return _tcpClient.GetStream();
        }

        public void Close()
        {
            _tcpClient.Close();
        }
    }
}