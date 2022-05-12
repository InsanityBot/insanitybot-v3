namespace InsanityBot.Extensions.Permissions;

using Microsoft.Extensions.Logging;

internal static class LoggerEventIds
{
    public static readonly EventId DefaultPermissionLoading = new(5000, "PermissionService");
    public static readonly EventId DefaultPermissionLoaded = new(5010, "PermissionService");
    public static readonly EventId DefaultPermissionCached = new(5020, "PermissionService");
    public static readonly EventId DefaultPermissionLoadingFailed = new(5030, "PermissionService");

    public static readonly EventId RolePermissionLoading = new(5001, "PermissionService");
    public static readonly EventId RolePermissionLoaded = new(5011, "PermissionService");
    public static readonly EventId RolePermissionCached = new(5021, "PermissionService");
    public static readonly EventId RolePermissionLoadingFailed = new(5031, "PermissionService");
    public static readonly EventId RolePermissionUpdateFailed = new(5041, "PermissionService");
}
