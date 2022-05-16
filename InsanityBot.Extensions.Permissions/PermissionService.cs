namespace InsanityBot.Extensions.Permissions;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;

using InsanityBot.Extensions.Configuration;
using InsanityBot.Extensions.Permissions.Objects;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Starnight.Internal.Entities;
using Starnight.Internal.Entities.Guilds;
using Starnight.Internal.Entities.Users;
using Starnight.Internal.Rest.Resources;

public class PermissionService : IPermissionService
{
    private readonly ILogger<IPermissionService> __logger;
    private readonly IMemoryCache __cache;
    private readonly IConfiguration __configuration;

    private readonly DiscordGuildsRestResource __guild_resource;
    private readonly HttpClient __http_client;

    private readonly DefaultPermissionService __default_permission_service;
    private readonly RolePermissionService __role_permission_service;
    private readonly UserPermissionService __user_permission_service;

    private readonly PermissionManifest __manifest;
    private readonly PermissionMapping __mapping;
    private readonly IReadOnlyList<String> __permissions;

    public PermissionService
    (
        ILogger<IPermissionService> logger,
        IMemoryCache cache,
        PermissionConfiguration configuration,
        DiscordGuildsRestResource guildResource,
        HttpClient httpClient,
        DefaultPermissionService defaultService,
        RolePermissionService roleService,
        UserPermissionService userService
    )
    {
        this.__logger = logger;
        this.__cache = cache;
        this.__configuration = configuration;
        this.__guild_resource = guildResource;
        this.__http_client = httpClient;
        this.__default_permission_service = defaultService;
        this.__role_permission_service = roleService;
        this.__user_permission_service = userService;

        this.__logger.LogInformation(LoggerEventIds.PermissionsInitializing, "Initializing permission subsystem...");

        if(!this.__cache.TryGetValue(CacheKeyHelper.GetManifestKey(), out PermissionManifest? manifest))
        {
            manifest = this.loadManifest();
        }

        if(!this.__cache.TryGetValue(CacheKeyHelper.GetMappingKey(), out PermissionMapping? mapping))
        {
            mapping = this.loadMappings();
        }

        this.__manifest = manifest!;
        this.__mapping = mapping!;

        this.__permissions = this.extractPermissions();

        this.__logger.LogInformation(LoggerEventIds.PermissionsInitialized, "The permission subsystem was successfully initialized.");
    }

    #region public api
    public ValueTask<DefaultPermissions> GetDefaultPermissions()
    {
        return ValueTask.FromResult(
            this.__default_permission_service.GetDefaultPermissions()
            ?? this.__default_permission_service.CreateDefaultPermissions(this.__manifest));
    }

    public ValueTask<RolePermissions> GetRolePermissions(DiscordRole role)
    {
        return ValueTask.FromResult(
                this.__role_permission_service.GetRolePermissions(role.Id)
                ?? this.__role_permission_service.CreateRolePermissions(role.Id));
    }

    public ValueTask<UserPermissions> GetUserPermissions(DiscordUser user)
    {
        return ValueTask.FromResult(
                this.__user_permission_service.GetUserPermissions(user.Id)
                ?? this.__user_permission_service.CreateUserPermissions(user.Id));
    }

    public ValueTask SetDefaultPermissions(DefaultPermissions defaultPermissions)
    {
        this.__default_permission_service.WriteDefaultPermissions(defaultPermissions);

        return ValueTask.CompletedTask;
    }

    public ValueTask SetRolePermissions(RolePermissions rolePermissions)
    {
        this.__role_permission_service.WriteRolePermissions(rolePermissions);

        return ValueTask.CompletedTask;
    }

    public ValueTask SetUserPermissions(UserPermissions userPermissions)
    {
        this.__user_permission_service.WriteUserPermissions(userPermissions);

        return ValueTask.CompletedTask;
    }

    public ValueTask CreateRolePermissions(DiscordRole role)
    {
        this.__role_permission_service.CreateRolePermissions(role.Id);

        return ValueTask.CompletedTask;
    }

    public ValueTask CreateRolePermissions(RolePermissions permissions)
        => this.SetRolePermissions(permissions);

    public ValueTask CreateUserPermissions(DiscordUser user)
    {
        this.__user_permission_service.CreateUserPermissions(user.Id);

        return ValueTask.CompletedTask;
    }

