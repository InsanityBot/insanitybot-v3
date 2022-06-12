namespace InsanityBot.Extensions.Timers;

using System;
using System.Text.Json.Serialization;

using InsanityBot.Extensions.Datafixers;

public record TimedObject : IDatafixable
{
    [JsonPropertyName("data_version")]
    public String DataVersion { get; set; } = null!;

    [JsonPropertyName("expiry")]
    public DateTimeOffset Expiry { get; set; }

    [JsonPropertyName("additional_data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public String? AdditionalData { get; set; }

    [JsonPropertyName("guid")]
    public Guid Guid { get; set; }
}
