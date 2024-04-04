namespace Sia.Inspector.API.Reponses;

public static class EntityHostExtensions
{
    public static IEnumerable<long> GetEntityIdsResponse(this IEntityHost host)
        => host.Select(entity => entity.Id.Value);
}