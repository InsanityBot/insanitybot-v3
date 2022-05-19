namespace InsanityBot.Extensions.Permissions.Unsafe;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using InsanityBot.Extensions.Permissions.Objects;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

public partial class UnsafePermissionService : IPermissionService
{
    private readonly ILogger<IPermissionService> __logger;
    private readonly IMemoryCache __cache;

    private readonly PermissionManifest __manifest;
    private readonly PermissionMapping __mapping;
    private readonly IEnumerable<String> __permissions;

    public UnsafePermissionService
    (
        ILogger<IPermissionService> logger,
        IMemoryCache cache
    )
    {
        this.__logger = logger;
        this.__cache = cache;

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

        this.__logger.LogInformation(LoggerEventIds.PermissionsInitialized, "The permission subsystem was successfully initialized.");
    }
}
