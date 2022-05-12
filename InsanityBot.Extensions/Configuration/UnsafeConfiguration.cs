namespace InsanityBot.Extensions.Configuration;

using System;
using System.IO;
using System.Text.Json;

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

        // TODO: backup config handling
        // TODO: Datafixer calls

        this.Configuration = fullFile.SelectElement("configuration") ?? new();
        this.DataVersion = fullFile.SelectElement("data_version")?.ToString() ?? String.Empty;

        logger.LogDebug(LoggerEventIds.UnsafeConfigurationSuccess, "Successfully loaded unsafe configuration");
    }
}
