# Serilog.Sink.ConditionalCachedForwarder

The goal of this sink is to provide an intermediate sink that can be activated or deactivated if needed.
It uses a three state approach:

- Allowed (Forwarding logs to the connected sinks)
- Denied (Discarding logs)
- Unknown (Caching the logs until the state is either Allowed or Denied)

An example use case would be when sending logs to the server that needs confirmation by the user.
If the user allows it then we send everything (also cached until this point) and if the user denies it then the sink discards all the logs.

## Usage

```csharp
ILoggingPermissionListener myPermissionSource = ...; // Interface for permission events

var Logger = new LoggerConfiguration()
    .WithConditionalCachingSink(myPermissionSource)
        .AddSink(new MyOnlineLogger(...))           // add sinks to the conditional sink
        .Build()
    .WriteTo.Sink<MyConsoleLogger>()                // always log to console
    .CreateLogger();
```

## Building / Test locally

Run the following commands from inside the `.cake` directory.

- Make sure the Cake build tool is installed:  
    `dotnet tool restore`
- Running tests:  
    `dotnet cake .\build.cake --target=Test`
- Building:  
    `dotnet cake .\build.cake --target=Build --configuration=Debug`
- Publishing:  
    `dotnet cake .\build.cake --target=Publish`

## Notes

Feedback and Pull Requests are generally welcome!
Please note that this package tries to be as lightweight as possible, so adding new external dependencies will not be accepted.
In case you need it for your use case - fork it.
