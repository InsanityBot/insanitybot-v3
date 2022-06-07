namespace InsanityBot;

using System;
using System.Reflection;
using System.Threading.Tasks;

using InsanityBot.Extensions.Datafixers;
using InsanityBot.Extensions.Permissions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Starnight.Internal.Rest;

public static partial class Program
{
    public static async Task Main(String[] argv)
    {
        IHostBuilder hostBuilder = Host
            .CreateDefaultBuilder(argv)
            .UseConsoleLifetime();

        hostBuilder.addInsanityBotLogging();

        String token = "";
        PermissionServiceType selectedPermissionService = PermissionServiceType.Default;

        hostBuilder.ConfigureServices(async services =>
        {
            DataFixerUpper dataFixerUpper = new();
            await dataFixerUpper.DiscoverDatafixers(services.BuildServiceProvider(), Assembly.GetExecutingAssembly());

            services.AddSingleton<IDatafixerService>(dataFixerUpper);
        });

        // todo: load and register configs

        // register starnight
        hostBuilder.ConfigureServices(services =>
        {
            services.AddMemoryCache();
            services.AddStarnightRestClient(new RestClientOptions()
            {
                MedianFirstRequestRetryDelay = TimeSpan.FromSeconds(0.25),
                RatelimitedRetryCount = 25,
                RetryCount = 25,
                Token = token
            });
        });

        // register further InsanityBot extensions
        hostBuilder.ConfigureServices(services =>
        {
            services.AddPermissionServices(selectedPermissionService);
        });

        await hostBuilder.Build().StartAsync();
    }
}
