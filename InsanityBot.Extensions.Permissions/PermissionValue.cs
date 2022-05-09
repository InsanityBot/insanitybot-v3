namespace InsanityBot.Extensions.Permissions;

using System;

public enum PermissionValue : SByte
{
    Allowed = 1,
    Passthrough = 0,
    Denied = -1
}
