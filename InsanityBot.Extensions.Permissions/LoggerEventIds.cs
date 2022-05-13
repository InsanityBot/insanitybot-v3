namespace InsanityBot.Extensions.Permissions;

using Microsoft.Extensions.Logging;

internal static class LoggerEventIds
{
    public static readonly EventId DefaultPermissionLoading = new(5000, "PermissionService");
    public static readonly EventId DefaultPermissionLoaded = new(5010, "PermissionService");
    public static readonly EventId DefaultPermissionCached = new(5020, "PermissionService");
    public static readonly EventId DefaultPermissionLoadingFailed = new(5030, "PermissionService");
    public static readonly EventId DefaultPermissionEdited = new(5060, "PermissionService");

    public static readonly EventId RolePermissionLoading = new(5001, "PermissionService");
    public static readonly EventId RolePermissionLoaded = new(5011, "PermissionService");
    public static readonly EventId RolePermissionCached = new(5021, "PermissionService");
    public static readonly EventId RolePermissionLoadingFailed = new(5031, "PermissionService");
    public static readonly EventId RolePermissionUpdateFailed = new(5041, "PermissionService");
    public static readonly EventId RolePermissionUpdated = new(5051, "PermissionService");
    public static readonly EventId RolePermissionEdited = new(5061, "PermissionService");

    public static readonly EventId UserPermissionLoading = new(5002, "PermissionService");
    public static readonly EventId UserPermissionLoaded = new(5012, "PermissionService");
    public static readonly EventId UserPermissionCached = new(5022, "PermissionService");
    public static readonly EventId UserPermissionLoadingFailed = new(5032, "PermissionService");
    public static readonly EventId UserPermissionUpdateFailed = new(5042, "PermissionService");
    public static readonly EventId UserPermissionUpdated = new(5052, "PermissionService");
    public static readonly EventId UserPermissionEdited = new(5062, "PermissionService");
}
