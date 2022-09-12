namespace Serilog.Sinks.ConditionalCachedForwarder.Tests
{
    using System;
    using NUnit.Framework;
    using System.Threading.Tasks;
    using Serilog.Events;

    [TestFixture]
    public class CachingSinkTests
    {
        private const string Log1 = "log1";
        private const string Log2 = "log2";
        private const string Log3 = "log3";
        private const string Log4 = "log4";
        private const string Log5 = "log5";

        [Test]
        public async Task LogsEmittedAfterPermissionGranted()
        {
            // Arrange
            var permissionTrigger = new TestPermissionTrigger();

            var outputSink = new TestableOutputSink();

            var cachingSink = new ConditionalCachingSink(permissionTrigger);
            cachingSink.AddSink(outputSink);

            using (var logger = new LoggerConfiguration()
                               .WriteTo.Sink(cachingSink)
                               .CreateLogger())
            {
                // Act
                // At start the permission should be unknown
                Assert.AreEqual(PermissionState.Unknown, cachingSink._permissionState);

                logger.Write(LogEventLevel.Warning, Log1);
                logger.Write(LogEventLevel.Warning, Log2);
                logger.Write(LogEventLevel.Warning, Log3);
                ;

                await Task.Delay(TimeSpan.FromSeconds(0.5f));

                Assert.AreEqual(3, cachingSink._logStorage.Count());
                Assert.AreEqual(0, outputSink.Logs.Count);

                permissionTrigger.ChangePermission(PermissionState.Denied);

                Assert.AreEqual(0, cachingSink._logStorage.Count());
                Assert.AreEqual(0, outputSink.Logs.Count);

                permissionTrigger.ChangePermission(PermissionState.Granted);

                logger.Write(LogEventLevel.Warning, Log4);
                logger.Write(LogEventLevel.Warning, Log5);

                await Task.Delay(TimeSpan.FromSeconds(0.5f));

                Assert.AreEqual(0, cachingSink._logStorage.Count());
                Assert.AreEqual(2, outputSink.Logs.Count);
            }
        }
    }
}