namespace Sia.Inspector.API.Endpoints;

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using Sia.Inspector.API.Reponses;
using Sia.Inspector.API.Services;

public static class EntityEndpoints
{
    private record FullEventMessage(object Event, long Size);

    private class FullEventListener(ChannelWriter<FullEventMessage> writer) : IEventListener<EntityRef>
    {
        public bool OnEvent<TEvent>(in EntityRef entity, in TEvent e)
            where TEvent : IEvent
        {
            writer.TryWrite(new(e, Unsafe.SizeOf<TEvent>()));
            if (typeof(TEvent) == typeof(WorldEvents.Remove)) {
                writer.Complete();
            }
            return false;
        }
    }

    private class EventListener(ChannelWriter<string> writer) : IEventListener<EntityRef>
    {
        public bool OnEvent<TEvent>(in EntityRef entity, in TEvent e)
            where TEvent : IEvent
        {
            writer.TryWrite(e.GetType().ToString());
            if (typeof(TEvent) == typeof(WorldEvents.Remove)) {
                writer.Complete();
            }
            return false;
        }
    }

    public static WebApplication RegisterEntityEndpoints(this WebApplication app, string prefix)
    {
        var group = app.MapGroup(prefix + "entities");

        group.MapGet("/", (SiaService sia, HttpContext context) => {
            lock (sia.Lock) {
                return Results.Json(
                    sia.World.GetEntityIdsResponse(context));
            }
        });

        group.MapGet("/{entityId:long}", (
            long entityId, SiaService sia, EntityMapService entityMap, HostMapService hostMap, HttpContext context) => {
            lock (sia.Lock) {
                return entityMap.ToResult(entityId, entity =>
                    Results.Json(new {
                        host = hostMap[entity.Host],
                        comps = entity.GetComponentsResponse(context)
                    }, SiaJsonOptions.Component));
            }
        });

        group.MapGet("/{entityId:long}/host", (
            long entityId, SiaService sia, EntityMapService entityMap, HostMapService hostMap) => {
            lock (sia.Lock) {
                return entityMap.ToResult(entityId, entity =>
                    Results.Json(
                        hostMap[entity.Host],
                        SiaJsonOptions.Component));
            }
        });

        group.MapGet("/{entityId:long}/comps", (
            long entityId, SiaService sia, EntityMapService entityMap, HttpContext context) => {
            lock (sia.Lock) {
                return entityMap.ToResult(entityId, entity =>
                    Results.Json(
                        entity.GetComponentsResponse(context),
                        SiaJsonOptions.Component));
            }
        });

        group.MapGet("/{entityId:long}/events", async (
            long entityId, SiaService sia, EntityMapService entityMap, HostMapService hostMap,
            HttpContext context, CancellationToken token,
            [FromQuery] bool full = true, [FromQuery] bool unnamed = false) => {
            EntityRef entity;

            lock (sia.Lock) {
                if (!entityMap.TryGet(entityId, out entity)) {
                    return Results.NotFound("invalid entity Id");
                }
            }

            var response = context.Response;
            var headers = response.Headers;

            headers.Append("Content-Type", "text/event-stream");
            headers.Append("Cache-Control", "no-cache");

            IEventListener<EntityRef> listener;

            if (full) {
                var channel = Channel.CreateUnbounded<FullEventMessage>(
                    new UnboundedChannelOptions { SingleWriter = true });
                listener = new FullEventListener(channel.Writer);
                lock (sia.Lock) {
                    sia.World.Dispatcher.Listen(entity, listener);
                }
                if (unnamed) {
                    await foreach (var msg in channel.Reader.ReadAllAsync(token)) {
                        var type = msg.Event.GetType().ToString();
                        object data = msg.Size != 0
                            ? new { Type = type, Value = msg.Event }
                            : new { Type = type };

                        await response.WriteAsync("data: ", token);
                        await response.WriteAsync(JsonSerializer.Serialize(data, SiaJsonOptions.Event), token);
                        await response.WriteAsync("\n\n", token);
                        await response.Body.FlushAsync(token);
                    }
                }
                else {
                    await foreach (var msg in channel.Reader.ReadAllAsync(token)) {
                        await response.WriteAsync("event: ", token);
                        await response.WriteAsync(msg.Event.GetType().ToString(), token);
                        if (msg.Size != 0) {
                            await response.WriteAsync("\ndata: ", token);
                            await response.WriteAsync(JsonSerializer.Serialize(msg.Event, SiaJsonOptions.Event), token);
                        }
                        await response.WriteAsync("\n\n", token);
                        await response.Body.FlushAsync(token);
                    }
                }
            }
            else {
                var channel = Channel.CreateUnbounded<string>(
                    new UnboundedChannelOptions { SingleWriter = true });
                listener = new EventListener(channel.Writer);
                lock (sia.Lock) {
                    sia.World.Dispatcher.Listen(entity, listener);
                }
                if (unnamed) {
                    await foreach (var type in channel.Reader.ReadAllAsync(token)) {
                        await response.WriteAsync("data: ", token);
                        await response.WriteAsync(JsonSerializer.Serialize(type, SiaJsonOptions.Event), token);
                        await response.WriteAsync("\n\n", token);
                        await response.Body.FlushAsync(token);
                    }
                }
                else {
                    await foreach (var type in channel.Reader.ReadAllAsync(token)) {
                        await response.WriteAsync("event: ", token);
                        await response.WriteAsync(type, token);
                        await response.WriteAsync("\n\n", token);
                        await response.Body.FlushAsync(token);
                    }
                }
            }

            lock (sia.Lock) {
                sia.World.Dispatcher.Unlisten(entity, listener);
            }

            return Results.Ok();
        });

        return app;
    }
}