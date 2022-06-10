namespace InsanityBot.Extensions.Configuration;

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;

using Json.Path;

using Microsoft.Extensions.Logging;

public class MainConfiguration : IConfiguration
{
    public JsonElement Configuration { get; set; }

    public String DataVersion { get; set; } = null!;

    public String Token { get; set; } = null!;

    public Int64 HomeGuildId { get; set; }

    public MainConfiguration(ILogger<MainConfiguration> logger, HttpClient client)
    {
        logger.LogDebug("Loading main configuration from disk...");

        JsonDocument fullFile;

        if(File.Exists("./config/main.json"))
        {
            StreamReader reader = new("./config/main.json");

            try
            {
                fullFile = JsonDocument.Parse(reader.ReadToEnd(), new JsonDocumentOptions()
                {
                    CommentHandling = JsonCommentHandling.Skip
                });
            }
            catch
            {
                reader.Close();

                logger.LogError("\n\tMain configuration could not be parsed, downloading from source tree.\n\t" +
                    "This will reset all previously specified options.\n\t" +
                    "You will need to specify at least a token and a home guild ID and then restart InsanityBot.");

                String commitHash = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>()
                .Where(xm => xm.Key == "git-revision-hash")
                .First()
                .Value!;

                fullFile = ConfigurationDownloader.DownloadConfiguration(commitHash, "main", client).Result;

                logger.LogInformation("Main configuration was successfully restored from the source tree.");
            }

            reader.Close();
        }
        else
        {
            logger.LogWarning("\n\tNo main configuration found, downloading from source tree.\n\t" +
                "This will reset all previously specified options.\n\t" +
                "You will need to specify at least a token and a home guild ID and then restart InsanityBot.");

            String commitHash = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>()
                .Where(xm => xm.Key == "git-revision-hash")
                .First()
                .Value!;

            fullFile = ConfigurationDownloader.DownloadConfiguration(commitHash, "main", client).Result;

            logger.LogInformation("Main configuration was successfully restored from the source tree.");
        }

        // TODO: Datafixer calls

        this.Configuration = fullFile.RootElement;

        JsonPath versionPath = JsonPath.Parse("$.data_version");

        this.DataVersion = versionPath.Evaluate(this.Configuration).Matches![0].Value.Deserialize<String>()!;

        JsonPath tokenPath = JsonPath.Parse("$.token");

        this.Token = tokenPath.Evaluate(this.Configuration).Matches![0].Value.Deserialize<String>()!;

        JsonPath idPath = JsonPath.Parse("$.home_guild_id");

        this.HomeGuildId = idPath.Evaluate(this.Configuration).Matches![0].Value.Deserialize<Int64>()!;

        logger.LogDebug("Successfully loaded main configuration");
    }
}
