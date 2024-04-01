namespace Sia.WebInspector.API.Endpoints;

using Sia.WebInspector.API.Reponses;
using Sia.WebInspector.API.Services;

public static class AddonEndpoints
{
    public static WebApplication RegisterAddonEndpoints(this WebApplication app, string prefix)
    {
        var group = app.MapGroup(prefix + "addons");

        group.MapGet("/", (World world, AddonMapService addonMap) =>
            world.Addons.Select(addon =>
                new {
                    id = addonMap[addon],
                    type = addon.GetType().ToString()
                }));
        
        group.MapGet("/{addonId:long}", (long addonId, AddonMapService addonMap) =>
            addonMap.ToResult(addonId, addon =>
                Results.Json(new {
                    type = addon.GetType().ToString(),
                    value = addon
                }, SiaJsonOptions.Addon)));

        return app;
    }
}