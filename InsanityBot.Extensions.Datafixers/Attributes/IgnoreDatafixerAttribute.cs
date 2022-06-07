namespace InsanityBot.Extensions.Datafixers.Attributes;

using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class IgnoreDatafixerAttribute : Attribute
{
    // no body since we only check for its presence
}
