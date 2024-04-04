namespace Sia.Inspector.API.Endpoints;

using Sia.Inspector.API.Services;

public static class AddonEndpoints
{
    public static WebApplication RegisterAddonEndpoints(this WebApplication app, string prefix)
    {
        var group = app.MapGroup(prefix + "addons");

        group.MapGet("/", (SiaService sia, AddonMapService addonMap) => {
            lock (sia.Lock) {
                return sia.World.Addons.Select(
                    addon => new {
                        id = addonMap[addon],
                        type = addon.GetType().ToString()
                    }).ToJsonResult();
            }
        });
        
        group.MapGet("/{addonId:long}", (long addonId, SiaService sia, AddonMapService addonMap) => {
            lock (sia.Lock) {
                return addonMap.ToResult(addonId,
                    addon => Results.Json(new {
                        type = addon.GetType().ToString(),
                        value = addon
                    }, SiaJsonOptions.Addon));
            }
        });

        return app;
    }
}