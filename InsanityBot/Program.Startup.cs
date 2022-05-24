namespace InsanityBot;

using System;

using InsanityBot.Extensions.Permissions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Starnight.Internal.Rest;

public static partial class Program
{
    public static Int32 Main(String[] argv)
    {
        IHostBuilder hostBuilder = Host
            .CreateDefaultBuilder(argv)
            .UseConsoleLifetime();

        hostBuilder.addInsanityBotLogging();

        String token = "";
        PermissionServiceType selectedPermissionService = PermissionServiceType.Default;

        // todo: register datafixers
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

        return 0;
    }
}
