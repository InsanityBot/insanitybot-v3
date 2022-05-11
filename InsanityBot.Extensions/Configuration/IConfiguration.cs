namespace InsanityBot.Extensions.Configuration;

using System;
using System.Text.Json;

using InsanityBot.Extensions.Datafixers;

public interface IConfiguration : IDatafixable
{
    public JsonElement Configuration { get; }

    public T? Value<T>(String path)
    {
        return (T?)Configuration.SelectElement(path)?.Deserialize(typeof(T));
    }
}
