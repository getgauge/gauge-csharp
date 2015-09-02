using System.Collections.Generic;

namespace Gauge.CSharp.Lib
{
    public class GaugeMessages
    {
        internal static List<string> Messages = new List<string>();

        public static void WriteMessage(string message)
        {
            Messages.Add(message);
        }

        public static void WriteMessage(string message, params string[] args)
        {
            Messages.Add(string.Format(message, args));
        }
    }
}