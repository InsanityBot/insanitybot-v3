namespace InsanityBot.Extensions.Infractions;

using System;
using System.Text.Json.Serialization;

public struct Infraction
{
    [JsonPropertyName("type")]
    public InfractionType Type { get; set; }

    [JsonPropertyName("time")]
    public DateTimeOffset Time { get; set; }

    [JsonPropertyName("reason")]
    public String Reason { get; set; }
}
