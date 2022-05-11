namespace InsanityBot.Extensions;

using Microsoft.Extensions.Logging;

internal static class LoggerEventIds
{
    public readonly static EventId PermissionConfigurationLoading = new(2004, "PermissionConfiguration");
    public readonly static EventId PermissionConfigurationDatafixing = new(2014, "PermissionConfiguration");
    public readonly static EventId PermissionConfigurationSaving = new(2024, "PermissionConfiguration");
    public readonly static EventId PermissionConfigurationSuccess = new(2034, "PermissionConfiguration");
}
