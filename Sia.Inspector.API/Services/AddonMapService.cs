namespace Sia.WebInspector.API.Services;

using System.Diagnostics.CodeAnalysis;

public class AddonMapService : IDisposable
{
    public World World { get; }

    public long this[IAddon addon] => _reverseMap[addon];

    private readonly Dictionary<long, IAddon> _map = [];
    private readonly Dictionary<IAddon, long> _reverseMap = [];

    private bool _disposed;
    private long _acc = -1;

    public AddonMapService(World world)
    {
        World = world;
        world.OnAddonCreated += OnAddonCreated;
        world.OnAddonRemoved += OnAddonRemoved;

        foreach (var addon in world.Addons) {
            OnAddonCreated(addon);
        }
    }

    public bool TryGetHost(long id, [MaybeNullWhen(false)] out IAddon addon)
        => _map.TryGetValue(id, out addon);

    public bool TryGetHostId(IAddon addon, out long id)
        => _reverseMap.TryGetValue(addon, out id);
    
    public IResult ToResult(long id, Func<IAddon, IResult> mapper)
        => _map.TryGetValue(id, out var addon)
            ? mapper(addon) : Results.NotFound("invalid addon id");

    private void OnAddonCreated(IAddon addon)
    {
        var id = Interlocked.Increment(ref _acc);
        _map[id] = addon;
        _reverseMap[addon] = id;
    }

    private void OnAddonRemoved(IAddon addon)
    {
        if (_reverseMap.Remove(addon, out var id)) {
            _map.Remove(id);
        }
    }

    public void Dispose()
    {
        if (_disposed) { return; }
        _disposed = true;
        GC.SuppressFinalize(this);

        World.OnAddonCreated -= OnAddonCreated;
        World.OnAddonRemoved -= OnAddonRemoved;
    }
}