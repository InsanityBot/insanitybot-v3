namespace InsanityBot.Extensions.Permissions.Unsafe;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using InsanityBot.Extensions.Permissions.Objects;

using Starnight.Internal.Entities.Guilds;

using Starnight.Internal.Entities.Users;

public partial class UnsafePermissionService
{
    public ValueTask SetParent
    (
        DiscordUser user,
        Int64 parent
    )
    {
        UserPermissions userPermissions = this.__user_service.GetUserPermissions(user.Id)
            ?? this.__user_service.CreateUserPermissions(user.Id);

        userPermissions.AssignedRoles = Enumerable.Concat(userPermissions.AssignedRoles, Enumerable.Repeat(parent, 1))
            .ToArray();

        this.__user_service.WriteUserPermissions(userPermissions);

        return ValueTask.CompletedTask;
    }

    public ValueTask SetParent
    (
        DiscordRole role,
        Int64 parent
    )
    {
        RolePermissions rolePermissions = this.__role_service.GetRolePermissions(role.Id)
            ?? this.__role_service.CreateRolePermissions(role.Id);

        rolePermissions.Parents = Enumerable.Concat(rolePermissions.Parents, Enumerable.Repeat(parent, 1))
            .ToArray();

        this.__role_service.WriteRolePermissions(rolePermissions);

        _ = Task.Run(this.rebuildRoleTrees);

        return ValueTask.CompletedTask;
    }

    public ValueTask SetParents
    (
        DiscordUser user,
        IEnumerable<Int64> parents
    )
    {
        UserPermissions userPermissions = this.__user_service.GetUserPermissions(user.Id)
            ?? this.__user_service.CreateUserPermissions(user.Id);

        userPermissions.AssignedRoles = Enumerable.Concat(userPermissions.AssignedRoles, parents)
            .ToArray();

        this.__user_service.WriteUserPermissions(userPermissions);

        return ValueTask.CompletedTask;
    }

    public ValueTask SetParents
    (
        DiscordRole role,
        IEnumerable<Int64> parents
    )
    {
        RolePermissions rolePermissions = this.__role_service.GetRolePermissions(role.Id)
            ?? this.__role_service.CreateRolePermissions(role.Id);

        rolePermissions.Parents = Enumerable.Concat(rolePermissions.Parents, parents)
            .ToArray();

        this.__role_service.WriteRolePermissions(rolePermissions);

        _ = Task.Run(this.rebuildRoleTrees);

        return ValueTask.CompletedTask;
    }

    public ValueTask RestoreDefaults
    (
        DiscordUser user
    )
    {
        UserPermissions userPermissions = this.__user_service.CreateUserPermissions(user.Id);
        this.__user_service.WriteUserPermissions(userPermissions);

        return ValueTask.CompletedTask;
    }

    public ValueTask RestoreDefaults
    (
        DiscordRole role
    )
    {
        RolePermissions rolePermissions = this.__role_service.CreateRolePermissions(role.Id);
        this.__role_service.WriteRolePermissions(rolePermissions);

        _ = Task.Run(this.rebuildRoleTrees);

        return ValueTask.CompletedTask;
    }

    public ValueTask RestoreManifestDefaults()
    {
        DefaultPermissions permissions = this.__default_service.CreateDefaultPermissions(this.__manifest);
        this.__default_service.WriteDefaultPermissions(permissions);

        return ValueTask.CompletedTask;
    }
}
