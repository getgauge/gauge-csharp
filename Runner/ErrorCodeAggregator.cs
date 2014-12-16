using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace Gauge.CSharp.Runner
{
    public class ErrorCodeAggregator : ILogger
    {
        public ErrorCodeAggregator()
        {
            ErrorCodes = new List<string>();
        }

        public List<string> ErrorCodes { get; set; }

        public void Initialize(IEventSource eventSource)
        {
            eventSource.ErrorRaised += (sender, args) => ErrorCodes.Add(args.Code);
        }

        public void Shutdown()
        {
        }

        public LoggerVerbosity Verbosity { get; set; }
        public string Parameters { get; set; }
    }
}