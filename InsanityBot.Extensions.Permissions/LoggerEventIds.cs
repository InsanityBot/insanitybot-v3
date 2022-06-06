namespace InsanityBot.Extensions.Permissions;

using Microsoft.Extensions.Logging;

internal static class LoggerEventIds
{
    public static readonly EventId DefaultPermissionLoading = new(15000, "PermissionService");
    public static readonly EventId DefaultPermissionLoaded = new(15010, "PermissionService");
    public static readonly EventId DefaultPermissionCached = new(15020, "PermissionService");
    public static readonly EventId DefaultPermissionLoadingFailed = new(15030, "PermissionService");
    public static readonly EventId DefaultPermissionEdited = new(15060, "PermissionService");

    public static readonly EventId RolePermissionLoading = new(15001, "PermissionService");
    public static readonly EventId RolePermissionLoaded = new(15011, "PermissionService");
    public static readonly EventId RolePermissionCached = new(15021, "PermissionService");
    public static readonly EventId RolePermissionLoadingFailed = new(15031, "PermissionService");
    public static readonly EventId RolePermissionUpdateFailed = new(15041, "PermissionService");
    public static readonly EventId RolePermissionUpdated = new(15051, "PermissionService");
    public static readonly EventId RolePermissionEdited = new(15061, "PermissionService");

    public static readonly EventId UserPermissionLoading = new(15002, "PermissionService");
    public static readonly EventId UserPermissionLoaded = new(15012, "PermissionService");
    public static readonly EventId UserPermissionCached = new(15022, "PermissionService");
    public static readonly EventId UserPermissionLoadingFailed = new(15032, "PermissionService");
    public static readonly EventId UserPermissionUpdateFailed = new(15042, "PermissionService");
    public static readonly EventId UserPermissionUpdated = new(15052, "PermissionService");
    public static readonly EventId UserPermissionEdited = new(15062, "PermissionService");

    public static readonly EventId PermissionsInitializing = new(15100, "PermissionService");
    public static readonly EventId PermissionsInitialized = new(15101, "PermissionService");
    public static readonly EventId PermissionManifestMissing = new(15102, "PermissionService");
    public static readonly EventId PermissionMappingMissing = new(15103, "PermissionService");
}
