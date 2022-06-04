namespace InsanityBot.Extensions.Permissions.Unsafe;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

using InsanityBot.Extensions.Permissions.Objects;

using Starnight.Internal.Entities.Guilds;
using Starnight.Internal.Entities.Users;

public partial class UnsafePermissionService
{
    public ValueTask<Boolean> CheckPermission(DiscordUser user, String permission)
    {
        UserPermissions userPermissions = this.__user_service.GetUserPermissions(user.Id)
            ?? this.__user_service.CreateUserPermissions(user.Id);

        if(userPermissions.IsAdministrator || userPermissions.Permissions[permission] == PermissionValue.Allowed)
        {
            return ValueTask.FromResult(true);
        }
        else if(userPermissions.Permissions[permission] == PermissionValue.Denied)
        {
            return ValueTask.FromResult(false);
        }

        foreach(Int64 role in userPermissions.AssignedRoles)
        {
            RolePermissions primaryRolePermissions = this.__role_service.GetRolePermissions(role)!;

            if(primaryRolePermissions is null)
            {
                continue;
            }

            if(primaryRolePermissions.IsAdministrator
                || primaryRolePermissions.Permissions[permission] == PermissionValue.Allowed)
            {
                return ValueTask.FromResult(true);
            }
            else if(primaryRolePermissions.Permissions[permission] == PermissionValue.Denied)
            {
                return ValueTask.FromResult(false);
            }

            if(!this.__role_trees.ContainsKey(role))
            {
                continue;
            }

            foreach(Int64 secondaryRole in this.__role_trees[role])
            {
                RolePermissions secondaryRolePermissions = this.__role_service.GetRolePermissions(secondaryRole)!;

                if(secondaryRolePermissions is null)
                {
                    continue;
                }

                if(secondaryRolePermissions.IsAdministrator
                    || primaryRolePermissions.Permissions[permission] == PermissionValue.Allowed)
                {
                    return ValueTask.FromResult(true);
                }
                else if(secondaryRolePermissions.Permissions[permission] == PermissionValue.Denied)
                {
                    return ValueTask.FromResult(false);
                }
            }
        }

        return ValueTask.FromResult(false);
    }

    public ValueTask<Boolean> CheckPermission(DiscordRole role, String permission)
    {
        RolePermissions primaryRolePermissions = this.__role_service.GetRolePermissions(role.Id)
            ?? this.__role_service.CreateRolePermissions(role.Id);

        if(primaryRolePermissions.IsAdministrator
            || primaryRolePermissions.Permissions[permission] == PermissionValue.Allowed)
        {
            return ValueTask.FromResult(true);
        }
        else if(primaryRolePermissions.Permissions[permission] == PermissionValue.Denied)
        {
            return ValueTask.FromResult(false);
        }

        if(!this.__role_trees.ContainsKey(role.Id))
        {
            this.__role_trees.TryAdd(role.Id, buildRoleTree(role.Id));
        }

        foreach(Int64 secondaryRole in this.__role_trees[role.Id])
        {
            RolePermissions secondaryRolePermissions = this.__role_service.GetRolePermissions(secondaryRole)!;

            if(secondaryRolePermissions is null)
            {
                continue;
            }

            if(secondaryRolePermissions.IsAdministrator
                || secondaryRolePermissions.Permissions[permission] == PermissionValue.Allowed)
            {
                return ValueTask.FromResult(true);
            }
            else if(secondaryRolePermissions.Permissions[permission] == PermissionValue.Denied)
            {
                return ValueTask.FromResult(false);
            }
        }
    
        return ValueTask.FromResult(false);
    }

    public ValueTask<Boolean> CheckPermission(DiscordGuildMember member, String permission)
    {
        UserPermissions userPermissions = this.__user_service.GetUserPermissions(member.User!.Id)
            ?? this.__user_service.CreateUserPermissions(member.User!.Id);

        if(userPermissions.IsAdministrator || userPermissions.Permissions[permission] == PermissionValue.Allowed)
        {
            return ValueTask.FromResult(true);
        }
        else if(userPermissions.Permissions[permission] == PermissionValue.Denied)
        {
            return ValueTask.FromResult(false);
        }

        IEnumerable<Int64> roles = member.Roles is null
            ? userPermissions.AssignedRoles
            : Enumerable.Concat(userPermissions.AssignedRoles, member.Roles);

        foreach(Int64 role in roles)
        {
            RolePermissions primaryRolePermissions = this.__role_service.GetRolePermissions(role)!;

            if(primaryRolePermissions is null)
            {
                continue;
            }

            if(primaryRolePermissions.IsAdministrator
                || primaryRolePermissions.Permissions[permission] == PermissionValue.Allowed)
            {
                return ValueTask.FromResult(true);
            }
            else if(primaryRolePermissions.Permissions[permission] == PermissionValue.Denied)
            {
                return ValueTask.FromResult(false);
            }

            if(!this.__role_trees.ContainsKey(role))
            {
                continue;
            }

            foreach(Int64 secondaryRole in this.__role_trees[role])
            {
                RolePermissions secondaryRolePermissions = this.__role_service.GetRolePermissions(secondaryRole)!;

                if(secondaryRolePermissions is null)
                {
                    continue;
                }

                if(secondaryRolePermissions.IsAdministrator
                    || primaryRolePermissions.Permissions[permission] == PermissionValue.Allowed)
                {
                    return ValueTask.FromResult(true);
                }
                else if(secondaryRolePermissions.Permissions[permission] == PermissionValue.Denied)
                {
                    return ValueTask.FromResult(false);
                }
            }
        }

        return ValueTask.FromResult(false);
    }

