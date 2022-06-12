namespace InsanityBot;

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using InsanityBot.Extensions.Configuration;
using InsanityBot.Extensions.Datafixers;
using InsanityBot.Extensions.Permissions;
using InsanityBot.Extensions.Timers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using Starnight.Internal.Rest;

public static partial class Program
{
    public static async Task Main(String[] argv)
    {
        String gitDesc = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>()
            .Where(xm => xm.Key == "git-tree-state")
            .First()
            .Value!;

        String version = Assembly.GetExecutingAssembly().GetName().Version!.ToString();

        Console.Title = $"InsanityBot {version}+{gitDesc}";

        IHostBuilder hostBuilder = Host
            .CreateDefaultBuilder(argv)
            .UseConsoleLifetime();

        hostBuilder.addInsanityBotLogging();

        PermissionServiceType selectedPermissionService = PermissionServiceType.Default;

        hostBuilder.ConfigureServices(async services =>
        {
            services.AddMemoryCache();
            services.AddHttpClient();
            services.AddSingleton<IDatafixerService, DataFixerUpper>();
            services.AddSingleton<TimerService>();

            IServiceProvider provider = services.BuildServiceProvider();

            // make sure this stays in this order
            DataFixerUpper dataFixerUpper = (DataFixerUpper)provider.GetRequiredService<IDatafixerService>();

            await dataFixerUpper.DiscoverDatafixers(services.BuildServiceProvider(), Assembly.GetExecutingAssembly());

            // we need the timer service to initialize early
            _ = provider.GetRequiredService<TimerService>();
        });

        String token = "";

        hostBuilder.ConfigureServices(services =>
        {
            services.AddSingleton<MainConfiguration>()
                .AddSingleton<PermissionConfiguration>()
                .AddSingleton<UnsafeConfiguration>();

            IServiceProvider provider = services.BuildServiceProvider();

            token = provider.GetRequiredService<MainConfiguration>().Token;

            // retrieve permission service type
            selectedPermissionService = (provider.GetRequiredService<MainConfiguration>() as IConfiguration)
                .Value<String>("insanitybot.extensions.permissions.permission_service") switch
            {
                "default" or "standard" => PermissionServiceType.Default,
                "fast" or "fast_unsafe" or "unsafe" => PermissionServiceType.FastUnsafe,
                _ => PermissionServiceType.Default
            };
        });

        // register starnight
        hostBuilder.ConfigureServices(services =>
        {
            try
            {
                services.AddStarnightRestClient(new RestClientOptions()
                {
                    MedianFirstRequestRetryDelay = TimeSpan.FromSeconds(0.25),
                    RatelimitedRetryCount = 25,
                    RetryCount = 25,
                    Token = token
                });
            }
            catch(Exception e)
            {
                Log.Logger.Error(e, "Failed to register Starnight rest client.");
                return;
            }
        }); 

        // register further InsanityBot extensions
        hostBuilder.ConfigureServices(services =>
        {
            services.AddPermissionServices(selectedPermissionService);
        });

        IHost host = hostBuilder.Build();

        await host.StartAsync();

        await host.WaitForShutdownAsync();
    }
}
