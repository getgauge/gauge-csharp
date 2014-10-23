using main;

namespace Gauge.CSharp.Runner.Processors
{
    public class DataStoreInitProcessor : IMessageProcessor
    {
        public Message Process(Message request)
        {
            DataStoreFactory.GetDataStoreFor(request.MessageType).Initialize();
            
            //send back a default response?
            return new DefaultProcessor().Process(request);
        }
    }
}