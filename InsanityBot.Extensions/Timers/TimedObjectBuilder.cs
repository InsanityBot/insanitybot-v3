namespace InsanityBot.Extensions.Timers;

using System;
using System.Text.Json;

public class TimedObjectBuilder
{
    private String __data_version = "3.0.0.1";
    private DateTimeOffset __expiry;
    private String? __additional_data;
    private Guid __guid;


    public TimedObjectBuilder()
    {
        this.__guid = Guid.NewGuid();
        this.__expiry = DateTimeOffset.UtcNow;
    }

    public TimedObjectBuilder(TimedObject timer)
    {
        this.__data_version = timer.DataVersion;
        this.__expiry = timer.Expiry;
        this.__additional_data = timer.AdditionalData;
        this.__guid = timer.Guid;
    }

    public TimedObjectBuilder(TimedObjectBuilder builder)
    {
        this.__data_version = builder.__data_version;
        this.__expiry = builder.__expiry;
        this.__additional_data = builder.__additional_data;
        this.__guid = builder.__guid;
    }

    public TimedObjectBuilder WithDataVersion(String dataVersion)
    {
        this.__data_version = dataVersion;
        return this;
    }

    public TimedObjectBuilder WithExpiry(DateTimeOffset expiry)
    {
        this.__expiry = expiry;
        return this;
    }

    public TimedObjectBuilder WithAdditionalData(String additionalData)
    {
        this.__additional_data = additionalData;
        return this;
    }

    public TimedObjectBuilder WithAdditionalData(Object additionalData)
    {
        // escape json
        this.__additional_data = JsonSerializer.Serialize(additionalData).Replace("\"", "\\\"");
        return this;
    }

    public TimedObjectBuilder WithGuid(Guid guid)
    {
        this.__guid = guid;
        return this;
    }

    public TimedObject Build()
    {
        return new TimedObject
        {
            DataVersion = this.__data_version,
            Expiry = this.__expiry,
            AdditionalData = this.__additional_data,
            Guid = this.__guid
        };
    }
}
