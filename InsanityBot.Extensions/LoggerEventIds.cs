namespace InsanityBot.Extensions;

using Microsoft.Extensions.Logging;

internal static class LoggerEventIds
{
    public readonly static EventId PermissionConfigurationLoading = new(2004, "PermissionConfiguration");
    public readonly static EventId PermissionConfigurationDatafixing = new(2014, "PermissionConfiguration");
    public readonly static EventId PermissionConfigurationSaving = new(2024, "PermissionConfiguration");
    public readonly static EventId PermissionConfigurationSuccess = new(2034, "PermissionConfiguration");

    public readonly static EventId UnsafeConfigurationLoading = new(2004, "UnsafeConfiguration");
    public readonly static EventId UnsafeConfigurationDatafixing = new(2014, "UnsafeConfiguration");
    public readonly static EventId UnsafeConfigurationSaving = new(2024, "UnsafeConfiguration");
    public readonly static EventId UnsafeConfigurationSuccess = new(2034, "UnsafeConfiguration");
}
