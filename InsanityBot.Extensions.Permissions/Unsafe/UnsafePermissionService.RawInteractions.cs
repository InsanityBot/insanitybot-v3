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
        return ValueTask.FromResult(
            this.__default_service.GetDefaultPermissions()
            ?? this.__default_service.CreateDefaultPermissions(this.__manifest));
    }

    public ValueTask<RolePermissions> GetRolePermissions(DiscordRole role)
    {
        return ValueTask.FromResult(
            this.__role_service.GetRolePermissions(role.Id)
            ?? this.__role_service.CreateRolePermissions(role.Id));
    }

    public ValueTask<UserPermissions> GetUserPermissions(DiscordUser user)
    {
        return ValueTask.FromResult(
            this.__user_service.GetUserPermissions(user.Id)
            ?? this.__user_service.CreateUserPermissions(user.Id));
    }


    public ValueTask SetDefaultPermissions(DefaultPermissions defaultPermissions)
    {
        this.__current_update_guid = Guid.NewGuid();

        this.__default_service.WriteDefaultPermissions(defaultPermissions with { UpdateGuid = this.__current_update_guid});

        return ValueTask.CompletedTask;
    }

    public ValueTask SetRolePermissions(RolePermissions rolePermissions)
    {
        this.__role_service.WriteRolePermissions(rolePermissions);

        return ValueTask.CompletedTask;
    }

    public ValueTask SetUserPermissions(UserPermissions userPermissions)
    {
        this.__user_service.WriteUserPermissions(userPermissions);

        return ValueTask.CompletedTask;
    }


    public ValueTask CreateRolePermissions(DiscordRole role)
    {
        RolePermissions permissions = this.__role_service.CreateRolePermissions(role.Id);

        this.__role_service.WriteRolePermissions(permissions);

        return ValueTask.CompletedTask;
    }

    public ValueTask CreateRolePermissions(RolePermissions permissions)
    {
        this.__role_service.WriteRolePermissions(permissions);

        return ValueTask.CompletedTask;
    }

    public ValueTask CreateUserPermissions(DiscordUser user)
    {
        UserPermissions permissions = this.__user_service.CreateUserPermissions(user.Id);

        this.__user_service.WriteUserPermissions(permissions);

        return ValueTask.CompletedTask;
    }

    public ValueTask CreateUserPermissions(UserPermissions permissions)
    {
        this.__user_service.WriteUserPermissions(permissions);

        return ValueTask.CompletedTask;
    }
}