    public ValueTask CreateUserPermissions(UserPermissions permissions)
        => this.SetUserPermissions(permissions);


    // ---- higher-level capability ---- //

    public ValueTask RemapPermissions(IEnumerable<DiscordRole> roles)
    {
        foreach(DiscordRole role in roles)
        {
            RolePermissions permissions = this.__role_permission_service.GetRolePermissions(role.Id)
                ?? this.__role_permission_service.CreateRolePermissions(role.Id);

            if(this.__mapping.PermitComplexMapping)
            {
                foreach(String permission in permissions.Permissions.Keys)
                {
                    foreach(DiscordPermissions discordPermission in this.__mapping.ComplexMapping![permission])
                    {
                        if((role.Permissions & discordPermission) != 0)
                        {
                            permissions.Permissions[permission] = PermissionValue.Allowed;
                        }
                    }
                }
            }

            this.__role_permission_service.WriteRolePermissions(permissions);
        }

        return ValueTask.CompletedTask;
    }

    public async ValueTask RemapPermissions(DiscordGuild guild)
    { 
        IEnumerable<DiscordRole> roles = await this.__guild_resource.GetRolesAsync(guild.Id);

        await this.RemapPermissions(roles);
    }

    public async ValueTask<Boolean> CheckPermission(DiscordUser user, String permission)
    {
        IEnumerable<String> resolved = this.resolveWildcards(permission);

        UserPermissions permissions = await this.GetUserPermissions(user);

        IEnumerable<Boolean> results = resolved
            .AsParallel()
            .Select(xm => this.checkSinglePermission(permissions, xm))
            .Distinct();

        return results.Count() switch
        {
            1 => results.First(),
            _ => false
        };
    }

    public async ValueTask<Boolean> CheckPermission(DiscordRole role, String permission)
    {
        IEnumerable<String> resolved = this.resolveWildcards(permission);

        RolePermissions permissions = await this.GetRolePermissions(role);

        IEnumerable<Boolean> results = resolved
            .AsParallel()
            .Select(xm => this.checkSinglePermission(permissions, xm))
            .Distinct();

        return results.Count() switch
        {
            1 => results.First(),
            _ => false
        };
    }

    public ValueTask<Boolean> CheckPermission(DiscordGuildMember member, String permission)
    {
        IEnumerable<String> resolved = this.resolveWildcards(permission);

        IEnumerable<Boolean> results = resolved
            .AsParallel()
            .Select(xm => this.checkSinglePermission(member, xm))
            .Distinct();

        return ValueTask.FromResult(results.Count() switch
        {
            1 => results.First(),
            _ => false
        });
    }

    public async ValueTask<Boolean> CheckAnyPermission(DiscordUser user, IEnumerable<String> permissions)
    {
        foreach(String permission in permissions)
        {
            if(await this.CheckPermission(user, permission))
            {
                return true;
            }
        }

        return false;
    }

    public async ValueTask<Boolean> CheckAnyPermission(DiscordRole role, IEnumerable<String> permissions)
    {
        foreach(String permission in permissions)
        {
            if(await this.CheckPermission(role, permission))
            {
                return true;
            }
        }

        return false;
    }

    public async ValueTask<Boolean> CheckAnyPermission(DiscordGuildMember member, IEnumerable<String> permissions)
    {
        foreach(String permission in permissions)
        {
            if(await this.CheckPermission(member, permission))
            {
                return true;
            }
        }

        return false;
    }

    public async ValueTask<Boolean> CheckAllPermissions(DiscordUser user, IEnumerable<String> permissions)
    {
        foreach(String permission in permissions)
{
            if(!await this.CheckPermission(user, permission))
            {
                return false;
            }
        }

        return true;
    }

    public async ValueTask<Boolean> CheckAllPermissions(DiscordRole role, IEnumerable<String> permissions)
    {
        foreach(String permission in permissions)
        {
            if(!await this.CheckPermission(role, permission))
            {
                return false;
            }
        }

        return true;
    }

    public async ValueTask<Boolean> CheckAllPermissions(DiscordGuildMember member, IEnumerable<String> permissions)
    {
        foreach(String permission in permissions)
        {
            if(!await this.CheckPermission(member, permission))
            {
                return false;
            }
        }

        return true;
    }

