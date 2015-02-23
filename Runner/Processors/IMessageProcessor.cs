using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public interface IMessageProcessor
    {
        Message Process(Message request);
    }
}