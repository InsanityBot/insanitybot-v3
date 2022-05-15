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

    private readonly TimeSpan __sliding_expiration;

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

        __sliding_expiration = this.__configuration.Value<Boolean>("insanitybot.permissions.default.always_keep_default_loaded")
                ? TimeSpan.MaxValue
                : TimeSpan.Parse(this.__configuration.Value<String>("insanitybot.permissions.default.cache_expiration")!);

        this.loadAndCachePermissions();
    }

    public DefaultPermissions? GetDefaultPermissions(Boolean bypassCache = false)
    {
        if(!bypassCache && this.__cache.TryGetValue(
            CacheKeyHelper.GetDefaultPermissionKey(),
            out DefaultPermissions? permissions))
        {
            return permissions;
        }

        return loadAndCachePermissions();
    }

    public void WriteDefaultPermissions(DefaultPermissions permissions)
    {
        StreamWriter writer;

        if(!File.Exists("./data/permissions/default.json"))
        {
            writer = new(File.Create("./data/permissions/default.json"));
        }
        else
        {
            writer = new("./data/permissions/default.json");
        }

        writer.Write(JsonSerializer.Serialize(permissions, PermissionSerializationContexts.Default.DefaultPermissions));

        this.__cache.GetOrCreate(
            CacheKeyHelper.GetDefaultPermissionKey(),
            entry =>
            {
                entry.SlidingExpiration = this.__sliding_expiration;
                return permissions;
            });

        writer.Close();

        this.__logger?.LogDebug(LoggerEventIds.DefaultPermissionEdited, "Edited default permissions");
    }

    public DefaultPermissions CreateDefaultPermissions(PermissionManifest manifest)
    {
        DefaultPermissions permissions = new();

        foreach(PermissionManifestEntry entry in manifest.Manifest)
        {
            permissions.Permissions.Add(
                entry.Permission,
                entry.Value switch
                {
                    true => PermissionValue.Allowed,
                    false => PermissionValue.Denied
                });
        }

        this.WriteDefaultPermissions(permissions);

        this.__logger?.LogDebug(LoggerEventIds.DefaultPermissionCached, "Cached newly created default permissions");

        return permissions;
    }

    public DefaultPermissions UpdateDefaultPermissions(PermissionManifest manifest)
    {
        DefaultPermissions permissions = this.GetDefaultPermissions()!;

        if(permissions == null)
        {
            return this.CreateDefaultPermissions(manifest);
        }

        foreach(PermissionManifestEntry entry in manifest.Manifest)
        {
            if(!permissions.Permissions.ContainsKey(entry.Permission))
            {
                permissions.Permissions.Add(
                entry.Permission,
                entry.Value switch
                {
                    true => PermissionValue.Allowed,
                    false => PermissionValue.Denied
                });
            }
        }

        this.WriteDefaultPermissions(permissions);

        this.__logger?.LogDebug(LoggerEventIds.DefaultPermissionCached, "Cached newly created default permissions");

        return permissions;
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
            this.__cache.CreateEntry(CacheKeyHelper.GetDefaultPermissionKey())
                .SetValue(permissions)
                .SetSlidingExpiration(__sliding_expiration);

            this.__logger?.LogDebug(LoggerEventIds.DefaultPermissionCached, "Enregistered default permissions into cache.");
        }

        reader.Close();

        return permissions;
    }
}
