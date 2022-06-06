namespace InsanityBot.Extensions;

using Microsoft.Extensions.Logging;

internal static class LoggerEventIds
{
    public readonly static EventId PermissionConfigurationLoading = new(12004, "PermissionConfiguration");
    public readonly static EventId PermissionConfigurationDatafixing = new(12014, "PermissionConfiguration");
    public readonly static EventId PermissionConfigurationSaving = new(12024, "PermissionConfiguration");
    public readonly static EventId PermissionConfigurationSuccess = new(12034, "PermissionConfiguration");

    public readonly static EventId UnsafeConfigurationLoading = new(12004, "UnsafeConfiguration");
    public readonly static EventId UnsafeConfigurationDatafixing = new(12014, "UnsafeConfiguration");
    public readonly static EventId UnsafeConfigurationSaving = new(12024, "UnsafeConfiguration");
    public readonly static EventId UnsafeConfigurationSuccess = new(12034, "UnsafeConfiguration");
}
