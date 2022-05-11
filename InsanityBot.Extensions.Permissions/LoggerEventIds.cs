namespace InsanityBot.Extensions.Permissions;

using Microsoft.Extensions.Logging;

internal static class LoggerEventIds
{
    public static readonly EventId DefaultPermissionLoading = new(5000, "PermissionService");
    public static readonly EventId DefaultPermissionLoaded = new(5010, "PermissionService");
    public static readonly EventId DefaultPermissionCached = new(5020, "PermissionService");
    public static readonly EventId DefaultPermissionLoadingFailed = new(5030, "PermissionService");
}
