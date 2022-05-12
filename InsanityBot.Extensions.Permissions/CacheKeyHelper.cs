namespace InsanityBot.Extensions.Permissions;

using System;

internal static class CacheKeyHelper
{
    public static String GetDefaultPermissionKey() => "InsanityBot.Extensions.Permissions.DefaultPermissions";

    public static String GetRolePermissionKey(UInt64 id) => $"InsanityBot.Extensions.Permissions.RolePermissions:{id}";
}
