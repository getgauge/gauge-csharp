using main;

namespace gauge_csharp
{
    public interface IMessageProcessor
    {
        Message Process(Message request);
    }
}