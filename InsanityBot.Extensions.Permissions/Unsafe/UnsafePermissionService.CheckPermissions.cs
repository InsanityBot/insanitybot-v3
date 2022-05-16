namespace InsanityBot.Extensions.Permissions.Unsafe;

using System;
using System.Threading.Tasks;

using Starnight.Internal.Entities.Guilds;
using Starnight.Internal.Entities.Users;

public partial class UnsafePermissionService
{
    public ValueTask<Boolean> CheckPermission(DiscordUser user, String permission)
    {
        throw new NotImplementedException();
    }

    public ValueTask<Boolean> CheckPermission(DiscordRole role, String permission)
    {
        throw new NotImplementedException();
    }

    public ValueTask<Boolean> CheckPermission(DiscordGuildMember member, String permission)
    {
        throw new NotImplementedException();
    }

    public ValueTask<Boolean> CheckAdministrator(DiscordUser user)
    {
        throw new NotImplementedException();
    }

    public ValueTask<Boolean> CheckAdministrator(DiscordRole role)
    {
        throw new NotImplementedException();
    }
}
