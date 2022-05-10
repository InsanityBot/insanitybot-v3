namespace InsanityBot.Extensions.Permissions.Objects;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using Remora.Discord.API.Abstractions.Objects;

public record PermissionMapping
{
    [JsonPropertyName("permit_complex_mapping")]
    public Boolean PermitComplexMapping { get; set; } = false;

    [JsonPropertyName("simple_mapping")]
    public Dictionary<String, DiscordPermission>? Mapping { get; set; } = new();

    [JsonPropertyName("complex_mapping")]
    public Dictionary<String, DiscordPermission[]>? ComplexMapping { get; set; } = new();
}
