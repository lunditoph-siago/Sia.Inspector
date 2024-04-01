namespace Sia.WebInspector.API;

using Sia.WebInspector.API.Services;

public static class SiaServicesExtensions
{
    public static IServiceCollection AddSiaServices(this IServiceCollection services, World world)
    {
        return services
            .AddSingleton(world)
            .AddSingleton(new EntityMapService(world))
            .AddSingleton(new HostMapService(world))
            .AddSingleton(new AddonMapService(world));
    }
}