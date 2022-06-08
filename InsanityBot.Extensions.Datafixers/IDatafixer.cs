namespace InsanityBot.Extensions.Datafixers;

using System;

public interface IDatafixer
{
    public String OldVersion { get; }
    public String NewVersion { get; }

    public Type Datafixable { get; }
}
