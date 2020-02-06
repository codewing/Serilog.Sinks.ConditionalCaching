namespace Serilog.Sink.ConditionalCachedForwarder
{
    using Serilog.Core;
    using Serilog.Events;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    
    public class ConditionalCachingSink : ILogEventSink, IDisposable
    {
        internal readonly ILogStorage _logStorage;
        private readonly List<ILogEventSink> _sinks;
        private readonly ILoggingPermissionListener _permissionListener;

        internal PermissionState _permissionState;
        private CancellationTokenSource _loggerTokenSource;

        private LoggerConfiguration _loggerConfiguration;

        public ConditionalCachingSink(ILoggingPermissionListener permissionListener) : this(
            new QueueLogStorage(), permissionListener)
        {}

        public ConditionalCachingSink(ILogStorage logStorage, ILoggingPermissionListener permissionListener)
        {
            _logStorage = logStorage;
            _sinks = new List<ILogEventSink>();
            _permissionListener = permissionListener ?? throw new ArgumentNullException(nameof(permissionListener));
            _permissionListener.PermissionChangedEvent += OnPermissionListenerChanged;
            _permissionState = PermissionState.Unknown;
        }

        public void SetLoggerConfiguration(LoggerConfiguration loggerConfiguration)
        {
            _loggerConfiguration = loggerConfiguration;
        }

        public LoggerConfiguration Build()
        {
            return _loggerConfiguration;
        }

        public ConditionalCachingSink AddSink(ILogEventSink sink)
        {
            _sinks.Add(sink);
            return this;
        }

        public void Emit(LogEvent logEvent)
        {
            if (_permissionState == PermissionState.Denied)
            {
                return;
            }

            _logStorage.Store(logEvent);
        }

        public void Dispose()
        {
            _permissionListener.PermissionChangedEvent -= OnPermissionListenerChanged;

            if (_sinks != null && _sinks.Any())
            {
                foreach (var sink in _sinks)
                {
                    if (sink is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }

                _sinks.Clear();
            }

            GC.SuppressFinalize(this);
        }

        private void EmitLog(LogEvent logEvent)
        {
            foreach (var sink in _sinks)
            {
                sink.Emit(logEvent);
            }
        }

        private void OnPermissionListenerChanged(object sender, PermissionState permissionState)
        {
            _permissionState = permissionState;
            switch (permissionState)
            {
                case PermissionState.Unknown:
                    StopForwardingLogs();
                    break;
                case PermissionState.Denied:
                    StopForwardingLogs();
                    _logStorage.Clear();
                    break;
                case PermissionState.Granted:
                    StartForwardingLogs();
                    break;
            }
        }

        private void StartForwardingLogs()
        {
            if (_loggerTokenSource != null) return;

            _loggerTokenSource = new CancellationTokenSource();
            Task.Run(() => ForwardLogs(_loggerTokenSource.Token), _loggerTokenSource.Token);
        }

        private async Task ForwardLogs(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_logStorage.TryRetrieve(out var logEvent))
                    {
                        System.Diagnostics.Debug.WriteLine($"Emitting log: {logEvent.MessageTemplate.Text}");
                        EmitLog(logEvent);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }

                while (!_logStorage.Any() && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
                }
            }
        }

        private void StopForwardingLogs()
        {
            if (_loggerTokenSource == null) return;

            _loggerTokenSource.Cancel();
            _loggerTokenSource.Dispose();
            _loggerTokenSource = null;
        }
    }
}