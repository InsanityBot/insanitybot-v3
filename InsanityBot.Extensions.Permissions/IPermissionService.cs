namespace InsanityBot.Extensions.Permissions;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using InsanityBot.Extensions.Permissions.Objects;

using Remora.Discord.API.Abstractions.Objects;
using Remora.Results;

public interface IPermissionService
{
    // ---- lower-level capability ---- //

    public ValueTask<DefaultPermissions> GetDefaultPermissions();
    public ValueTask<Result<RolePermissions>> GetRolePermissions(IPartialRole role);
    public ValueTask<Result<UserPermissions>> GetUserPermissions(IPartialUser user);

    public ValueTask SetDefaultPermissions(DefaultPermissions defaultPermissions);
    public ValueTask SetRolePermissions(RolePermissions rolePermissions);
    public ValueTask SetUserPermissions(UserPermissions userPermissions);
           
    public ValueTask<IResult> CreateRolePermissions(IPartialRole role);
    public ValueTask CreateRolePermissions(RolePermissions permissions);
    public ValueTask<IResult> CreateUserPermissions(IPartialUser user);
    public ValueTask CreateUserPermissions(UserPermissions permissions);


    // ---- higher-level capability ---- //

    public ValueTask<IResult> RemapPermissions(IEnumerable<IPartialRole> roles);
    public ValueTask<IResult> RemapPermissions(IPartialGuild guild);

    public ValueTask<Result<Boolean>> CheckPermission(IPartialUser user, String permission);
    public ValueTask<Result<Boolean>> CheckPermission(IPartialRole role, String permission);
    public ValueTask<Result<Boolean>> CheckAnyPermission(IPartialUser user, IEnumerable<String> permissions);
    public ValueTask<Result<Boolean>> CheckAnyPermission(IPartialRole role, IEnumerable<String> permissions);
    public ValueTask<Result<Boolean>> CheckAllPermissions(IPartialUser user, IEnumerable<String> permissions);
    public ValueTask<Result<Boolean>> CheckAllPermissions(IPartialRole role, IEnumerable<String> permissions);

    public ValueTask<IResult> GrantPermission(IPartialUser user, String permission);
    public ValueTask<IResult> GrantPermission(IPartialRole role, String permission);
    public ValueTask<IResult> GrantPermissions(IPartialUser user, IEnumerable<String> permissions);
    public ValueTask<IResult> GrantPermissions(IPartialRole user, IEnumerable<String> permissions);

    public ValueTask<IResult> RevokePermission(IPartialUser user, String permission);
    public ValueTask<IResult> RevokePermission(IPartialRole role, String permission);
    public ValueTask<IResult> RevokePermissions(IPartialUser user, IEnumerable<String> permissions);
    public ValueTask<IResult> RevokePermissions(IPartialRole user, IEnumerable<String> permissions);

    public ValueTask<IResult> UseFallback(IPartialUser user, String permission);
    public ValueTask<IResult> UseFallback(IPartialRole role, String permission);
    public ValueTask<IResult> UseFallbacks(IPartialUser user, IEnumerable<String> permissions);
    public ValueTask<IResult> UseFallbacks(IPartialRole user, IEnumerable<String> permissions);

    public ValueTask<IResult> SetAdministrator(IPartialUser user, Boolean administrator);
    public ValueTask<IResult> SetAdministrator(IPartialRole role, Boolean administrator);

    public ValueTask<IResult> SetParent(IPartialUser user, UInt64 parent);
    public ValueTask<IResult> SetParent(IPartialRole role, UInt64 parent);
    public ValueTask<IResult> SetParents(IPartialUser user, IEnumerable<UInt64> parent);
    public ValueTask<IResult> SetParents(IPartialRole role, IEnumerable<UInt64> parent);

    public ValueTask<IResult> RestoreDefaults(IPartialUser user);
    public ValueTask<IResult> RestoreDefaults(IPartialRole role);
    public ValueTask RestoreManifestDefaults();


    // ---- safety capability ---- //

    public ValueTask<IResult> EnsureDefaultFileIntegrity();
    public ValueTask<IResult> EnsureFileIntegrity();
    public ValueTask<IResult> EnsureFileIntegrity(IPartialUser user);
    public ValueTask<IResult> EnsureFileIntegrity(IPartialRole role);
}
