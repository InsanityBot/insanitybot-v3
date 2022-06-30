namespace InsanityBot.Extensions.Timers;

using System;
using System.Text.Json;

public static class TimedObjectExtensions
{
    public static String? GetOriginalJsonString
    (
        this TimedObject timer
    )
    {
        return timer.AdditionalData?.Replace("\\\"", "\"");
    }

    public static T? GetOriginalDataObject<T>
    (
        this TimedObject timer
    )
    {
        String unescaped = GetOriginalJsonString(timer)!;

        if(unescaped is null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(unescaped);
    }
}
