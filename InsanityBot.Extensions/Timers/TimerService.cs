namespace InsanityBot.Extensions.Timers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using InsanityBot.Extensions.Datafixers;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

public class TimerService
{
    private readonly ILogger<TimerService> __logger;
    private readonly IMemoryCache __cache;

    public AsyncEvent<TimerService, TimedObject> TimerExpiredEvent { get; } = new();

    public TimerService
    (
        ILogger<TimerService> logger,
        IMemoryCache cache,
        IDatafixerService datafixer
    )
    {
        this.__logger = logger;
        this.__cache = cache;

        this.TimerExpiredEvent.ExceptionHandlers += this.handleEventException;

        if(!Directory.Exists("./cache/timers"))
        {
            Directory.CreateDirectory("./cache/timers");
            return;
        }

        IEnumerable<String> files = Directory.GetFiles("./cache/timers/")
            .Where(xm => xm.StartsWith("./cache/timers/t-"));

        IEnumerable<TimedObject> timers = files
            .AsParallel()
            .Select(xm =>
            {
                StreamReader reader = new(xm);

                TimedObject timer = JsonSerializer.Deserialize<TimedObject>(reader.ReadToEnd())!;

                reader.Close();

                datafixer.ApplyDatafixers(timer);

                _ = Task.Run(() =>
                {
                    StreamWriter writer = new(xm);

                    writer.Write(JsonSerializer.Serialize(timer));

                    writer.Close();
                });

                return timer;
            })
            .AsEnumerable();

        Parallel.ForEach(timers, xm =>
        {
            this.__cache.CreateEntry(xm.Guid)
                .SetAbsoluteExpiration(xm.Expiry)
                .SetValue(xm)
                .RegisterPostEvictionCallback(handleEviction);
        });

        this.__logger.LogInformation("Timer service successfully initialized, {count} timers were loaded", timers.Count());
    }

    public void Register(TimedObject timer)
    {
        _ = Task.Run(() =>
        {
            this.__cache.CreateEntry(timer.Guid)
                .SetAbsoluteExpiration(timer.Expiry)
                .SetValue(timer)
                .RegisterPostEvictionCallback(handleEviction);
        });

        StreamWriter writer = new(File.Create($"./cache/timers/{timer.Guid}"));

        writer.Write(JsonSerializer.Serialize(timer));

        writer.Close();
    }

    private void handleEviction(Object key, Object? value, EvictionReason reason, Object? state)
    {
        if(key is not Guid guid)
        {
            return;
        }

        if(value is null || value is not TimedObject timer)
        {
            return;
        }

        switch(reason)
        {
            case EvictionReason.Replaced:
                this.__logger.LogWarning("Timer cache item {guid} was replaced, likely on accident. Data loss may occur.", guid);
                break;
            case EvictionReason.Removed:
                this.__logger.LogWarning("Timer cache item {guid} was removed externally. Data loss may occur.", guid);
                break;
        }

        _ = Task.Run(() =>
        {
            CancellationTokenSource cts = new();

            this.TimerExpiredEvent.Invoke(this, timer, cts.Token);
        });

        if(File.Exists($"./cache/timers/t-{guid}"))
        {
            File.Delete($"./cache/timers/t-{guid}");
        }
    }

    private ValueTask handleEventException(TimerService sender, TimedObject args, Exception e, CancellationToken token)
    {
        this.__logger.LogError(
            e,
            "Timer expiry event execution for {guid} failed. Affected timer object: {args}",
            args.Guid,
            args.ToString());

        return ValueTask.CompletedTask;
    }
}