    public async ValueTask GrantPermission(DiscordUser user, String permission)
    {
        UserPermissions permissions = await this.GetUserPermissions(user);

        foreach(String s in this.resolveWildcards(permission))
        {
            permissions.Permissions[s] = PermissionValue.Allowed;
        }

        await this.SetUserPermissions(permissions);
    }

    public async ValueTask GrantPermission(DiscordRole role, String permission)
    {
        RolePermissions permissions = await this.GetRolePermissions(role);

        foreach(String s in this.resolveWildcards(permission))
        {
            permissions.Permissions[s] = PermissionValue.Allowed;
        }

        await this.SetRolePermissions(permissions);
    }

    public async ValueTask GrantPermissions(DiscordUser user, IEnumerable<String> permissions)
    {
        UserPermissions perm = await this.GetUserPermissions(user);

        IEnumerable<String> resolvedPermissions = permissions
            .SelectMany(this.resolveWildcards)
            .Distinct();

        foreach(String s in resolvedPermissions)
        {
            perm.Permissions[s] = PermissionValue.Allowed;
        }

        await this.SetUserPermissions(perm);
    }

    public async ValueTask GrantPermissions(DiscordRole role, IEnumerable<String> permissions)
    {
        RolePermissions perm = await this.GetRolePermissions(role);

        IEnumerable<String> resolvedPermissions = permissions
            .SelectMany(this.resolveWildcards)
            .Distinct();

        foreach(String s in resolvedPermissions)
        {
            perm.Permissions[s] = PermissionValue.Allowed;
        }

        await this.SetRolePermissions(perm);
    }

    public ValueTask RevokePermission(DiscordUser user, String permission)
    {
        throw new NotImplementedException();
    }
    public ValueTask RevokePermission(DiscordRole role, String permission)
    {
        throw new NotImplementedException();
    }
    public ValueTask RevokePermissions(DiscordUser user, IEnumerable<String> permissions)
    {
        throw new NotImplementedException();
    }
    public ValueTask RevokePermissions(DiscordRole user, IEnumerable<String> permissions)
    {
        throw new NotImplementedException();
    }

    public ValueTask UseFallback(DiscordUser user, String permission)
    {
        throw new NotImplementedException();
    }
    public ValueTask UseFallback(DiscordRole role, String permission)
    {
        throw new NotImplementedException();
    }
    public ValueTask UseFallbacks(DiscordUser user, IEnumerable<String> permissions)
    {
        throw new NotImplementedException();
    }
    public ValueTask UseFallbacks(DiscordRole user, IEnumerable<String> permissions)
    {
        throw new NotImplementedException();
    }

    public ValueTask SetAdministrator(DiscordUser user, Boolean administrator)
    {
        throw new NotImplementedException();
    }
    public ValueTask SetAdministrator(DiscordRole role, Boolean administrator)
    {
        throw new NotImplementedException();
    }

    public ValueTask SetParent(DiscordUser user, UInt64 parent)
    {
        throw new NotImplementedException();
    }
    public ValueTask SetParent(DiscordRole role, UInt64 parent)
    {
        throw new NotImplementedException();
    }
    public ValueTask SetParents(DiscordUser user, IEnumerable<UInt64> parent)
    {
        throw new NotImplementedException();
    }
    public ValueTask SetParents(DiscordRole role, IEnumerable<UInt64> parent)
    {
        throw new NotImplementedException();
    }

    public ValueTask RestoreDefaults(DiscordUser user)
    {
        throw new NotImplementedException();
    }
    public ValueTask RestoreDefaults(DiscordRole role)
    {
        throw new NotImplementedException();
    }
    public ValueTask RestoreManifestDefaults()
    {
        throw new NotImplementedException();
    }


    // ---- safety capability ---- //

