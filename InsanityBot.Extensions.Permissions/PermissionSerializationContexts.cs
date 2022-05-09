namespace InsanityBot.Extensions.Permissions;

using System.Text.Json.Serialization;

using InsanityBot.Extensions.Permissions.Objects;

[JsonSerializable(typeof(UserPermissions), TypeInfoPropertyName = "User")]
public partial class PermissionSerializationContexts
{
}
