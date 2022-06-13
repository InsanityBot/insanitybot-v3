namespace InsanityBot.Extensions.Infractions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class InfractionCollection : IEnumerable<Infraction>
{
    [JsonPropertyName("username")]
    public String? LastKnownUsername { get; set; }

    [JsonPropertyName("infractions")]
    public List<Infraction> Infractions { get; set; } = new();

    public IEnumerator<Infraction> GetEnumerator()
    {
        return Infractions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}
