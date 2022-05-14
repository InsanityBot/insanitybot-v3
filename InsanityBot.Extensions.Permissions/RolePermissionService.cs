namespace InsanityBot.Extensions.Permissions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using InsanityBot.Extensions.Configuration;
using InsanityBot.Extensions.Permissions.Objects;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

public class RolePermissionService
{
    private readonly ILogger<IPermissionService>? __logger;
    private readonly IMemoryCache __cache;
    private readonly IConfiguration __configuration;
    private readonly IConfiguration __unsafe_configuration;

    private readonly DefaultPermissionService __defaults;

    private readonly TimeSpan __sliding_expiration;

    public RolePermissionService
    (
        ILogger<IPermissionService>? logger,
        IMemoryCache cache,
        PermissionConfiguration configuration,
        UnsafeConfiguration unsafeConfiguration,
        DefaultPermissionService defaults
    )
    {
        this.__logger = logger;
        this.__cache = cache;
        this.__configuration = configuration;
        this.__unsafe_configuration = unsafeConfiguration;
        this.__defaults = defaults;
        this.__sliding_expiration =
            this.__configuration.Value<Boolean>("insanitybot.permissions.roles.always_keep_roles_loaded")
            ? TimeSpan.MaxValue
            : TimeSpan.Parse(this.__configuration.Value<String>("insanitybot.permissions.roles.cache_expiration")!);

        preloadRoles();

    }

    public RolePermissions? GetRolePermissions(UInt64 id, Boolean bypassCache = false)
    {
        if(!bypassCache && this.__cache.TryGetValue(CacheKeyHelper.GetRolePermissionKey(id), out RolePermissions permissions))
        {
            return permissions;
        }

        this.__logger?.LogTrace(LoggerEventIds.RolePermissionLoading, "Loading role permissions for role {id}...", id);

        if(File.Exists($"./data/permissions/{id}.json"))
        {
            StreamReader reader = new($"./data/permissions/{id}.json");

            try
            {
                permissions = JsonSerializer.Deserialize(reader.ReadToEnd(), PermissionSerializationContexts.Default.Role)!;
                reader.Close();
            }
            catch(Exception e)
            {
                this.__logger?.LogError(e, "Failed to load role permission file for role {role}", id);
                reader.Close();
                return null;
            }

            this.__logger?.LogTrace(LoggerEventIds.RolePermissionLoaded, "Loaded role permissions for role {id}.", id);

            permissions = updatePermissions(permissions);

            this.__cache.GetOrCreate(
                CacheKeyHelper.GetRolePermissionKey(permissions.SnowflakeIdentifier),
                entry =>
                {
                    entry.SlidingExpiration = this.__sliding_expiration;
                    return permissions;
                });

            this.__logger?.LogTrace(LoggerEventIds.RolePermissionCached, "Cached role permissions for role {id}.", id);

            return permissions;
        }
        else
        {
            return null;
        }
    }

