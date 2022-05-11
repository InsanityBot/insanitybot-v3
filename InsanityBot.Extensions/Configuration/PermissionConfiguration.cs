namespace InsanityBot.Extensions.Configuration;

using System;
using System.IO;
using System.Text.Json;

using Microsoft.Extensions.Logging;

public class PermissionConfiguration : IConfiguration
{
    public JsonElement Configuration { get; set; }
    public String DataVersion { get; set; }

    public PermissionConfiguration(ILogger<IConfiguration> logger)
    {
        logger.LogDebug(LoggerEventIds.PermissionConfigurationLoading, "Loading permission configuration from disk...");

        StreamReader reader = new("./config/permissions.json");

        JsonDocument fullFile = JsonDocument.Parse(reader.ReadToEnd());

        // TODO: backup config handling
        // TODO: Datafixer calls

        this.Configuration = fullFile.SelectElement("configuration") ?? new();
        this.DataVersion = fullFile.SelectElement("data_version")?.ToString() ?? String.Empty;

        logger.LogDebug(LoggerEventIds.PermissionConfigurationSuccess, "Successfully loaded permission configuration");
    }
}

