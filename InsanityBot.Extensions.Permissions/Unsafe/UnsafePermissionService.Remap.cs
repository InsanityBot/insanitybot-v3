namespace InsanityBot.Extensions.Permissions.Unsafe;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Starnight.Internal.Entities.Guilds;

public partial class UnsafePermissionService
{
    public ValueTask RemapPermissions(IEnumerable<DiscordRole> roles)
    {
        throw new NotImplementedException();
    }

    public ValueTask RemapPermissions(DiscordGuild guild)
    {
        throw new NotImplementedException();
    }
}
