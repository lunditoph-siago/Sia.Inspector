namespace Sia.WebInspector.API.Services;

using System.Runtime.CompilerServices;

public class EntityMapService : IDisposable
{
    public World World { get; }

    private readonly Dictionary<long, EntityRef> _map = [];
    private bool _disposed;

    public EntityMapService(World world)
    {
        World = world;
        world.OnEntityHostAdded += OnEntityHostAdded;
        world.OnEntityHostRemoved += OnEntityHostRemoved;

        foreach (var host in world.Hosts) {
            OnEntityHostAdded(host);
        }
        foreach (var entity in world) {
            OnEntityCreated(entity);
        }
    }

    public bool TryGet(long id, out EntityRef entity)
        => _map.TryGetValue(id, out entity);
    
    public IResult ToResult(long id, Func<EntityRef, IResult> mapper)
        => TryGet(id, out var entity)
            ? mapper(entity) : Results.NotFound("invalid entity id");

    private void OnEntityHostAdded(IReactiveEntityHost host)
    {
        host.OnEntityCreated += OnEntityCreated;
        host.OnEntityReleased += OnEntityReleased;
    }

    private void OnEntityHostRemoved(IReactiveEntityHost host)
    {
        host.OnEntityCreated -= OnEntityCreated;
        host.OnEntityReleased -= OnEntityReleased;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OnEntityCreated(in EntityRef entity)
    {
        _map[entity.Id.Value] = entity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OnEntityReleased(in EntityRef entity)
    {
        _map.Remove(entity.Id.Value);
    }

    public void Dispose()
    {
        if (_disposed) { return; }
        _disposed = true;
        GC.SuppressFinalize(this);

        foreach (var host in World.Hosts) {
            host.OnEntityCreated -= OnEntityCreated;
            host.OnEntityReleased -= OnEntityReleased;
        }

        World.OnEntityHostAdded -= OnEntityHostAdded;
        World.OnEntityHostRemoved -= OnEntityHostRemoved;
    }
}