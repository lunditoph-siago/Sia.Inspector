namespace Sia.WebInspector.API.Endpoints;

public static class WorldEndpoints
{
    public static WebApplication RegisterWorldEndpoints(this WebApplication app, string prefix)
    {
        var group = app.MapGroup(prefix);

        group.MapGet("/", (World world) =>
            new {
                AddonCount = world.Addons.Count(),
                EntityCount = world.Count,
                HostCount = world.Hosts.Count
            });

        return app;
    }
}