namespace InsanityBot.Extensions.Permissions.Unsafe;

using System;
using System.Collections.Generic;

public partial class UnsafePermissionService : IPermissionService
{
    private readonly IEnumerable<String> __permissions;

    public UnsafePermissionService
    (
    )
    {
        // we only allocate 32 bit of memory through this entire block.
        // this could almost fit into a slim, read: low-memory implementation of IPermissionService,
        // however other parts of UnsafePermissionService couldn't.
        this.__wildcards = new Char[] { '*', '?' }.AsMemory();
        this.__tolerate_anything_pattern = this.__wildcards[..1];
    }
}
