using main;

namespace Gauge.CSharp.Runner.Processors
{
    internal class KillProcessProcessor : IMessageProcessor
    {
        public Message Process(Message request)
        {
            return request;
        }
    }
}