    public Boolean VerifyRolePermissionExistence(UInt64 id, Boolean verifyValidity = false, Boolean cacheIfValid = false)
    {
        if(!File.Exists($"./data/permissions/{id}.json"))
        {
            return false;
        }

        // i'm aware this isn't atomic. silence.
        // trying to add the object to cache later is technically a race condition. however, there's no reasonable way to
        // resolve it without slowing everything down to a standstill.
        if(this.__cache.TryGetValue(CacheKeyHelper.GetRolePermissionKey(id), out RolePermissions _))
        {
            return true;
        }

        if(!verifyValidity)
        {
            return true;
        }

        try
        {
            StreamReader reader = new($"./data/permissions/{id}.json");

            RolePermissions? permissions = JsonSerializer.Deserialize(
                reader.ReadToEnd(),
                PermissionSerializationContexts.Default.Role);

            if(permissions != null && cacheIfValid)
            {
                this.__cache.CreateEntry(CacheKeyHelper.GetRolePermissionKey(id))
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

    public void WriteRolePermissions(RolePermissions permissions)
    {
        StreamWriter writer;

        if(!File.Exists($"./data/permissions/{permissions.SnowflakeIdentifier}.json"))
        {
            writer = new(File.Create($"./data/permissions/{permissions.SnowflakeIdentifier}.json"));
        }
        else
        {
            writer = new($"./data/permissions/{permissions.SnowflakeIdentifier}.json");
        }

        writer.Write(JsonSerializer.Serialize(permissions, PermissionSerializationContexts.Default.Role));

        this.__cache.GetOrCreate(
            CacheKeyHelper.GetRolePermissionKey(permissions.SnowflakeIdentifier),
            entry =>
            {
                entry.SlidingExpiration = this.__sliding_expiration;
                return permissions;
            });

        writer.Close();

        this.__logger?.LogDebug(
            LoggerEventIds.RolePermissionEdited,
            "Edited role permissions for role {id}",
            permissions.SnowflakeIdentifier);
    }

    public RolePermissions CreateRolePermissions(UInt64 snowflake)
    {
        // only called after we ensured defaults were present
        DefaultPermissions defaultPermissions = this.__defaults.GetDefaultPermissions()!;

        RolePermissions permissions = new()
        {
            IsAdministrator = defaultPermissions.IsAdministrator,
            Permissions = defaultPermissions.Permissions,
            UpdateGuid = defaultPermissions.UpdateGuid,
            SnowflakeIdentifier = snowflake
        };

        this.WriteRolePermissions(permissions);

        return permissions;
    }

    private RolePermissions updatePermissions(RolePermissions permissions)
    {
        DefaultPermissions defaultPermissions = this.__defaults.GetDefaultPermissions()!;

        if(defaultPermissions is null)
        {
            this.__logger?.LogError(
                LoggerEventIds.RolePermissionUpdateFailed,
                "Failed to update role permissions for {id}: default permissions could not be loaded.",
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

    // only executed if caching is enabled. why else would you preload roles?
    private void preloadRoles()
    {
        List<String> filenames;
        UInt64[] roles;

        if(this.__configuration.Value<Boolean>("insanitybot.permissions.roles.always_preload_all_roles"))
        {
            filenames = Directory.GetFiles("./data/permissions").ToList();
            filenames.Remove(Path.GetFullPath("./data/permissions/default.json"));
        }
        else if((roles = this.__configuration.Value<UInt64[]>("insanitybot.permissions.roles.preload_roles")!).Length != 0)
        {
            filenames = new();

            foreach(UInt64 role in roles)
            {
                filenames.Add($"./data/permissions/{role}.json");
            }
        }
        else
        {
            return;
        }

        this.__logger?.LogDebug(LoggerEventIds.RolePermissionLoading, "Pre-loading role permissions...");

        // load them now
        RolePermissions[] permissions;

        if(this.__unsafe_configuration.Value<Boolean>("insanitybot.permissions.roles.preload_roles_parallelized"))
        {
            RolePermissions? permission;
            Boolean faulted = false;
            permissions = filenames
                .AsParallel()
                .Select(xm =>
                {
                    StreamReader reader = new(xm);

                    try
                    {
                        permission = JsonSerializer.Deserialize(reader.ReadToEnd(), PermissionSerializationContexts.Default.Role)!;
                    }
                    catch(Exception e)
                    {
                        // we can't reasonably return this early without potentially causing major trouble
                        // this is an unsafe setting for a reason: if load fails we get no benefits and all the harm.
                        // it can also cause trouble on low-end machines or on... any windows version, really
                        faulted = true;
                        this.__logger?.LogError(e, "Failed to load role permission file {filename}, abandoning pre-load", xm);
                        permission = null;
                    }

                    reader.Close();
                    return permission;
                })
                // we're handling null ourselves right below

                .ToArray()!;

            if(faulted)
            {
                return;
            }
        }
        else
        {
            permissions = new RolePermissions[filenames.Count];

            for(Int32 i = 0; i < filenames.Count; i++)
            {
                StreamReader reader = new(filenames[i]);

                try
                {
                    permissions[i] = JsonSerializer.Deserialize(reader.ReadToEnd(), PermissionSerializationContexts.Default.Role)!;
                }
                catch(Exception e)
                {
                    this.__logger?.LogError(e, "Failed to load role permission file {filename}, abandoning pre-load", filenames[i]);
                    reader.Close();
                    return;
                }

                reader.Close();
            }
        }

        this.__logger?.LogDebug(LoggerEventIds.RolePermissionLoaded, "Successfully pre-loaded role permissions.");

        // and now cache them

        foreach(RolePermissions role in permissions)
        {
            this.__cache.CreateEntry(CacheKeyHelper.GetRolePermissionKey(role.SnowflakeIdentifier))
                .SetValue(role)
                .SetSlidingExpiration(this.__sliding_expiration);
        }

        this.__logger?.LogDebug(LoggerEventIds.RolePermissionCached, "Successfully cached pre-loaded role permissions.");
    }
}
