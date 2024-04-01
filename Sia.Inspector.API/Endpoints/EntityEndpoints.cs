namespace Sia.WebInspector.API.Endpoints;

using System.Text.Json;
using Sia.WebInspector.API.Reponses;
using Sia.WebInspector.API.Services;

public static class EntityEndpoints
{
    public static WebApplication RegisterEntityEndpoints(this WebApplication app, string prefix)
    {
        var group = app.MapGroup(prefix + "entities");

        group.MapGet("/", (World world, HttpContext context) =>
            world.GetEntityIdsResponse(context));

        group.MapGet("/{entityId:long}", (
            long entityId, EntityMapService entityMap, HostMapService hostMap, HttpContext context) =>
            entityMap.ToResult(entityId, entity =>
                Results.Json(new {
                    host = hostMap[entity.Host],
                    comps = entity.GetComponentsResponse(context)
                }, SiaJsonOptions.Component)));

        return app;
    }
}