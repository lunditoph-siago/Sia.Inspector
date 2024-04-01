namespace Sia.WebInspector.API.Endpoints;

using Sia.WebInspector.API.Reponses;
using Sia.WebInspector.API.Services;

public static class HostEndpoints
{
    public static WebApplication RegisterHostEndpoints(this WebApplication app, string prefix)
    {
        var group = app.MapGroup(prefix + "hosts");

        group.MapGet("/", (HostMapService hostMap) =>
            hostMap.World.Hosts
                .Select(host => new {
                    Id = hostMap[host],
                    host.Count,
                    Desc = host.Descriptor.GetComponentsResponse()
                }));

        group.MapGet("/ids", (HostMapService hostMap) =>
            hostMap.World.Hosts
                .Select(host => hostMap[host]));

        group.MapGet("/{hostId:long}", (long hostId, HostMapService hostMap) =>
            hostMap.ToResult(hostId, host =>
                Results.Json(new {
                    host.Capacity,
                    host.Count,
                    Desc = host.Descriptor.GetComponentsResponse()
                })));

        group.MapGet("/{hostId:long}/entities", (long hostId, HostMapService hostMap) =>
            hostMap.ToResult(hostId, host =>
                Results.Json(host.GetEntityIdsResponse())));

        return app;
    }
}