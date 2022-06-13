namespace InsanityBot.Extensions.Infractions;

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using InsanityBot.Extensions.Configuration;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Starnight.Internal.Entities.Users;

public class InfractionService
{
    private readonly ILogger<InfractionService> __logger;
    private readonly IMemoryCache __cache;
    private readonly IConfiguration __configuration;

    private readonly TimeSpan __cache_expiration;

    public InfractionService(ILogger<InfractionService> logger, IMemoryCache cache, MainConfiguration configuration)
    {
        this.__logger = logger;
        this.__cache = cache;
        this.__configuration = configuration;

        this.__cache_expiration = this.__configuration.Value<TimeSpan>("insanitybot.moderation.infractions.cache_expiration");
    }

    public void SetInfractions(Int64 userId, InfractionCollection infractions)
    {
        this.__cache.GetOrCreate(getCacheKey(userId), xm =>
        {
            xm.SetValue(infractions);
            xm.SetSlidingExpiration(this.__cache_expiration);
            return infractions;
        });

        StreamWriter writer = new($"./data/{userId}/infractions.json");

        writer.Write(JsonSerializer.Serialize(infractions));

        writer.Close();
    }

    public void SetInfractions(DiscordUser user, InfractionCollection infractions)
    {
        this.__cache.GetOrCreate(getCacheKey(user.Id), xm =>
        {
            xm.SetValue(infractions);
            xm.SetSlidingExpiration(this.__cache_expiration);
            return infractions;
        });

        StreamWriter writer = new($"./data/{user.Id}/infractions.json");

        writer.Write(JsonSerializer.Serialize(infractions));

        writer.Close();
    }

    public InfractionCollection GetInfractions(Int64 userId)
    {
        if(!this.__cache.TryGetValue(getCacheKey(userId), out InfractionCollection? infractions) &&
            infractions is not null)
        {
            return infractions;
        }

        if(!Directory.Exists($"./data/{userId}") && !File.Exists($"./data/{userId}/infractions.json"))
        {
            return CreateInfractions(userId);
        }
        else
        {
            StreamReader reader = new($"./data/{userId}/infractions.json");

            InfractionCollection collection = JsonSerializer.Deserialize<InfractionCollection>(reader.ReadToEnd())!;

            this.__cache.CreateEntry(getCacheKey(userId))
                .SetSlidingExpiration(this.__cache_expiration)
                .SetValue(collection);

            reader.Close();

            return collection;
        }
    }

    public InfractionCollection GetInfractions(DiscordUser user)
    {
        InfractionCollection collection;

        if(this.__cache.TryGetValue(getCacheKey(user.Id), out InfractionCollection? infractions) &&
            infractions is not null)
        {
            infractions.LastKnownUsername = user.Username;
            return infractions;
        }

        if(!Directory.Exists($"./data/{user.Id}") || !File.Exists($"./data/{user.Id}/infractions.json"))
        {
            collection = CreateInfractions(user.Id);
        }
        else
        {
            StreamReader reader = new($"./data/{user.Id}/infractions.json");

            collection = JsonSerializer.Deserialize<InfractionCollection>(reader.ReadToEnd())!;

            this.__cache.CreateEntry(getCacheKey(user.Id))
                .SetSlidingExpiration(this.__cache_expiration)
                .SetValue(collection);

            reader.Close();
        }

        collection.LastKnownUsername = user.Username;

        return collection;
    }

    public InfractionCollection CreateInfractions(Int64 userId)
    {
        InfractionCollection infractions = new();

        _ = Task.Run(() =>
        {
            if(!Directory.Exists($"./data/{userId}/infractions.json"))
            {
                Directory.CreateDirectory($"./data/{userId}/infractions.json");
            }

            StreamWriter writer = File.Exists($"./data/{userId}/infractions.json")
                ? new($"./data/{userId}/infractions.json")
                : new(File.Create($"./data/{userId}/infractions.json"));

            writer.Write(JsonSerializer.Serialize(infractions));

            writer.Close();

            this.__cache.CreateEntry(getCacheKey(userId))
                .SetSlidingExpiration(this.__cache_expiration)
                .SetValue(infractions);

            this.__logger.LogDebug("Created infraction file for {userId}", userId);
        });

        return infractions;
    }

    public InfractionCollection CreateInfractions(DiscordUser user)
    {
        InfractionCollection infractions = new()
        {
            LastKnownUsername = user.Username
        };

        _ = Task.Run(() =>
        {
            if(!Directory.Exists($"./data/{user.Id}/infractions.json"))
            {
                Directory.CreateDirectory($"./data/{user.Id}/infractions.json");
            }

            StreamWriter writer = File.Exists($"./data/{user.Id}/infractions.json")
                ? new($"./data/{user.Id}/infractions.json")
                : new(File.Create($"./data/{user.Id}/infractions.json"));

            writer.Write(JsonSerializer.Serialize(infractions));

            writer.Close();

            this.__cache.CreateEntry(getCacheKey(user.Id))
                .SetSlidingExpiration(this.__cache_expiration)
                .SetValue(infractions);

            this.__logger.LogDebug("Created infraction file for {username}#{discriminator} ({id})",
                user.Username, user.Discriminator, user.Id);
        });

        return infractions;
    }

    private static String getCacheKey(Int64 userId)
    {
        return $"InsanityBot.Extensions.Infractions:{userId}";
    }
}
