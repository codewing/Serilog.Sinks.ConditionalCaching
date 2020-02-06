namespace Serilog.Sink.ConditionalCachedForwarder.Tests
{
    using System;

    public class TestPermissionTrigger : ILoggingPermissionListener
    {
        public void ChangePermission(PermissionState state)
        {
            PermissionChangedEvent?.Invoke(this, state);
        }

        public event EventHandler<PermissionState> PermissionChangedEvent;
    }
}