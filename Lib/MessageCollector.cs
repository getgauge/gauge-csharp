using System.Collections.Generic;

namespace Gauge.CSharp.Lib
{
    public class MessageCollector
    {
        public static List<string> GetAllPendingMessages()
        {
            var pendingMessages = new List<string>(GaugeMessages.Messages);
            GaugeMessages.Messages.Clear();
            return pendingMessages;
        } 
    }
}