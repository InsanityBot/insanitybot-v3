namespace InsanityBot.Extensions.Permissions.Unsafe;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using InsanityBot.Extensions.Permissions.Objects;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Starnight.Internal.Rest.Resources;

public partial class UnsafePermissionService : IPermissionService
{
    private readonly ILogger<IPermissionService> __logger;
    private readonly IMemoryCache __cache;

    private readonly DefaultPermissionService __default_service;
    private readonly RolePermissionService __role_service;
    private readonly UserPermissionService __user_service;

    private readonly DiscordGuildRestResource __guild_resource;

    private readonly PermissionManifest __manifest;
    private readonly PermissionMapping __mapping;
    private readonly IEnumerable<String> __permissions;

    private readonly Dictionary<String, PermissionValue> __permissions_all_passthrough;

    private readonly ConcurrentDictionary<Int64, IEnumerable<Int64>> __role_trees;

    private Guid __current_update_guid;

    // belonging to UnsafePermissionService.ResolveWildcards.cs
    // 
    // one thing to note about the use of Memory<Char> and .Span calls here: these are purely on the stack.
    // there are no heap allocations incurred there: in fact matchWildcards is allocation-free, and resolveWildcards
    // only allocates once for its final result.
    private readonly Memory<Char> __wildcards;
    private readonly Memory<Char> __tolerate_anything_pattern;

    public UnsafePermissionService
    (
        ILogger<IPermissionService> logger,
        IMemoryCache cache,
        DefaultPermissionService defaultService,
        RolePermissionService roleService,
        UserPermissionService userService,
        DiscordGuildRestResource guildResource
    )
    {
        this.__logger = logger;
        this.__cache = cache;
        this.__default_service = defaultService;
        this.__role_service = roleService;
        this.__user_service = userService;
        this.__guild_resource = guildResource;

        this.__logger.LogInformation(LoggerEventIds.PermissionsInitializing, "Initializing permission subsystem...");

        // we only allocate 32 bit of memory through this entire block.
        // this could almost fit into a slim, read: low-memory implementation of IPermissionService,
        // however other parts of UnsafePermissionService couldn't.
        this.__wildcards = new Char[] { '*', '?' }.AsMemory();
        this.__tolerate_anything_pattern = this.__wildcards[..1];

        if(!this.__cache.TryGetValue(CacheKeyHelper.GetManifestKey(), out PermissionManifest? manifest))
        {
            StreamReader reader = new("./config/permissions/permissions.manifest");
            manifest = JsonSerializer.Deserialize(reader.ReadToEnd(), PermissionSerializationContexts.Default.Manifest)!;
            reader.Close();
        }

        if(!this.__cache.TryGetValue(CacheKeyHelper.GetMappingKey(), out PermissionMapping? mapping))
        {
            StreamReader reader = new("./config/permissions/permissions.mapping");
            mapping = JsonSerializer.Deserialize(reader.ReadToEnd(), PermissionSerializationContexts.Default.Mapping)!;
            reader.Close();
        }

        this.__manifest = manifest!;
        this.__mapping = mapping!;

        this.__permissions = manifest!.Manifest.Select(xm => xm.Permission);

        this.__current_update_guid = (this.__default_service.GetDefaultPermissions()
            ?? this.__default_service.CreateDefaultPermissions(manifest!)).UpdateGuid;

        IEnumerable<PermissionValue> passthroughPermissions = __permissions.Select(xm => PermissionValue.Passthrough);
        IEnumerator<String> permissionEnumerator = this.__permissions.GetEnumerator();

        this.__permissions_all_passthrough = passthroughPermissions
            .ToDictionary(_ =>
            {
                permissionEnumerator.MoveNext();
                return permissionEnumerator.Current;
            });

        this.__role_trees = new();

        this.__logger.LogInformation(LoggerEventIds.PermissionsInitialized, "The permission subsystem was successfully initialized.");
    }
}
