namespace Sia.Inspector.API.Reponses;

using CommunityToolkit.HighPerformance.Buffers;

public static class EntityQueryExtensions
{
    public static IEnumerable<long> GetEntityIdsResponse(this IEntityQuery query, HttpContext context)
    {
        var owner = MemoryOwner<long>.Allocate(query.Count);
        context.Response.RegisterForDispose(owner);

        var span = owner.Span;
        int index = 0;

        foreach (var entity in query) {
            span[index] = entity.Id.Value;
            ++index;
        }

        return owner.DangerousGetArray();
    }
}