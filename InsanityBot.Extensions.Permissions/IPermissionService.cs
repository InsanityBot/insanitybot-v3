namespace InsanityBot.Extensions.Permissions;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using InsanityBot.Extensions.Permissions.Objects;

using Starnight.Internal.Entities.Guilds;
using Starnight.Internal.Entities.Users;

public interface IPermissionService
{
    // ---- lower-level capability ---- //

    public ValueTask<DefaultPermissions> GetDefaultPermissions();
    public ValueTask<RolePermissions> GetRolePermissions(DiscordRole role);
    public ValueTask<UserPermissions> GetUserPermissions(DiscordUser user);

    public ValueTask SetDefaultPermissions(DefaultPermissions defaultPermissions);
    public ValueTask SetRolePermissions(RolePermissions rolePermissions);
    public ValueTask SetUserPermissions(UserPermissions userPermissions);
           
    public ValueTask CreateRolePermissions(DiscordRole role);
    public ValueTask CreateRolePermissions(RolePermissions permissions);
    public ValueTask CreateUserPermissions(DiscordUser user);
    public ValueTask CreateUserPermissions(UserPermissions permissions);


    // ---- higher-level capability ---- //

    public ValueTask RemapPermissions(IEnumerable<DiscordRole> roles);
    public ValueTask RemapPermissions(DiscordGuild guild);

    public ValueTask<Boolean> CheckPermission(DiscordUser user, String permission);
    public ValueTask<Boolean> CheckPermission(DiscordRole role, String permission);
    public ValueTask<Boolean> CheckPermission(DiscordGuildMember member, String permission);
    public ValueTask<Boolean> CheckAnyPermission(DiscordUser user, IEnumerable<String> permissions);
    public ValueTask<Boolean> CheckAnyPermission(DiscordRole role, IEnumerable<String> permissions);
    public ValueTask<Boolean> CheckAnyPermission(DiscordGuildMember member, IEnumerable<String> permissions);
    public ValueTask<Boolean> CheckAllPermissions(DiscordUser user, IEnumerable<String> permissions);
    public ValueTask<Boolean> CheckAllPermissions(DiscordRole role, IEnumerable<String> permissions);
    public ValueTask<Boolean> CheckAllPermissions(DiscordGuildMember member, IEnumerable<String> permissions);
    public ValueTask<Boolean> CheckAdministrator(DiscordUser user);
    public ValueTask<Boolean> CheckAdministrator(DiscordRole role);

    public ValueTask GrantPermission(DiscordUser user, String permission);
    public ValueTask GrantPermission(DiscordRole role, String permission);
    public ValueTask GrantPermissions(DiscordUser user, IEnumerable<String> permissions);
    public ValueTask GrantPermissions(DiscordRole role, IEnumerable<String> permissions);

    public ValueTask RevokePermission(DiscordUser user, String permission);
    public ValueTask RevokePermission(DiscordRole role, String permission);
    public ValueTask RevokePermissions(DiscordUser user, IEnumerable<String> permissions);
    public ValueTask RevokePermissions(DiscordRole role, IEnumerable<String> permissions);

    public ValueTask UseFallback(DiscordUser user, String permission);
    public ValueTask UseFallback(DiscordRole role, String permission);
    public ValueTask UseFallbacks(DiscordUser user, IEnumerable<String> permissions);
    public ValueTask UseFallbacks(DiscordRole role, IEnumerable<String> permissions);

    public ValueTask SetAdministrator(DiscordUser user, Boolean administrator);
    public ValueTask SetAdministrator(DiscordRole role, Boolean administrator);

    public ValueTask SetParent(DiscordUser user, Int64 parent);
    public ValueTask SetParent(DiscordRole role, Int64 parent);
    public ValueTask SetParents(DiscordUser user, IEnumerable<Int64> parent);
    public ValueTask SetParents(DiscordRole role, IEnumerable<Int64> parent);

    public ValueTask RestoreDefaults(DiscordUser user);
    public ValueTask RestoreDefaults(DiscordRole role);
    public ValueTask RestoreManifestDefaults();
}
