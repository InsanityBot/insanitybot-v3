namespace InsanityBot.Extensions.Localization;

using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;

using InsanityBot.Extensions.Configuration;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

public class LocalizationService
{
    private readonly ILogger<LocalizationService> __logger;
    private readonly IMemoryCache __cache;
    private readonly HttpClient __client;
    private readonly IConfiguration __configuration;

    public LocalizationService
    (
        ILogger<LocalizationService> logger,
        IMemoryCache cache,
        HttpClient client,
        MainConfiguration configuration
    )
    {
        this.__logger = logger;
        this.__cache = cache;
        this.__client = client;
        this.__configuration = configuration;

        String invariantLocale = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>()
            .Where(xm => xm.Key == "invariant-localization")
            .FirstOrDefault()
            ?.Value ?? "en-us";

        this.__logger.LogDebug("Invariant locale: {locale}", invariantLocale);

        String defaultLocale = this.__configuration.Value<String>("insanitybot.localization.default_locale");

        this.__logger.LogDebug("Default locale: {locale}", defaultLocale);
    }
}
