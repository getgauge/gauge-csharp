using main;

namespace Gauge.CSharp.Runner.Processors
{
    public interface IMessageProcessor
    {
        Message Process(Message request);
    }
}