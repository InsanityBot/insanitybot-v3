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

public class UnsafeConfiguration : IConfiguration
{
    public JsonElement Configuration { get; set; }
    public String DataVersion { get; set; }

    public UnsafeConfiguration
    (
        ILogger<UnsafeConfiguration> logger,
        HttpClient client,
        IDatafixerService datafixer
    )
    {
        logger.LogDebug(LoggerEventIds.UnsafeConfigurationLoading, "Loading unsafe configuration from disk...");

        JsonDocument fullFile;

        if(File.Exists("./config/unsafe.json"))
        {
            StreamReader reader = new("./config/unsafe.json");

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

                logger.LogError("Unsafe configuration could not be parsed, downloading from source tree. " +
                    "This will reset all previously specified options.");

                String commitHash = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>()
                    .Where(xm => xm.Key == "git-revision-hash")
                    .First()
                    .Value!;

                fullFile = ConfigurationDownloader.DownloadConfiguration(commitHash, "unsafe", client).Result;

                logger.LogInformation("Unsafe configuration was successfully restored from the source tree.");
            }
            finally
            {
                reader.Close();
            }
        }
        else
        {
            logger.LogWarning("No unsafe configuration found, downloading from source tree. " +
                "This will reset all previously specified options.");

            String commitHash = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>()
                .Where(xm => xm.Key == "git-revision-hash")
                .First()
                .Value!;

            fullFile = ConfigurationDownloader.DownloadConfiguration(commitHash, "unsafe", client).Result;

            logger.LogInformation("Unsafe configuration was successfully restored from the source tree.");
        }

        // TODO: Datafixer calls

        this.Configuration = fullFile.RootElement;

        JsonPath versionPath = JsonPath.Parse("$.data_version");

        this.DataVersion = versionPath.Evaluate(this.Configuration).Matches![0].Value.Deserialize<String>()!;

        logger.LogDebug(LoggerEventIds.UnsafeConfigurationSuccess, "Successfully loaded unsafe configuration");

        datafixer.ApplyDatafixers(this);

        // lastly, save the now-datafixed config

        _ = Task.Run(() =>
        {
            StreamWriter writer = new("./config/unsafe.json");

            writer.Write(this.Configuration.ToJsonString());

            writer.Close();
        });
    }
}