    public ValueTask<Boolean> CheckAdministrator(DiscordUser user)
    {
        UserPermissions userPermissions = this.__user_service.GetUserPermissions(user.Id)
            ?? this.__user_service.CreateUserPermissions(user.Id);

        if(userPermissions.IsAdministrator)
        {
            return ValueTask.FromResult(true);
        }

        foreach(Int64 role in userPermissions.AssignedRoles)
        {
            RolePermissions primaryRolePermissions = this.__role_service.GetRolePermissions(role)!;

            if(primaryRolePermissions is null)
            {
                continue;
            }

            if(primaryRolePermissions.IsAdministrator)
            {
                return ValueTask.FromResult(true);
            }

            if(!this.__role_trees.ContainsKey(role))
            {
                continue;
            }

            foreach(Int64 secondaryRole in this.__role_trees[role])
            {
                RolePermissions secondaryRolePermissions = this.__role_service.GetRolePermissions(secondaryRole)!;

                if(secondaryRolePermissions is null)
                {
                    continue;
                }

                if(secondaryRolePermissions.IsAdministrator)
                {
                    return ValueTask.FromResult(true);
                }
            }
        }

        return ValueTask.FromResult(false);
    }

    public ValueTask<Boolean> CheckAdministrator(DiscordRole role)
    {
        RolePermissions primaryRolePermissions = this.__role_service.GetRolePermissions(role.Id)
             ?? this.__role_service.CreateRolePermissions(role.Id);

        if(primaryRolePermissions.IsAdministrator)
        {
            return ValueTask.FromResult(true);
        }

        if(!this.__role_trees.ContainsKey(role.Id))
        {
            this.__role_trees.TryAdd(role.Id, buildRoleTree(role.Id));
        }

        foreach(Int64 secondaryRole in this.__role_trees[role.Id])
        {
            RolePermissions secondaryRolePermissions = this.__role_service.GetRolePermissions(secondaryRole)!;

            if(secondaryRolePermissions is null)
            {
                continue;
            }

            if(secondaryRolePermissions.IsAdministrator)
            {
                return ValueTask.FromResult(true);
            }
        }

        return ValueTask.FromResult(false);
    }

    private Int64[] buildRoleTree(Int64 role)
    {
        RolePermissions initial = this.__role_service.GetRolePermissions(role)
            ?? this.__role_service.CreateRolePermissions(role);

        List<Int64> roles = initial.Parents.ToList();

        for(Int32 i = 0; i < roles.Count; i++)
        {
            RolePermissions current = this.__role_service.GetRolePermissions(roles[i])!;

            if(current is null)
            {
                continue;
            }

            roles.AddRange(current.Parents);
            roles = roles.Distinct().ToList();
        }

        return roles.ToArray();
    }

    private void rebuildRoleTrees()
    {
        IEnumerable<Int64> rootIds = this.__role_trees.Keys;

        this.__role_trees.Clear();

        _ = rootIds
            .AsParallel()
            // aggressive misuse of .All but what are you going to do about it
            // could potentially add failure handling here, since a single failed registration
            // will be known.
            .All(root =>
            {
                RolePermissions initial = this.__role_service.GetRolePermissions(root)
                    ?? this.__role_service.CreateRolePermissions(root);

                List<Int64> roles = initial.Parents.ToList();

                for(Int32 i = 0; i < roles.Count; i++)
                {
                    RolePermissions current = this.__role_service.GetRolePermissions(roles[i])!;

                    if(current is null)
                    {
                        continue;
                    }

                    roles.AddRange(current.Parents);
                    roles = roles.Distinct().ToList();
                }

                return this.__role_trees.TryAdd(root, roles);
            });
    }
}
