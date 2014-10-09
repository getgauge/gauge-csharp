using main;

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