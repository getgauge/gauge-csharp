namespace Gauge.CSharp.Runner.Communication
{
    public class GaugeConnection : AbstractGaugeConnection
    {
        public GaugeConnection(ITcpClientWrapper tcpClientWrapper) : base(tcpClientWrapper)
        {
        }
    }
}