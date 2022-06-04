namespace InsanityBot.Extensions.Permissions.Unsafe;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Starnight.Internal.Entities.Guilds;

using Starnight.Internal.Entities.Users;

public partial class UnsafePermissionService
{
    public async ValueTask<Boolean> CheckAnyPermission(DiscordUser user, IEnumerable<String> permissions)
    {
        IEnumerable<Task<Boolean>> tasks = permissions
            .Select(xm => this.CheckPermission(user, xm).AsTask());

        return (await Task.WhenAll(tasks)).Any(xm => xm);
    }

    public async ValueTask<Boolean> CheckAnyPermission(DiscordRole role, IEnumerable<String> permissions)
    {
        IEnumerable<Task<Boolean>> tasks = permissions
            .Select(xm => this.CheckPermission(role, xm).AsTask());

        return (await Task.WhenAll(tasks)).Any(xm => xm);
    }

    public async ValueTask<Boolean> CheckAnyPermission(DiscordGuildMember member, IEnumerable<String> permissions)
    {
        IEnumerable<Task<Boolean>> tasks = permissions
            .Select(xm => this.CheckPermission(member, xm).AsTask());

        return (await Task.WhenAll(tasks)).Any(xm => xm);
    }

    public async ValueTask<Boolean> CheckAllPermissions(DiscordUser user, IEnumerable<String> permissions)
    {
        IEnumerable<Task<Boolean>> tasks = permissions
            .Select(xm => this.CheckPermission(user, xm).AsTask());

        return (await Task.WhenAll(tasks)).All(xm => xm);
    }

    public async ValueTask<Boolean> CheckAllPermissions(DiscordRole role, IEnumerable<String> permissions)
    {
        IEnumerable<Task<Boolean>> tasks = permissions
            .Select(xm => this.CheckPermission(role, xm).AsTask());

        return (await Task.WhenAll(tasks)).All(xm => xm);
    }

    public async ValueTask<Boolean> CheckAllPermissions(DiscordGuildMember member, IEnumerable<String> permissions)
    {
        IEnumerable<Task<Boolean>> tasks = permissions
            .Select(xm => this.CheckPermission(member, xm).AsTask());

        return (await Task.WhenAll(tasks)).All(xm => xm);
    }
}
