namespace InsanityBot.Extensions.Localization;

using System;
using System.Text.Json;

using InsanityBot.Extensions.Datafixers;

public class Localization : IDatafixable
{
    public String DataVersion { get; set; } = null!;

    public String Locale { get; set; } = null!;

    public JsonElement Translations { get; set; }
}
