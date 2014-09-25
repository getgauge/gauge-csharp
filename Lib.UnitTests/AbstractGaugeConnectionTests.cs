using System.Net.Sockets;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Lib.UnitTests
{
    [TestFixture]
    public class AbstractGaugeConnectionTests
    {
        [Test]
        public void ShouldBeConnected()
        {
            var client = new Mock<ITcpClientWrapper>();
            client.SetupGet(x => x.Connected).Returns(true);
            var testGaugeConnection = new TestGaugeConnection(client.Object);
            Assert.IsTrue(testGaugeConnection.Connected);
        }

        class TestGaugeConnection : AbstractGaugeConnection
        {
            public TestGaugeConnection(ITcpClientWrapper client) : base(client)
            {
            }
        }         
    }

}