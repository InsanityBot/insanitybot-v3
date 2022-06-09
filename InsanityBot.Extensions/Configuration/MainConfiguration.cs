namespace InsanityBot.Extensions.Configuration;

using System;
using System.IO;
using System.Text.Json;

using Json.Path;

using Microsoft.Extensions.Logging;

public class MainConfiguration : IConfiguration
{
    public JsonElement Configuration { get; set; }

    public String DataVersion { get; set; } = null!;

    public String Token { get; set; } = null!;

    public Int64 HomeGuildId { get; set; }

    public MainConfiguration(ILogger<MainConfiguration> logger)
    {
        logger.LogDebug(LoggerEventIds.PermissionConfigurationLoading, "Loading permission configuration from disk...");

        StreamReader reader = new("./config/main.json");

        JsonDocument fullFile = JsonDocument.Parse(reader.ReadToEnd(), new JsonDocumentOptions()
        {
            CommentHandling = JsonCommentHandling.Skip
        });

        reader.Close();

        // TODO: backup config handling
        // TODO: Datafixer calls

        this.Configuration = fullFile.RootElement;

        JsonPath versionPath = JsonPath.Parse("$.data_version");

        this.DataVersion = versionPath.Evaluate(this.Configuration).Matches![0].Value.Deserialize<String>()!;

        JsonPath tokenPath = JsonPath.Parse("$.token");

        this.Token = tokenPath.Evaluate(this.Configuration).Matches![0].Value.Deserialize<String>()!;

        JsonPath idPath = JsonPath.Parse("$.home_guild_id");

        this.HomeGuildId = idPath.Evaluate(this.Configuration).Matches![0].Value.Deserialize<Int64>()!;

        logger.LogDebug(LoggerEventIds.PermissionConfigurationSuccess, "Successfully loaded permission configuration");
    }
}
