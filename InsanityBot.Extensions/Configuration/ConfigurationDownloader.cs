namespace InsanityBot.Extensions.Configuration;

using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Spectre.Console;
using Spectre.Console.Extensions.Progress;

internal static class ConfigurationDownloader
{
    public static async Task<JsonDocument> DownloadConfiguration
    (
        String commitHash,
        String name,
        HttpClient client
    )
    {
        String url = $"https://raw.githubusercontent.com/insanitybot/insanitybot-v3/{commitHash}/config/release/{name}.json";
        MemoryStream intermediate = new();
        JsonDocument document = null!;

        await AnsiConsole.Progress()
            .Columns(
                new TaskDescriptionColumn()
                {
                    Alignment = Justify.Right
                },
                new ProgressBarColumn()
                {
                    Width = 50,
                    CompletedStyle = new Style(Color.Purple3),
                    FinishedStyle = new Style(Color.MediumSpringGreen)
                },
                new PercentageColumn()
                {
                    Style = new Style(Color.Purple3),
                    CompletedStyle = new Style(Color.MediumSpringGreen)
                },
                new ElapsedTimeColumn(),
                new SpinnerColumn(Spinner.Known.Dots2)
            )
            .WithHttp(client, new HttpRequestMessage()
            {
                RequestUri = new(url),
                Method = HttpMethod.Get
            },
            $"Downloading missing configuration",
            async stream =>
            {
                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(intermediate);

                intermediate.Seek(0, SeekOrigin.Begin);
                stream.Dispose();

                FileStream save = File.Exists($"./config/{name}.json")
                    ? File.OpenWrite($"./config/{name}.json")
                    : File.Create($"./config/{name}.json");

                intermediate.CopyTo(save);
                intermediate.Seek(0, SeekOrigin.Begin);

                save.Flush();
                save.Dispose();

                document = JsonDocument.Parse(intermediate, new JsonDocumentOptions()
                {
                    CommentHandling = JsonCommentHandling.Skip
                });

                intermediate.Dispose();
            })
            .StartAsync();

        return document;
    }
}
