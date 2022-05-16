namespace InsanityBot.Extensions.Permissions.Unsafe;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Starnight.Internal.Entities.Guilds;

using Starnight.Internal.Entities.Users;

public partial class UnsafePermissionService
{
    public ValueTask GrantPermission(DiscordUser user, String permission)
    {
        throw new NotImplementedException();
    }

    public ValueTask GrantPermission(DiscordRole role, String permission)
    {
        throw new NotImplementedException();
    }

    public ValueTask GrantPermissions(DiscordUser user, IEnumerable<String> permissions)
    {
        throw new NotImplementedException();
    }

    public ValueTask GrantPermissions(DiscordRole role, IEnumerable<String> permissions)
    {
        throw new NotImplementedException();
    }

    public ValueTask RevokePermission(DiscordUser user, String permission)
    {
        throw new NotImplementedException();
    }

    public ValueTask RevokePermission(DiscordRole role, String permission)
    {
        throw new NotImplementedException();
    }

    public ValueTask RevokePermissions(DiscordUser user, IEnumerable<String> permissions)
    {
        throw new NotImplementedException();
    }

    public ValueTask RevokePermissions(DiscordRole role, IEnumerable<String> permissions)
    {
        throw new NotImplementedException();
    }

    public ValueTask UseFallback(DiscordUser user, String permission)
    {
        throw new NotImplementedException();
    }

    public ValueTask UseFallback(DiscordRole role, String permission)
    {
        throw new NotImplementedException();
    }

    public ValueTask UseFallbacks(DiscordUser user, IEnumerable<String> permissions)
    {
        throw new NotImplementedException();
    }

    public ValueTask UseFallbacks(DiscordRole role, IEnumerable<String> permissions)
    {
        throw new NotImplementedException();
    }

    public ValueTask SetAdministrator(DiscordUser user, Boolean administrator)
    {
        throw new NotImplementedException();
    }

    public ValueTask SetAdministrator(DiscordRole role, Boolean administrator)
    {
        throw new NotImplementedException();
    }
}
