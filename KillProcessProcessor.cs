using System;
using main;

namespace gauge_csharp
{
    internal class KillProcessProcessor:IMessageProcessor
    {
        public Message Process(Message request)
        {
            return request;
        }
    }
}