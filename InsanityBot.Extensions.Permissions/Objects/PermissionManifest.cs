namespace InsanityBot.Extensions.Permissions.Objects;

using System;
using System.Text.Json.Serialization;

public record PermissionManifest
{
    [JsonPropertyName("manifest")]
    public PermissionManifestEntry[] Manifest { get; set; } = Array.Empty<PermissionManifestEntry>();
}
