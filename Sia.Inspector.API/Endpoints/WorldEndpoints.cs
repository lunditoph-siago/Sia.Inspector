using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;

namespace Sia.Inspector.API.Endpoints;

public static class WorldEndpoints
{
    private record FullEventMessage(long Entity, object Event, long Size);
    private record EventMessage(long Entity, string Type);

    private class FullEventListener(ChannelWriter<FullEventMessage> writer) : IEventListener<EntityRef>
    {
        public bool OnEvent<TEvent>(in EntityRef entity, in TEvent e)
            where TEvent : IEvent
        {
            writer.TryWrite(new(entity.Id.Value, e, Unsafe.SizeOf<TEvent>()));
            return false;
        }
    }

    private class EventListener(ChannelWriter<EventMessage> writer) : IEventListener<EntityRef>
    {
        public bool OnEvent<TEvent>(in EntityRef entity, in TEvent e)
            where TEvent : IEvent
        {
            writer.TryWrite(new(entity.Id.Value, e.GetType().ToString()));
            return false;
        }
    }

    public static WebApplication RegisterWorldEndpoints(this WebApplication app, string prefix)
    {
        var group = app.MapGroup(prefix);

        group.MapGet("/", (SiaService sia) => {
            lock (sia.Lock) {
                var world = sia.World;
                return new {
                    AddonCount = world.Addons.Count(),
                    EntityCount = world.Count,
                    HostCount = world.Hosts.Count
                };
            }
        });
        
        group.MapGet("/events", async (
            SiaService sia, HttpContext context, CancellationToken token,
            [FromQuery] bool full = true, [FromQuery] bool unnamed = false) => {
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
                    sia.World.Dispatcher.Listen(listener);
                }
                if (unnamed) {
                    await foreach (var msg in channel.Reader.ReadAllAsync(token)) {
                        var type = msg.Event.GetType().ToString();
                        object data = msg.Size != 0
                            ? new { msg.Entity, Type = type, Value = msg.Event }
                            : new { msg.Entity, Type = type };

                        await response.WriteAsync("data: ", token);
                        await response.WriteAsync(JsonSerializer.Serialize(data, SiaJsonOptions.Event), token);
                        await response.WriteAsync("\n\n", token);
                        await response.Body.FlushAsync(token);
                    }
                }
                else {
                    await foreach (var msg in channel.Reader.ReadAllAsync(token)) {
                        object data = msg.Size != 0
                            ? new { msg.Entity, Value = msg.Event }
                            : new { msg.Entity };

                        await response.WriteAsync("event: ", token);
                        await response.WriteAsync(msg.Event.GetType().ToString(), token);
                        await response.WriteAsync("\ndata: ", token);
                        await response.WriteAsync(JsonSerializer.Serialize(data, SiaJsonOptions.Event), token);
                        await response.WriteAsync("\n\n", token);
                        await response.Body.FlushAsync(token);
                    }
                }
            }
            else {
                var channel = Channel.CreateUnbounded<EventMessage>(
                    new UnboundedChannelOptions { SingleWriter = true });
                listener = new EventListener(channel.Writer);
                lock (sia.Lock) {
                    sia.World.Dispatcher.Listen(listener);
                }
                if (unnamed) {
                    await foreach (var msg in channel.Reader.ReadAllAsync(token)) {
                        await response.WriteAsync("data: ", token);
                        await response.WriteAsync(JsonSerializer.Serialize(msg, SiaJsonOptions.Event), token);
                        await response.WriteAsync("\n\n", token);
                        await response.Body.FlushAsync(token);
                    }
                }
                else {
                    await foreach (var msg in channel.Reader.ReadAllAsync(token)) {
                        await response.WriteAsync("event: ", token);
                        await response.WriteAsync(msg.Type, token);
                        await response.WriteAsync("\ndata: " + msg.Entity, token);
                        await response.WriteAsync("\n\n", token);
                        await response.Body.FlushAsync(token);
                    }
                }
            }

            lock (sia.Lock) {
                sia.World.Dispatcher.Unlisten(listener);
            }
        });

        return app;
    }
}