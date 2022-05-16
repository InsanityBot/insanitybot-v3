namespace InsanityBot.Extensions.Permissions.Objects;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public record RolePermissions : IPermissionObject
{
    [JsonPropertyName("parents")]
    public Int64[] Parents { get; set; } = Array.Empty<Int64>();

    [JsonPropertyName("permissions")]
    public Dictionary<String, PermissionValue> Permissions { get; set; } = new();

    [JsonPropertyName("is_administrator")]
    public Boolean IsAdministrator { get; set; } = false;

    [JsonPropertyName("snowflake")]
    public Int64 SnowflakeIdentifier { get; set; } = 0;

    [JsonPropertyName("update_guid")]
    public Guid UpdateGuid { get; set; } = Guid.Empty;
}
