namespace Sia.Inspector.API.Endpoints;

using Sia.Inspector.API.Reponses;
using Sia.Inspector.API.Services;

public static class HostEndpoints
{
    public static WebApplication RegisterHostEndpoints(this WebApplication app, string prefix)
    {
        var group = app.MapGroup(prefix + "hosts");

        group.MapGet("/", (SiaService sia, HostMapService hostMap) => {
            lock (sia.Lock) {
                return hostMap.World.Hosts
                    .Select(host => new {
                        Id = hostMap[host],
                        host.Capacity,
                        host.Count,
                        Desc = host.Descriptor.GetComponentsResponse()
                    }).ToJsonResult();
            }
        });

        group.MapGet("/ids", (SiaService sia, HostMapService hostMap) => {
            lock (sia.Lock) {
                return hostMap.World.Hosts
                    .Select(host => hostMap[host])
                    .ToJsonResult();
            }
        });

        group.MapGet("/{hostId:long}", (long hostId, SiaService sia, HostMapService hostMap) => {
            lock (sia.Lock) {
                return hostMap.ToResult(hostId, host =>
                    Results.Json(new {
                        host.Capacity,
                        host.Count,
                        Desc = host.Descriptor.GetComponentsResponse()
                    }));
            }
        });

        group.MapGet("/{hostId:long}/entities", (long hostId, SiaService sia, HostMapService hostMap) => {
            lock (sia.Lock) {
                return hostMap.ToResult(hostId, host =>
                    host.GetEntityIdsResponse().ToJsonResult());
            }
        });

        return app;
    }
}