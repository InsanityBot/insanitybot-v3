namespace InsanityBot.Extensions.Permissions.Objects;

using System;
using System.Collections.Generic;

public interface IPermissionObject
{
    public Dictionary<String, PermissionValue> Permissions { get; set; }

    public Boolean IsAdministrator { get; set; }

    public Int64 SnowflakeIdentifier { get; set; }

    public Guid UpdateGuid { get; set; }
}
