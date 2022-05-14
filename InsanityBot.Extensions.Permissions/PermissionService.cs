namespace InsanityBot.Extensions.Permissions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

using InsanityBot.Extensions.Configuration;
using InsanityBot.Extensions.Permissions.Objects;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Results;

public class PermissionService : IPermissionService
{
    private readonly ILogger<IPermissionService> __logger;
    private readonly IMemoryCache __cache;
    private readonly IConfiguration __configuration;

    private readonly IDiscordRestGuildAPI __guild_api;
    private readonly HttpClient __http_client;

    private readonly DefaultPermissionService __default_permission_service;
    private readonly RolePermissionService __role_permission_service;
    private readonly UserPermissionService __user_permission_service;

    private readonly PermissionManifest __manifest;
    private readonly PermissionMapping __mapping;

    public PermissionService
    (
        ILogger<IPermissionService> logger,
        IMemoryCache cache,
        PermissionConfiguration configuration,
        IDiscordRestGuildAPI guildAPI,
        HttpClient httpClient,
        DefaultPermissionService defaultService,
        RolePermissionService roleService,
        UserPermissionService userService
    )
    {
        this.__logger = logger;
        this.__cache = cache;
        this.__configuration = configuration;
        this.__guild_api = guildAPI;
        this.__http_client = httpClient;
        this.__default_permission_service = defaultService;
        this.__role_permission_service = roleService;
        this.__user_permission_service = userService;

        this.__logger.LogInformation(LoggerEventIds.PermissionsInitializing, "Initializing permission subsystem...");

        if(!this.__cache.TryGetValue(CacheKeyHelper.GetManifestKey(), out PermissionManifest manifest))
        {
            manifest = this.loadManifest();
        }

        if(!this.__cache.TryGetValue(CacheKeyHelper.GetMappingKey(), out PermissionMapping mapping))
        {
            mapping = this.loadMappings();
        }

        this.__manifest = manifest;
        this.__mapping = mapping;

        this.__logger.LogInformation(LoggerEventIds.PermissionsInitialized, "The permission subsystem was successfully initialized.");
    }

    #region public api
    public ValueTask<DefaultPermissions> GetDefaultPermissions()
    {
        return ValueTask.FromResult(
            this.__default_permission_service.GetDefaultPermissions()
            ?? this.__default_permission_service.CreateDefaultPermissions(this.__manifest));
    }

    public ValueTask<Result<RolePermissions>> GetRolePermissions(IPartialRole role)
    {
        if(!role.ID.HasValue)
        {
            return ValueTask.FromResult(
                Result<RolePermissions>.FromError(
                    new ArgumentNullError(
                        nameof(role.ID),
                        "A valid snowflake identifier is required to load role permissions.")));
        }

        return ValueTask.FromResult(
            Result<RolePermissions>.FromSuccess(
                this.__role_permission_service.GetRolePermissions(role.ID.Value.Value)
                ?? this.__role_permission_service.CreateRolePermissions(role.ID.Value.Value)));
    }

