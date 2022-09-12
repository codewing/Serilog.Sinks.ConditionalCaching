namespace Serilog.Sinks.ConditionalCachedForwarder
{
    using Serilog.Configuration;
    
    public static class SinkExtensions
    {
        public static ConditionalCachingSink WithConditionalCachingSink(
            this LoggerSinkConfiguration loggerConfiguration, ILoggingPermissionListener permissionListener)
        {
            var sink = new ConditionalCachingSink(permissionListener);
            var config = loggerConfiguration.Sink(sink);
            sink.SetLoggerConfiguration(config);
            return sink;
        }
    }
}