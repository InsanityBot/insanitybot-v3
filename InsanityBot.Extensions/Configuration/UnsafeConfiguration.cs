namespace InsanityBot.Extensions.Configuration;

using System;
using System.IO;
using System.Text.Json;

using Json.Path;

using Microsoft.Extensions.Logging;

public class UnsafeConfiguration : IConfiguration
{
    public JsonElement Configuration { get; set; }
    public String DataVersion { get; set; }

    public UnsafeConfiguration(ILogger<IConfiguration> logger)
    {
        logger.LogDebug(LoggerEventIds.UnsafeConfigurationLoading, "Loading unsafe configuration from disk...");

        StreamReader reader = new("./config/unsafe.json");

        JsonDocument fullFile = JsonDocument.Parse(reader.ReadToEnd(), new JsonDocumentOptions()
        {
            CommentHandling = JsonCommentHandling.Allow
        });

        reader.Close();

        // TODO: backup config handling
        // TODO: Datafixer calls

        this.Configuration = fullFile.RootElement;

        JsonPath versionPath = JsonPath.Parse("$.data_version");

        this.DataVersion = versionPath.Evaluate(this.Configuration).Matches![0].Value.Deserialize<String>()!;

        logger.LogDebug(LoggerEventIds.UnsafeConfigurationSuccess, "Successfully loaded unsafe configuration");
    }
}
