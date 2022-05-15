namespace InsanityBot.Extensions.Permissions.Objects;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using Starnight.Internal.Entities;

public record PermissionMapping
{
    [JsonPropertyName("permit_complex_mapping")]
    public Boolean PermitComplexMapping { get; set; } = false;

    [JsonPropertyName("simple_mapping")]
    public Dictionary<String, DiscordPermissions>? Mapping { get; set; } = new();

    [JsonPropertyName("complex_mapping")]
    public Dictionary<String, DiscordPermissions[]>? ComplexMapping { get; set; } = new();
}
