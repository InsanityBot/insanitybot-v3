namespace InsanityBot;

using System;
using System.Reflection;
using System.Threading.Tasks;

using InsanityBot.Extensions.Configuration;
using InsanityBot.Extensions.Datafixers;
using InsanityBot.Extensions.Permissions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using Starnight.Internal.Rest;

public static partial class Program
{
    public static async Task Main(String[] argv)
    {
        IHostBuilder hostBuilder = Host
            .CreateDefaultBuilder(argv)
            .UseConsoleLifetime();

        hostBuilder.addInsanityBotLogging();

        PermissionServiceType selectedPermissionService = PermissionServiceType.Default;

        hostBuilder.ConfigureServices(async services =>
        {
            services.AddMemoryCache();
            services.AddSingleton<IDatafixerService, DataFixerUpper>();

            IServiceProvider provider = services.BuildServiceProvider();

            DataFixerUpper dataFixerUpper = (DataFixerUpper)provider.GetRequiredService<IDatafixerService>();

            await dataFixerUpper.DiscoverDatafixers(services.BuildServiceProvider(), Assembly.GetExecutingAssembly());
        });

        String token = "";

        hostBuilder.ConfigureServices(services =>
        {
            services.AddSingleton<MainConfiguration>()
                .AddSingleton<PermissionConfiguration>()
                .AddSingleton<UnsafeConfiguration>();

            token = services.BuildServiceProvider().GetRequiredService<MainConfiguration>().Token;
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
