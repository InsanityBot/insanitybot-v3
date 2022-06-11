namespace InsanityBot.Extensions.Configuration;

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

using InsanityBot.Extensions.Datafixers;

using Json.More;
using Json.Path;

using Microsoft.Extensions.Logging;

public class PermissionConfiguration : IConfiguration
{
    public JsonElement Configuration { get; set; }
    public String DataVersion { get; set; }

    public PermissionConfiguration
    (
        ILogger<PermissionConfiguration> logger,
        HttpClient client,
        IDatafixerService datafixer
    )
    {
        logger.LogDebug(LoggerEventIds.PermissionConfigurationLoading, "Loading permission configuration from disk...");

        JsonDocument fullFile;

        if(File.Exists("./config/permissions.json"))
        {
            StreamReader reader = new("./config/permissions.json");

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

                logger.LogError("Permission configuration could not be parsed, downloading from source tree. " +
                    "This will reset all previously specified options.");

                String commitHash = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>()
                    .Where(xm => xm.Key == "git-revision-hash")
                    .First()
                    .Value!;

                fullFile = ConfigurationDownloader.DownloadConfiguration(commitHash, "permissions", client).Result;

                logger.LogInformation("Permission configuration was successfully restored from the source tree.");
            }
            finally
            {
                reader.Close();
            }
        }
        else
        {
            logger.LogWarning("No permission configuration found, downloading from source tree. " +
                "This will reset all previously specified options.");

            String commitHash = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>()
                .Where(xm => xm.Key == "git-revision-hash")
                .First()
                .Value!;

            fullFile = ConfigurationDownloader.DownloadConfiguration(commitHash, "permissions", client).Result;

            logger.LogInformation("Permission configuration was successfully restored from the source tree.");
        }

        this.Configuration = fullFile.RootElement;

        JsonPath versionPath = JsonPath.Parse("$.data_version");

        this.DataVersion = versionPath.Evaluate(this.Configuration).Matches![0].Value.Deserialize<String>()!;

        logger.LogDebug(LoggerEventIds.PermissionConfigurationSuccess, "Successfully loaded permission configuration");

        datafixer.ApplyDatafixers(this);

        // lastly, save the now-datafixed config

        _ = Task.Run(() =>
        {
            StreamWriter writer = new("./config/permissions.json");

            writer.Write(this.Configuration.ToJsonString());

            writer.Close();
        });
    }
}

