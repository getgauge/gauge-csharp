using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public class KillProcessProcessor : IMessageProcessor
    {
        public Message Process(Message request)
        {
            return request;
        }
    }
}