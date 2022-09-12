namespace Serilog.Sinks.ConditionalCachedForwarder
{
    using System.Collections.Concurrent;
    using Serilog.Events;

    public class QueueLogStorage : ILogStorage
    {
        private ConcurrentQueue<LogEvent> _logCollection = new ConcurrentQueue<LogEvent>();

        public void Store(LogEvent log)
        {
            _logCollection.Enqueue(log);
        }

        public int Count()
        {
            return _logCollection.Count;
        }

        public bool TryRetrieve(out LogEvent log)
        {
            return _logCollection.TryDequeue(out log);
        }

        public bool Any()
        {
            return !_logCollection.IsEmpty;
        }

        public void Clear()
        {
            #if NET471
            while (!_logCollection.IsEmpty)
            {
                _logCollection.TryDequeue(out _);
            }
            #elif NETSTANDARD2_1_OR_GREATER
            _logCollection.Clear();
            #else
            #error unsupported platform
            #endif
        }
    }
}