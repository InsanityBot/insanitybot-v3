namespace InsanityBot.Extensions.Permissions.Unsafe;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using InsanityBot.Extensions.Permissions.Objects;

using Starnight.Internal.Entities;
using Starnight.Internal.Entities.Guilds;

public partial class UnsafePermissionService
{
    public async ValueTask RemapPermissions(IEnumerable<DiscordRole> roles)
    {
        await Parallel.ForEachAsync(roles, this.remapSingleRole);
    }

    public async ValueTask RemapPermissions(DiscordGuild guild)
        => await this.RemapPermissions(guild.Roles);

    private ValueTask remapSingleRole(DiscordRole role, CancellationToken token)
    {
        // avoid constructor logic
        Object uninitialized = RuntimeHelpers.GetUninitializedObject(typeof(RolePermissions));
        RolePermissions permissions = Unsafe.As<RolePermissions>(uninitialized);

        permissions.SnowflakeIdentifier = role.Id;
        permissions.Parents = Array.Empty<Int64>();
        // in .NET 7, HasFlag is optimized as a VM intrinsic; we can freely use it for readability.
        permissions.IsAdministrator = role.Permissions.HasFlag(DiscordPermissions.Administrator);
        permissions.UpdateGuid = this.__current_update_guid;

        // return "all passthrough" if the role doesn't have explicit permissions
        // also make sure to re-allocate and not copy, which could well have disastrous consequences.
        if(role.Permissions == 0)
        {
            permissions.Permissions = new(this.__permissions_all_passthrough);
        }

        // if simple mapping is enabled, use that to save on some performance
        if(!this.__mapping.PermitComplexMapping)
        {
            permissions.Permissions = (Dictionary<String, PermissionValue>)this.__mapping.Mapping!
                .AsParallel()
                .Select(xm =>
                {
                    if((role.Permissions & xm.Value) == xm.Value)
                    {
                        return new KeyValuePair<String, PermissionValue>(xm.Key, PermissionValue.Allowed);
                    }
                    else
                    {
                        return new KeyValuePair<String, PermissionValue>(xm.Key, PermissionValue.Passthrough);
                    }
                })
                .AsEnumerable();
        }
        else
        {
            permissions.Permissions = (Dictionary<String, PermissionValue>)this.__mapping.ComplexMapping!
                .AsParallel()
                .Select(xm =>
                {
                    Boolean allow = xm.Value
                        .AsParallel()
                        .All(xm => (role.Permissions & xm) == xm);

                    if(allow)
                    {
                        return new KeyValuePair<String, PermissionValue>(xm.Key, PermissionValue.Allowed);
                    }
                    else
                    {
                        return new KeyValuePair<String, PermissionValue>(xm.Key, PermissionValue.Passthrough);
                    }
                })
                .AsEnumerable();
        }

        this.__role_service.WriteRolePermissions(permissions);

        return ValueTask.CompletedTask;
    }
}
