namespace InsanityBot.Extensions.Localization;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

using Json.Path;

public class LocalizationHandler
{
    private readonly Localization __localization;

    public LocalizationHandler
    (
        String locale
    )
    {
        String invariantLocale = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>()
            .Where(xm => xm.Key == "invariant-localization")
            .FirstOrDefault()
            ?.Value ?? "en-us";

        StreamReader reader = File.Exists($"./lang/{locale}.json")
            ? new($"./lang/{locale}.json")
            : new($"./lang/{invariantLocale}.json");

        JsonDocument document = JsonDocument.Parse(reader.ReadToEnd(), new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip
        });

        this.__localization = new()
        {
            Translations = document.RootElement,
            Locale = locale
        };

        JsonPath path = JsonPath.Parse("$.data_version");

        this.__localization.DataVersion = path.Evaluate(document.RootElement).Matches![0].Value.GetString()!;
    }

    public String GetLocalizedString
    (
        String component
    )
    {
        JsonPath path = component.StartsWith("$.")
            ? JsonPath.Parse(component)
            : JsonPath.Parse($"$.translations.{component}");

        return path.Evaluate(this.__localization.Translations).Matches![0].Value.GetString()!;
    }
}
