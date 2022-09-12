namespace Serilog.Sinks.ConditionalCachedForwarder
{
    using System;

    public interface ILoggingPermissionListener
    {
        event EventHandler<PermissionState> PermissionChangedEvent;
    }

    public enum PermissionState
    {
        Unknown,
        Denied,
        Granted
    }
}