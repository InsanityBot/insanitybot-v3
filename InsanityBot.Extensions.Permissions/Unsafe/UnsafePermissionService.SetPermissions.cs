namespace InsanityBot.Extensions.Permissions.Unsafe;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using InsanityBot.Extensions.Permissions.Objects;

using Starnight.Internal.Entities.Guilds;
using Starnight.Internal.Entities.Users;

public partial class UnsafePermissionService
{
    public ValueTask GrantPermission(DiscordUser user, String permission)
    {
        IEnumerable<String> permissions = this.resolveWildcards(permission);

        UserPermissions userPermissions = this.__user_service.GetUserPermissions(user.Id)
            ?? this.__user_service.CreateUserPermissions(user.Id);

        foreach(String singlePermission in permissions)
        {
            userPermissions.Permissions[singlePermission] = PermissionValue.Allowed;
        }

        this.__user_service.WriteUserPermissions(userPermissions);

        return ValueTask.CompletedTask;
    }

    public ValueTask GrantPermission(DiscordRole role, String permission)
    {
        IEnumerable<String> permissions = this.resolveWildcards(permission);

        RolePermissions rolePermissions = this.__role_service.GetRolePermissions(role.Id)
            ?? this.__role_service.CreateRolePermissions(role.Id);

        foreach(String singlePermission in permissions)
        {
            rolePermissions.Permissions[singlePermission] = PermissionValue.Allowed;
        }

        this.__role_service.WriteRolePermissions(rolePermissions);

        return ValueTask.CompletedTask;
    }

    public async ValueTask GrantPermissions(DiscordUser user, IEnumerable<String> permissions)
    {
        foreach(String singlePermission in permissions)
        {
            await this.GrantPermission(user, singlePermission);
        }
    }

    public async ValueTask GrantPermissions(DiscordRole role, IEnumerable<String> permissions)
    {
        foreach(String singlePermission in permissions)
        {
            await this.GrantPermission(role, singlePermission);
        }
    }

    public ValueTask RevokePermission(DiscordUser user, String permission)
    {
        IEnumerable<String> permissions = this.resolveWildcards(permission);

        UserPermissions userPermissions = this.__user_service.GetUserPermissions(user.Id)
            ?? this.__user_service.CreateUserPermissions(user.Id);

        foreach(String singlePermission in permissions)
        {
            userPermissions.Permissions[singlePermission] = PermissionValue.Denied;
        }

        this.__user_service.WriteUserPermissions(userPermissions);

        return ValueTask.CompletedTask;
    }

    public ValueTask RevokePermission(DiscordRole role, String permission)
    {
        IEnumerable<String> permissions = this.resolveWildcards(permission);

        RolePermissions rolePermissions = this.__role_service.GetRolePermissions(role.Id)
            ?? this.__role_service.CreateRolePermissions(role.Id);

        foreach(String singlePermission in permissions)
        {
            rolePermissions.Permissions[singlePermission] = PermissionValue.Denied;
        }

        this.__role_service.WriteRolePermissions(rolePermissions);

        return ValueTask.CompletedTask;
    }

    public async ValueTask RevokePermissions(DiscordUser user, IEnumerable<String> permissions)
    {
        foreach(String singlePermission in permissions)
        {
            await this.RevokePermission(user, singlePermission);
        }
    }

    public async ValueTask RevokePermissions(DiscordRole role, IEnumerable<String> permissions)
    {
        foreach(String singlePermission in permissions)
        {
            await this.RevokePermission(role, singlePermission);
        }
    }

    public ValueTask UseFallback(DiscordUser user, String permission)
    {
        IEnumerable<String> permissions = this.resolveWildcards(permission);

        UserPermissions userPermissions = this.__user_service.GetUserPermissions(user.Id)
            ?? this.__user_service.CreateUserPermissions(user.Id);

        foreach(String singlePermission in permissions)
        {
            userPermissions.Permissions[singlePermission] = PermissionValue.Passthrough;
        }

        this.__user_service.WriteUserPermissions(userPermissions);

        return ValueTask.CompletedTask;
    }

    public ValueTask UseFallback(DiscordRole role, String permission)
    {
        IEnumerable<String> permissions = this.resolveWildcards(permission);

        RolePermissions rolePermissions = this.__role_service.GetRolePermissions(role.Id)
            ?? this.__role_service.CreateRolePermissions(role.Id);

        foreach(String singlePermission in permissions)
        {
            rolePermissions.Permissions[singlePermission] = PermissionValue.Passthrough;
        }

        this.__role_service.WriteRolePermissions(rolePermissions);

        return ValueTask.CompletedTask;
    }

    public async ValueTask UseFallbacks(DiscordUser user, IEnumerable<String> permissions)
    {
        foreach(String singlePermission in permissions)
        {
            await this.UseFallback(user, singlePermission);
        }
    }

    public async ValueTask UseFallbacks(DiscordRole role, IEnumerable<String> permissions)
    {
        foreach(String singlePermission in permissions)
        {
            await this.UseFallback(role, singlePermission);
        }
    }

    public ValueTask SetAdministrator(DiscordUser user, Boolean administrator)
    {
        UserPermissions userPermissions = this.__user_service.GetUserPermissions(user.Id)
            ?? this.__user_service.CreateUserPermissions(user.Id);

        this.__user_service.WriteUserPermissions(userPermissions with { IsAdministrator = administrator });

        return ValueTask.CompletedTask;
    }

    public ValueTask SetAdministrator(DiscordRole role, Boolean administrator)
    {
        RolePermissions rolePermissions = this.__role_service.GetRolePermissions(role.Id)
            ?? this.__role_service.CreateRolePermissions(role.Id);

        this.__role_service.WriteRolePermissions(rolePermissions with { IsAdministrator = administrator });

        return ValueTask.CompletedTask;
    }
}
