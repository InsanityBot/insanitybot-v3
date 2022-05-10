namespace InsanityBot.Extensions.Permissions;

using System.Text.Json.Serialization;

using InsanityBot.Extensions.Permissions.Objects;

[JsonSerializable(typeof(UserPermissions), TypeInfoPropertyName = "User")]
[JsonSerializable(typeof(RolePermissions), TypeInfoPropertyName = "Role")]
public partial class PermissionSerializationContexts
{
}
