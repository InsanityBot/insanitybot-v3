namespace InsanityBot.Extensions.Permissions.Objects;

using System;
using System.Text.Json.Serialization;

public record struct PermissionManifestEntry
{
    [JsonPropertyName("permission")]
    public String Permission { get; set; }

    [JsonPropertyName("value")]
    public Boolean Value { get; set; }
}