    public ValueTask<Result<UserPermissions>> GetUserPermissions(IPartialUser user)
    {
        if(!user.ID.HasValue)
        {
            return ValueTask.FromResult(
                Result<UserPermissions>.FromError(
                    new ArgumentNullError(
                        nameof(user.ID),
                        "A valid snowflake identifier is required to load user permissions.")));
        }

        return ValueTask.FromResult(
            Result<UserPermissions>.FromSuccess(
                this.__user_permission_service.GetUserPermissions(user.ID.Value.Value)
                ?? this.__user_permission_service.CreateUserPermissions(user.ID.Value.Value)));
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

    public ValueTask<IResult> CreateRolePermissions(IPartialRole role)
    {
        if(!role.ID.HasValue)
        {
            return ValueTask.FromResult(
                (IResult)Result.FromError(
                    new ArgumentNullError(
                        nameof(role.ID),
                        "A valid snowflake identifier is required to load role permissions.")));
        }

        this.__role_permission_service.CreateRolePermissions(role.ID.Value.Value);

        return ValueTask.FromResult((IResult)Result.FromSuccess());
    }

    public ValueTask CreateRolePermissions(RolePermissions permissions)
        => this.SetRolePermissions(permissions);

    public ValueTask<IResult> CreateUserPermissions(IPartialUser user)
    {
        if(!user.ID.HasValue)
        {
            return ValueTask.FromResult(
                (IResult)Result.FromError(
                    new ArgumentNullError(
                        nameof(user.ID),
                        "A valid snowflake identifier is required to load user permissions.")));
}

        this.__user_permission_service.CreateUserPermissions(user.ID.Value.Value);

        return ValueTask.FromResult((IResult)Result.FromSuccess());
    }

    public ValueTask CreateUserPermissions(UserPermissions permissions)
        => this.SetUserPermissions(permissions);


    // ---- higher-level capability ---- //

    public ValueTask<IResult> RemapPermissions(IEnumerable<IPartialRole> roles);
    public ValueTask<IResult> RemapPermissions(IPartialGuild guild);

    public ValueTask<Result<Boolean>> CheckPermission(IPartialUser user, String permission);
    public ValueTask<Result<Boolean>> CheckPermission(IPartialRole role, String permission);
    public ValueTask<Result<Boolean>> CheckAnyPermission(IPartialUser user, IEnumerable<String> permissions);
    public ValueTask<Result<Boolean>> CheckAnyPermission(IPartialRole role, IEnumerable<String> permissions);
    public ValueTask<Result<Boolean>> CheckAllPermissions(IPartialUser user, IEnumerable<String> permissions);
    public ValueTask<Result<Boolean>> CheckAllPermissions(IPartialRole role, IEnumerable<String> permissions);

    public ValueTask<IResult> GrantPermission(IPartialUser user, String permission);
    public ValueTask<IResult> GrantPermission(IPartialRole role, String permission);
    public ValueTask<IResult> GrantPermissions(IPartialUser user, IEnumerable<String> permissions);
    public ValueTask<IResult> GrantPermissions(IPartialRole user, IEnumerable<String> permissions);

    public ValueTask<IResult> RevokePermission(IPartialUser user, String permission);
    public ValueTask<IResult> RevokePermission(IPartialRole role, String permission);
    public ValueTask<IResult> RevokePermissions(IPartialUser user, IEnumerable<String> permissions);
    public ValueTask<IResult> RevokePermissions(IPartialRole user, IEnumerable<String> permissions);

    public ValueTask<IResult> UseFallback(IPartialUser user, String permission);
    public ValueTask<IResult> UseFallback(IPartialRole role, String permission);
    public ValueTask<IResult> UseFallbacks(IPartialUser user, IEnumerable<String> permissions);
    public ValueTask<IResult> UseFallbacks(IPartialRole user, IEnumerable<String> permissions);

    public ValueTask<IResult> SetAdministrator(IPartialUser user, Boolean administrator);
    public ValueTask<IResult> SetAdministrator(IPartialRole role, Boolean administrator);

    public ValueTask<IResult> SetParent(IPartialUser user, UInt64 parent);
    public ValueTask<IResult> SetParent(IPartialRole role, UInt64 parent);
    public ValueTask<IResult> SetParents(IPartialUser user, IEnumerable<UInt64> parent);
    public ValueTask<IResult> SetParents(IPartialRole role, IEnumerable<UInt64> parent);

    public ValueTask<IResult> RestoreDefaults(IPartialUser user);
    public ValueTask<IResult> RestoreDefaults(IPartialRole role);
    public ValueTask RestoreManifestDefaults();


    // ---- safety capability ---- //

    public ValueTask<IResult> EnsureDefaultFileIntegrity();
    public ValueTask<IResult> EnsureFileIntegrity();
    public ValueTask<IResult> EnsureFileIntegrity(IPartialUser user);
    public ValueTask<IResult> EnsureFileIntegrity(IPartialRole role);
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
    #endregion
}
