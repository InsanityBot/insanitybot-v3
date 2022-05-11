namespace InsanityBot.Extensions.Permissions;

using System.Text.Json.Serialization;

using InsanityBot.Extensions.Permissions.Objects;

[JsonSerializable(typeof(UserPermissions), TypeInfoPropertyName = "User")]
[JsonSerializable(typeof(RolePermissions), TypeInfoPropertyName = "Role")]
[JsonSerializable(typeof(DefaultPermissions))]
[JsonSerializable(typeof(PermissionManifest), TypeInfoPropertyName = "Manifest")]
[JsonSerializable(typeof(PermissionMapping), TypeInfoPropertyName = "Mapping")]
internal partial class PermissionSerializationContexts : JsonSerializerContext
{
}
