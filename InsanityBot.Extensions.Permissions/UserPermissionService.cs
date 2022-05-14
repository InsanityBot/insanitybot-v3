namespace InsanityBot.Extensions.Permissions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using InsanityBot.Extensions.Configuration;
using InsanityBot.Extensions.Permissions.Objects;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

public class UserPermissionService
{
    private readonly ILogger<IPermissionService>? __logger;
    private readonly IMemoryCache __cache;
    private readonly IConfiguration __configuration;

    private readonly DefaultPermissionService __defaults;

    private readonly TimeSpan __sliding_expiration;

    public UserPermissionService
    (
        ILogger<IPermissionService>? logger,
        IMemoryCache cache,
        PermissionConfiguration configuration,
        DefaultPermissionService defaults
    )
    {
        this.__logger = logger;
        this.__cache = cache;
        this.__configuration = configuration;
        this.__defaults = defaults;

        this.__sliding_expiration = TimeSpan.Parse(this.__configuration.Value<String>(
            "insanitybot.permissions.users.cache_expiration")!);
    }

    public UserPermissions? GetUserPermissions(UInt64 id, Boolean bypassCache = false)
    {
        if(!bypassCache && this.__cache.TryGetValue(CacheKeyHelper.GetUserPermissionKey(id), out UserPermissions permissions))
        {
            return permissions;
        }

        this.__logger?.LogTrace(LoggerEventIds.UserPermissionLoading, "Loading role permissions for user {id}...", id);

        if(File.Exists($"./data/user-{id}/permissions.json"))
        {
            StreamReader reader = new($"./data/user-{id}/permissions.json");

            try
            {
                permissions = JsonSerializer.Deserialize(reader.ReadToEnd(), PermissionSerializationContexts.Default.User)!;
                reader.Close();
            }
            catch(Exception e)
            {
                this.__logger?.LogError(e, "Failed to load role permission file for user {id}", id);
                reader.Close();
                return null;
            }

            this.__logger?.LogTrace(LoggerEventIds.UserPermissionLoaded, "Loaded role permissions for user {id}.", id);

            permissions = updatePermissions(permissions);

            this.__cache.GetOrCreate(
                CacheKeyHelper.GetUserPermissionKey(permissions.SnowflakeIdentifier),
                entry =>
                {
                    entry.SlidingExpiration = this.__sliding_expiration;
                    return permissions;
                });

            this.__logger?.LogTrace(LoggerEventIds.UserPermissionCached, "Cached role permissions for user {id}.", id);

            return permissions;
        }
        else
        {
            return null;
        }
    }

    public Boolean VerifyUserPermissionExistence(UInt64 id, Boolean verifyValidity = false, Boolean cacheIfValid = false)
    {
        if(!File.Exists($"./data/user-{id}/permissions.json"))
        {
            return false;
        }

        // see the comment in RolePermissionService.cs
        if(this.__cache.TryGetValue(CacheKeyHelper.GetUserPermissionKey(id), out UserPermissions _))
        {
            return true;
        }

        if(!verifyValidity)
        {
            return true;
        }

        try
        {
            StreamReader reader = new($"./data/user-{id}/permissions.json");

            UserPermissions? permissions = JsonSerializer.Deserialize(
                reader.ReadToEnd(),
                PermissionSerializationContexts.Default.User);

            if(permissions != null && cacheIfValid)
            {
                this.__cache.CreateEntry(CacheKeyHelper.GetUserPermissionKey(id))
                    .SetValue(permissions)
                    .SetSlidingExpiration(this.__sliding_expiration);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public void WriteUserPermissions(UserPermissions permissions)
    {
        StreamWriter writer;

        if(!File.Exists($"./data/user-{permissions.SnowflakeIdentifier}/permissions.json"))
        {
            writer = new(File.Create($"./data/user-{permissions.SnowflakeIdentifier}/permissions.json"));
        }
        else
        {
            writer = new($"./data/user-{permissions.SnowflakeIdentifier}/permissions.json");
        }

        writer.Write(JsonSerializer.Serialize(permissions, PermissionSerializationContexts.Default.User));

        this.__cache.GetOrCreate(
            CacheKeyHelper.GetUserPermissionKey(permissions.SnowflakeIdentifier),
            entry =>
            {
                entry.SlidingExpiration = this.__sliding_expiration;
                return permissions;
            });

        writer.Close();

        this.__logger?.LogDebug(
            LoggerEventIds.UserPermissionEdited,
            "Edited user permissions for user {id}",
            permissions.SnowflakeIdentifier);
    }

    public UserPermissions CreateUserPermissions(UInt64 snowflake)
    {

        // only called after we ensured defaults were present
        DefaultPermissions defaultPermissions = this.__defaults.GetDefaultPermissions()!;

        UserPermissions permissions = new()
        {
            IsAdministrator = defaultPermissions.IsAdministrator,
            Permissions = defaultPermissions.Permissions,
            UpdateGuid = defaultPermissions.UpdateGuid,
            SnowflakeIdentifier = snowflake
        };

        this.WriteUserPermissions(permissions);

        return permissions;
    }

    private UserPermissions updatePermissions(UserPermissions permissions)
    {
        DefaultPermissions defaultPermissions = this.__defaults.GetDefaultPermissions()!;

        if(defaultPermissions is null)
        {
            this.__logger?.LogError(
                LoggerEventIds.UserPermissionUpdateFailed,
                "Failed to update user permissions for {id}: default permissions could not be loaded.",
                permissions.SnowflakeIdentifier);

            return permissions;
        }

        if(permissions.UpdateGuid == defaultPermissions.UpdateGuid)
        {
            return permissions;
        }

        foreach(String key in defaultPermissions.Permissions.Keys)
        {
            if(!permissions.Permissions.ContainsKey(key))
            {
                permissions.Permissions.Add(key, defaultPermissions.Permissions[key]);
            }
        }

        return permissions;
    }
}
