namespace InsanityBot;

using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Events;
using Serilog.Sinks.SpectreConsole;

public static partial class Program
{
    public const String ConsoleLogFormat =
        "[[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}]] " +
        "[[{Level}]]" +
        "{#if EventId is not null} [#FDEFB2][[{EventId.Name}:{EventId.Id}]][/]{#end}" +
        "[{SourceContext}]: {Message}{NewLine}{Exception}";

    public const String FileLogFormat =
        "[[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}]] " +
        "[[{Level}]]" +
        "{#if EventId is not null} [[{EventId.Name}:{EventId.Id}]]{#end}" +
        "[{SourceContext}]: {Message}{NewLine}{Exception}";

    private static IHostBuilder addInsanityBotLogging(this IHostBuilder host)
    {
        host.ConfigureServices((context, services) =>
        {
            LoggerConfiguration config = new LoggerConfiguration()
                .Enrich.WithEnvironmentName()
                .Enrich.FromLogContext()
                .WriteTo.SpectreConsole(ConsoleLogFormat,
#if DEBUG
                LogEventLevel.Debug)
#else
                LogEventLevel.Information)
#endif
                .WriteTo.Map(e => $"{DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime):yyyy-MM-dd}",
                (v, cf) =>
                {
                    cf.File($"./logs/insanitybot-{v}.log",
                    restrictedToMinimumLevel:
#if DEBUG
                    LogEventLevel.Debug,
#else
                    LogEvenLevel.Information,
#endif
                    outputTemplate: FileLogFormat,
                    // 32 megabytes
                    fileSizeLimitBytes: 33_554_432,
                    flushToDiskInterval: TimeSpan.FromMinutes(2.5),
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 50
                    );
                });

            Log.Logger = config.CreateLogger();

            services.AddLogging(l => l.ClearProviders().AddSerilog());
        });

        return host;
    }
}
