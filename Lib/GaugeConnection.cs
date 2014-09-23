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
    }
}