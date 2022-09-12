namespace Serilog.Sinks.ConditionalCachedForwarder
{
    using Events;

    public interface ILogStorage
    {
        void Store(LogEvent log);
        bool TryRetrieve(out LogEvent log);
        void Clear();
        int Count();
        bool Any();
    }
}