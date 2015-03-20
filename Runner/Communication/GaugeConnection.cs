namespace Gauge.CSharp.Lib
{
    public class GaugeConnection : AbstractGaugeConnection
    {
        public GaugeConnection(ITcpClientWrapper tcpClientWrapper) : base(tcpClientWrapper)
        {
        }
    }
}