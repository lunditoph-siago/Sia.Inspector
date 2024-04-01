namespace Sia.WebInspector.API.Services;

using System.Diagnostics.CodeAnalysis;

public class HostMapService : IDisposable
{
    public World World { get; }

    public long this[IEntityHost host] => _reverseMap[host];

    private readonly Dictionary<long, IEntityHost> _map = [];
    private readonly Dictionary<IEntityHost, long> _reverseMap = [];

    private bool _disposed;
    private long _acc = -1;

    public HostMapService(World world)
    {
        World = world;
        world.OnEntityHostAdded += OnEntityHostAdded;
        world.OnEntityHostRemoved += OnEntityHostRemoved;

        foreach (var host in world.Hosts) {
            OnEntityHostAdded(host);
        }
    }

    public bool TryGetHost(long id, [MaybeNullWhen(false)] out IEntityHost host)
        => _map.TryGetValue(id, out host);

    public bool TryGetHostId(IEntityHost host, out long id)
        => _reverseMap.TryGetValue(host, out id);
    
    public IResult ToResult(long id, Func<IEntityHost, IResult> mapper)
        => _map.TryGetValue(id, out var host)
            ? mapper(host) : Results.NotFound("invalid host id");

    private void OnEntityHostAdded(IReactiveEntityHost host)
    {
        var id = Interlocked.Increment(ref _acc);
        _map[id] = host;
        _reverseMap[host] = id;
    }

    private void OnEntityHostRemoved(IReactiveEntityHost host)
    {
        if (_reverseMap.Remove(host, out var id)) {
            _map.Remove(id);
        }
    }

    public void Dispose()
    {
        if (_disposed) { return; }
        _disposed = true;
        GC.SuppressFinalize(this);

        World.OnEntityHostAdded -= OnEntityHostAdded;
        World.OnEntityHostRemoved -= OnEntityHostRemoved;
    }
}