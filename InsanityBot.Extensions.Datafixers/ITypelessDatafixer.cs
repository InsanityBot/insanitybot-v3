namespace InsanityBot.Extensions.Datafixers;

using System;

// only used in datafixer service implementations
internal interface ITypelessDatafixer
{
    public String OldVersion { get; }
    public String NewVersion { get; }

    public Type Datafixable { get; }
}
