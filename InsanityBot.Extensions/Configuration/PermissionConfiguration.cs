namespace InsanityBot.Extensions.Configuration;

using System;
using System.IO;
using System.Text.Json;

using Json.Path;

using Microsoft.Extensions.Logging;

public class PermissionConfiguration : IConfiguration
{
    public JsonElement Configuration { get; set; }
    public String DataVersion { get; set; }

    public PermissionConfiguration(ILogger<IConfiguration> logger)
    {
        logger.LogDebug(LoggerEventIds.PermissionConfigurationLoading, "Loading permission configuration from disk...");

        StreamReader reader = new("./config/permissions.json");

        JsonDocument fullFile = JsonDocument.Parse(reader.ReadToEnd(), new JsonDocumentOptions()
        {
            CommentHandling = JsonCommentHandling.Allow
        });

        // TODO: backup config handling
        // TODO: Datafixer calls

        this.Configuration = fullFile.RootElement;

        JsonPath versionPath = JsonPath.Parse("$.data_version");

        this.DataVersion = versionPath.Evaluate(this.Configuration).Matches![0].Value.Deserialize<String>()!;

        logger.LogDebug(LoggerEventIds.PermissionConfigurationSuccess, "Successfully loaded permission configuration");
    }
}

