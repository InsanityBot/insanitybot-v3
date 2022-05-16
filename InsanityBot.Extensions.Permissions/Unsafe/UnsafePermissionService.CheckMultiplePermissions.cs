namespace InsanityBot.Extensions.Permissions.Unsafe;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Starnight.Internal.Entities.Guilds;

using Starnight.Internal.Entities.Users;

public partial class UnsafePermissionService
{
    public ValueTask<Boolean> CheckAnyPermission(DiscordUser user, IEnumerable<String> permissions)
    {
        throw new NotImplementedException();
    }

    public ValueTask<Boolean> CheckAnyPermission(DiscordRole role, IEnumerable<String> permissions)
    {
        throw new NotImplementedException();
    }

    public ValueTask<Boolean> CheckAnyPermission(DiscordGuildMember member, IEnumerable<String> permissions)
    {
        throw new NotImplementedException();
    }

    public ValueTask<Boolean> CheckAllPermissions(DiscordUser user, IEnumerable<String> permissions)
    {
        throw new NotImplementedException();
    }

    public ValueTask<Boolean> CheckAllPermissions(DiscordRole role, IEnumerable<String> permissions)
    {
        throw new NotImplementedException();
    }

    public ValueTask<Boolean> CheckAllPermissions(DiscordGuildMember member, IEnumerable<String> permissions)
    {
        throw new NotImplementedException();
    }

}
