using System;
using System.Linq;
using Gauge.CSharp.Lib;
using main;

namespace Gauge.CSharp.Runner
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            using (var gaugeConnection = new GaugeConnection(new TcpClientWrapper(Utils.GaugePort)))
            {
                while (gaugeConnection.Connected)
                {
                    var messageBytes = gaugeConnection.ReadBytes();
                    var message = Message.ParseFrom(messageBytes.ToArray());
                    var processor = MessageProcessorFactory.GetProcessor(message.MessageType);
                    var response = processor.Process(message);
                    gaugeConnection.WriteMessage(response);
                    if (message.MessageType == Message.Types.MessageType.KillProcessRequest)
                    {
                        return;
                    }
                }
            }
        }
    }
}