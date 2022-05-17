namespace InsanityBot.Extensions.Permissions;

using InsanityBot.Extensions.Permissions.Unsafe;

using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPermissionServices(this IServiceCollection services, PermissionServiceType type)
    {
        services.AddSingleton<DefaultPermissionService>()
            .AddSingleton<RolePermissionService>()
            .AddSingleton<UserPermissionService>();

        switch(type)
        {
            case PermissionServiceType.Default:
                services.AddSingleton<IPermissionService, PermissionService>();
                break;
            case PermissionServiceType.FastUnsafe:
                services.AddSingleton<IPermissionService, UnsafePermissionService>();
                break;
        }

        return services;
    }
}
