namespace InsanityBot.Extensions.Datafixers;

using System;

public interface IDatafixable
{
    public String DataVersion { get; set; }
}
