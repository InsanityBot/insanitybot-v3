namespace InsanityBot.Extensions.Permissions.Objects;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public record DefaultPermissions : IPermissionObject
{
    [JsonPropertyName("fallback_default")]
    public PermissionValue FallbackDefault { get; set; }

    [JsonPropertyName("permissions")]
    public Dictionary<String, PermissionValue> Permissions { get; set; } = new();

    [JsonPropertyName("is_administrator")]
    public Boolean IsAdministrator { get; set; } = false;

    [JsonPropertyName("snowflake")]
    public Int64 SnowflakeIdentifier { get; set; } = 0;

    [JsonPropertyName("update_guid")]
    public Guid UpdateGuid { get; set; } = Guid.Empty;
}
