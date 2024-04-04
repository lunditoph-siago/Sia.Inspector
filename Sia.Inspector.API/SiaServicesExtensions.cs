namespace Sia.Inspector.API;

using Sia.Inspector.API.Services;

public static class SiaServicesExtensions
{
    public static IServiceCollection AddSiaServices(
        this IServiceCollection services, World world, Scheduler mainScheduler, out object worldLock)
    {
        var siaService = new SiaService(world, mainScheduler, Thread.CurrentThread);
        worldLock = siaService.Lock;

        return services
            .AddSingleton(siaService)
            .AddSingleton(new EntityMapService(world))
            .AddSingleton(new HostMapService(world))
            .AddSingleton(new AddonMapService(world));
    }
}