    public ValueTask EnsureDefaultFileIntegrity()
    {
        throw new NotImplementedException();
    }
    public ValueTask EnsureFileIntegrity()
    {
        throw new NotImplementedException();
    }
    public ValueTask EnsureFileIntegrity(DiscordUser user)
    {
        throw new NotImplementedException();
    }
    public ValueTask EnsureFileIntegrity(DiscordRole role)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region internals - loading
    private PermissionManifest loadManifest()
    {
        if(!File.Exists("./config/permissions/permissions.manifest"))
        {
            this.__logger.LogWarning(
                LoggerEventIds.PermissionManifestMissing,
                "Missing permission manifest file, downloading from the source tree instead.");

            this.fetchFileFromSourceTree(
                "https://raw.githubusercontent.com/InsanityBot/insanitybot-v3/{commit}/config/permissions/permissions.manifest",
                "./config/permissions/permissions.manifest");
        }

        StreamReader reader = new("./config/permissions/permissions.manifest");
        PermissionManifest manifest;

        try
        {
            manifest = JsonSerializer.Deserialize(reader.ReadToEnd(), PermissionSerializationContexts.Default.Manifest)!;
        }
        catch
        {
            this.__logger.LogWarning(
                LoggerEventIds.PermissionManifestMissing,
                "Invalid permission manifest file, downloading from the source tree instead.");

            reader.Close();

            this.fetchFileFromSourceTree(
                "https://raw.githubusercontent.com/InsanityBot/insanitybot-v3/{commit}/config/permissions/permissions.manifest",
                "./config/permissions/permissions.manifest");

            reader = new("./config/permissions/permissions.manifest");

            manifest = JsonSerializer.Deserialize(reader.ReadToEnd(), PermissionSerializationContexts.Default.Manifest)!;
        }

        this.__cache.CreateEntry(CacheKeyHelper.GetManifestKey())
            .SetValue(manifest)
            .SetAbsoluteExpiration(TimeSpan.MaxValue);

        return manifest;
    }

    private PermissionMapping loadMappings()
    {
        if(!File.Exists("./config/permissions/permissions.mapping"))
        {
            this.__logger.LogWarning(
                LoggerEventIds.PermissionManifestMissing,
                "Missing permission mapping file, downloading from the source tree instead.");

            this.fetchFileFromSourceTree(
                "https://raw.githubusercontent.com/InsanityBot/insanitybot-v3/{commit}/config/permissions/permissions.mapping",
                "./config/permissions/permissions.mapping");
        }

        StreamReader reader = new("./config/permissions/permissions.mapping");
        PermissionMapping mapping;

        try
        {
            mapping = JsonSerializer.Deserialize(reader.ReadToEnd(), PermissionSerializationContexts.Default.Mapping)!;
        }
        catch
        {
            this.__logger.LogWarning(
                LoggerEventIds.PermissionMappingMissing,
                "Invalid permission mapping file, downloading from the source tree instead.");

            reader.Close();

            this.fetchFileFromSourceTree(
                "https://raw.githubusercontent.com/InsanityBot/insanitybot-v3/{commit}/config/permissions/permissions.mapping",
                "./config/permissions/permissions.mapping");

            reader = new("./config/permissions/permissions.mapping");

            mapping = JsonSerializer.Deserialize(reader.ReadToEnd(), PermissionSerializationContexts.Default.Mapping)!;
        }

        if(!mapping.PermitComplexMapping)
        {
            mapping.ComplexMapping = new();

            foreach(KeyValuePair<String, DiscordPermissions> map in mapping.Mapping!)
            {
                mapping.ComplexMapping.Add(map.Key, new DiscordPermissions[] { map.Value });
            }
        }

        this.__cache.CreateEntry(CacheKeyHelper.GetMappingKey())
            .SetValue(mapping)
            .SetAbsoluteExpiration(TimeSpan.MaxValue);

        return mapping;
    }

    private async void fetchFileFromSourceTree(String treeLink, String filename)
    {
        String commitHash = Assembly.GetExecutingAssembly()
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Where(xm => xm.Key == "git-revision-hash")
            .First()
            .Value!;

        Task<HttpResponseMessage> download = this.__http_client.GetAsync(treeLink.Replace("{commit}", commitHash));

        if(File.Exists(filename))
        {
            File.Delete(filename);
        }

        FileStream fileStream = File.Create(filename);

        (await (await download).Content.ReadAsStreamAsync()).CopyTo(fileStream);

        fileStream.Flush();
    }

    private IReadOnlyList<String> extractPermissions()
    {
        return this.__manifest.Manifest
            .Select(xm => xm.Permission)
            .ToImmutableList();
    }
    #endregion

    #region internals - wildcard handling
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private IEnumerable<String> resolveWildcards(String source)
    {
        // you have to contain at least one wildcard for us to filter wildcards
        if(!source.AsSpan().Contains('*'))
        {
            return new String[] { source };
        }

        List<String> resolved = new(this.__permissions);
        String[] parts = source.Split('.');

        for(Int32 i = 0; i < parts.Length; i++)
        {
            if(parts[i] == "*")
            {
                continue;
            }

            resolved = resolved
                .Where(xm => xm.StartsWith(String.Concat(parts[0..i])))
                .ToList();
        }

        return resolved;
    }
    #endregion

