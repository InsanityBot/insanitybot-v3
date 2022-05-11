namespace InsanityBot.Extensions.Permissions;

using System;
using System.IO;
using System.Text.Json;

using InsanityBot.Extensions.Configuration;
using InsanityBot.Extensions.Permissions.Objects;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

public class DefaultPermissionService
{
    private readonly ILogger<IPermissionService>? __logger;
    private readonly IMemoryCache __cache;
    private readonly IConfiguration __configuration;

    public DefaultPermissionService
    (
        ILogger<IPermissionService>? logger,
        IMemoryCache cache,
        PermissionConfiguration configuration
    )
    {
        this.__logger = logger;
        this.__cache = cache;
        this.__configuration = configuration;

        if(this.__configuration.Value<Boolean>("insanitybot.permissions.default.always_preload_default"))
        {
            this.loadAndCachePermissions();
        }
    }

    public DefaultPermissions? GetDefaultPermissions(Boolean bypassCache = false)
    {
        if(!bypassCache && this.__cache.TryGetValue(
            CacheKeyHelper.GetDefaultPermissionKey(),
            out DefaultPermissions permissions))
        {
            return permissions;
        }

        return loadAndCachePermissions();
    }

    private DefaultPermissions? loadAndCachePermissions()
    {
        this.__logger?.LogDebug(LoggerEventIds.DefaultPermissionLoading, "Loading default permissions...");
        StreamReader reader = new("./data/permissions/default.json");

        DefaultPermissions? permissions;

        try
        {
            permissions = JsonSerializer.Deserialize(
                reader.ReadToEnd(),
                PermissionSerializationContexts.Default.DefaultPermissions);
        }
        catch(Exception ex)
        {
            this.__logger?.LogError(ex, "Failed to load default permissions.");
            permissions = null;
        }

        this.__logger?.LogDebug(
            LoggerEventIds.DefaultPermissionLoaded,
            "Loaded default permissions, format: {validity}",
            permissions is null ? "invalid" : "valid");

        if(permissions is not null &&
            this.__configuration.Value<Boolean>("insanitybot.permissions.default.cache_default"))
        {
            TimeSpan expiration = this.__configuration.Value<Boolean>("insanitybot.permissions.default.always_keep_default_loaded")
                ? TimeSpan.MaxValue
                : TimeSpan.Parse(this.__configuration.Value<String>("insanitybot.permissions.default.cache_expiration")!);

            this.__cache.CreateEntry(CacheKeyHelper.GetDefaultPermissionKey())
                .SetValue(permissions)
                .SetSlidingExpiration(expiration);

            this.__logger?.LogDebug(LoggerEventIds.DefaultPermissionCached, "Enregistered default permissions into cache.");
        }

        reader.Close();

        return permissions;
    }
}
