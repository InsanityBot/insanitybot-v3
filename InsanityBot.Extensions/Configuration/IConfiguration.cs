namespace InsanityBot.Extensions.Configuration;

using System;
using System.Text.Json;

using InsanityBot.Extensions.Datafixers;

using Json.Path;

public interface IConfiguration : IDatafixable
{
    public JsonElement Configuration { get; }

    public T Value<T>
    (
        String path
    )
    {
        // assume we want it to start with $.configuration.
        // if $.data_version is requested instead, we should know that.
        if(!path.StartsWith('$'))
        {
            path = "$.configuration." + path;
        }

        JsonPath jpath = JsonPath.Parse(path);

        // in InsanityBot's config, all results are unique
        return jpath.Evaluate(this.Configuration).Matches![0].Value.Deserialize<T>()!;
    }
}
