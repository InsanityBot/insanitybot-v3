namespace InsanityBot;

using System;
using System.Collections.Generic;

using Serilog.Templates.Themes;

public class LoggerTheme
{
    public static Dictionary<TemplateThemeStyle, String> ThemeStyles { get; } = new()
    {
        [TemplateThemeStyle.LevelDebug] = "\u001b[38;5;212m",
        [TemplateThemeStyle.LevelInformation] = "\u001b[38;5;141m",
        [TemplateThemeStyle.LevelError] = "\u001b[38;5;196m",
        [TemplateThemeStyle.LevelFatal] = "\u001b[38;5;88m"
    };

    public static TemplateTheme Theme { get; } = new(TemplateTheme.Literate, ThemeStyles);
}
