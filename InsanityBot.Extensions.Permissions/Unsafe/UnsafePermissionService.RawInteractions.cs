namespace InsanityBot.Extensions.Permissions.Unsafe;

using System;
using System.Threading.Tasks;

using InsanityBot.Extensions.Permissions.Objects;

using Starnight.Internal.Entities.Guilds;
using Starnight.Internal.Entities.Users;

public partial class UnsafePermissionService
{
    public ValueTask<DefaultPermissions> GetDefaultPermissions()
    {
        throw new NotImplementedException();
    }

    public ValueTask<RolePermissions> GetRolePermissions(DiscordRole role)
    {
        throw new NotImplementedException();
    }

    public ValueTask<UserPermissions> GetUserPermissions(DiscordUser user)
    {
        throw new NotImplementedException();
    }


    public ValueTask SetDefaultPermissions(DefaultPermissions defaultPermissions)
    {
        throw new NotImplementedException();
    }

    public ValueTask SetRolePermissions(RolePermissions rolePermissions)
    {
        throw new NotImplementedException();
    }

    public ValueTask SetUserPermissions(UserPermissions userPermissions)
    {
        throw new NotImplementedException();
    }


    public ValueTask CreateRolePermissions(DiscordRole role)
    {
        throw new NotImplementedException();
    }

    public ValueTask CreateRolePermissions(RolePermissions permissions)
    {
        throw new NotImplementedException();
    }

    public ValueTask CreateUserPermissions(DiscordUser user)
    {
        throw new NotImplementedException();
    }

    public ValueTask CreateUserPermissions(UserPermissions permissions)
    {
        throw new NotImplementedException();
    }
}
