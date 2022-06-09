namespace InsanityBot;

using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;

public static partial class Program
{
    public const String ConsoleLogFormat =
        "[{@t:yyyy-MM-dd HH:mm:ss.fff}] " +
        "[{@l}] " +
        "[{Coalesce(SourceContext, '<none>')}]: {@m}\n{@x}";

    public const String FileLogFormat =
        "[{@t:yyyy-MM-dd HH:mm:ss.fff}] " +
        "[{@l}] " +
        "[{Coalesce(SourceContext, '<none>')}]: {@m}\n{@x}";

    private static IHostBuilder addInsanityBotLogging(this IHostBuilder host)
    {
        host.ConfigureServices((context, services) =>
        {
            LoggerConfiguration config = new LoggerConfiguration()
                .Enrich.WithEnvironmentName()
                .Enrich.FromLogContext()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .WriteTo.Console(formatter: new ExpressionTemplate(ConsoleLogFormat, theme: LoggerTheme.Theme))
                .WriteTo.Map(e => $"{DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime):yyyy-MM-dd}",
                (v, cf) =>
                {
                    cf.File(
                    new ExpressionTemplate(FileLogFormat),
                    $"./logs/insanitybot-{v}.log",
                    // 32 megabytes
                    fileSizeLimitBytes: 33_554_432,
                    flushToDiskInterval: TimeSpan.FromMinutes(2.5),
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
