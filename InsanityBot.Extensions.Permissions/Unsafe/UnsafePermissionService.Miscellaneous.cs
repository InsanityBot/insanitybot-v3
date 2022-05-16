namespace InsanityBot.Extensions.Permissions.Unsafe;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Starnight.Internal.Entities.Guilds;

using Starnight.Internal.Entities.Users;

public partial class UnsafePermissionService
{
    public ValueTask SetParent(DiscordUser user, Int64 parent)
    {
        throw new NotImplementedException();
    }

    public ValueTask SetParent(DiscordRole role, Int64 parent)
    {
        throw new NotImplementedException();
    }

    public ValueTask SetParents(DiscordUser user, IEnumerable<Int64> parent)
    {
        throw new NotImplementedException();
    }

    public ValueTask SetParents(DiscordRole role, IEnumerable<Int64> parent)
    {
        throw new NotImplementedException();
    }

    public ValueTask RestoreDefaults(DiscordUser user)
    {
        throw new NotImplementedException();
    }

    public ValueTask RestoreDefaults(DiscordRole role)
    {
        throw new NotImplementedException();
    }

    public ValueTask RestoreManifestDefaults()
    {
        throw new NotImplementedException();
    }
}
