namespace Serilog.Sinks.ConditionalCachedForwarder.Tests
{
    using System.Collections.Generic;
    using Serilog.Core;
    using Serilog.Events;
    
    public class TestableOutputSink : ILogEventSink
    {
        public LogEvent LastLog { get; private set; }
        public List<LogEvent> Logs { get; }

        public TestableOutputSink()
        {
            Logs = new List<LogEvent>();
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent != null)
            {
                LastLog = logEvent;
                Logs.Add(logEvent);
            }
        }
    }
}