    #region internals - permission checks
    private Boolean checkSinglePermission(UserPermissions permissions, String permission)
    {
        PermissionValue immediateValue = permissions.Permissions[permission];

        if(immediateValue == PermissionValue.Allowed)
        {
            return true;
        }
        else if(immediateValue == PermissionValue.Denied)
        {
            return false;
        }

        List<Int64> roles = permissions.AssignedRoles.ToList();

        for(Int32 i = 0; i < roles.Count; i++)
        {
            RolePermissions role = this.__role_permission_service.GetRolePermissions(roles[i])!;

            if(role == null)
            {
                continue;
            }

            PermissionValue roleValue = role.Permissions[permission];

            if(roleValue == PermissionValue.Allowed)
            {
                return true;
            }
            else if(roleValue == PermissionValue.Denied)
            {
                return false;
            }

            roles.AddRange(role.Parents);
            roles = roles.Distinct().ToList();
        }

        DefaultPermissions defaults = this.__default_permission_service.GetDefaultPermissions()
            ?? this.__default_permission_service.CreateDefaultPermissions(this.__manifest);

        return defaults.Permissions[permission] switch
        {
            PermissionValue.Allowed => true,
            PermissionValue.Denied => true,
            _ => defaults.FallbackDefault
        };
    }

    private Boolean checkSinglePermission(RolePermissions permissions, String permission)
    {
        PermissionValue immediateValue = permissions.Permissions[permission];

        if(immediateValue == PermissionValue.Allowed)
        {
            return true;
        }
        else if(immediateValue == PermissionValue.Denied)
        {
            return false;
        }

        List<Int64> roles = permissions.Parents.ToList();

        for(Int32 i = 0; i < roles.Count; i++)
        {
            RolePermissions role = this.__role_permission_service.GetRolePermissions(roles[i])!;

            if(role == null)
            {
                continue;
            }

            PermissionValue roleValue = role.Permissions[permission];

            if(roleValue == PermissionValue.Allowed)
            {
                return true;
            }
            else if(roleValue == PermissionValue.Denied)
            {
                return false;
            }

            roles.AddRange(role.Parents);
            roles = roles.Distinct().ToList();
        }

        DefaultPermissions defaults = this.__default_permission_service.GetDefaultPermissions()
            ?? this.__default_permission_service.CreateDefaultPermissions(this.__manifest);

        return defaults.Permissions[permission] switch
        {
            PermissionValue.Allowed => true,
            PermissionValue.Denied => true,
            _ => defaults.FallbackDefault
        };
    }

    private Boolean checkSinglePermission(DiscordGuildMember member, String permission)
    {
        UserPermissions permissions = this.__user_permission_service.GetUserPermissions(member.User!.Id)
            ?? this.__user_permission_service.CreateUserPermissions(member.User!.Id);

        PermissionValue immediateValue = permissions.Permissions[permission];

        if(immediateValue == PermissionValue.Allowed)
        {
            return true;
        }
        else if(immediateValue == PermissionValue.Denied)
        {
            return false;
        }

        List<Int64> roles = permissions.AssignedRoles.ToList();

        if(member.Roles != null)
        {
            roles.AddRange(member.Roles);
        }

        for(Int32 i = 0; i < roles.Count; i++)
        {
            RolePermissions role = this.__role_permission_service.GetRolePermissions(roles[i])!;

            if(role == null)
            {
                continue;
            }

            PermissionValue roleValue = role.Permissions[permission];

            if(roleValue == PermissionValue.Allowed)
            {
                return true;
            }
            else if(roleValue == PermissionValue.Denied)
            {
                return false;
            }

            roles.AddRange(role.Parents);
            roles = roles.Distinct().ToList();
        }

        DefaultPermissions defaults = this.__default_permission_service.GetDefaultPermissions()
            ?? this.__default_permission_service.CreateDefaultPermissions(this.__manifest);

        return defaults.Permissions[permission] switch
        {
            PermissionValue.Allowed => true,
            PermissionValue.Denied => true,
            _ => defaults.FallbackDefault
        };
    }
    #endregion
}
