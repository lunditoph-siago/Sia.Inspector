namespace Sia.WebInspector.API.Reponses;

using CommunityToolkit.HighPerformance.Buffers;

public static class EntityRefExtensions
{
    public struct ComponentsRecorder(ArraySegment<object?> components) : IRefGenericHandler<IHList>
    {
        public struct HeadComponentsRecorder(int index, ArraySegment<object?> components) : IGenericHandler
        {
            public readonly void Handle<T>(in T value)
                => components[index] = value;
        }

        public struct TailComponentsRecorder(int index, ArraySegment<object?> components) : IGenericHandler<IHList>
        {
            public readonly void Handle<T>(in T value) where T : IHList
            {
                value.HandleHead(new HeadComponentsRecorder(index, components));
                value.HandleTail(new TailComponentsRecorder(index + 1, components));
            }
        }

        public readonly void Handle<T>(ref T value)
            where T : IHList
        {
            value.HandleHead(new HeadComponentsRecorder(0, components));
            value.HandleTail(new TailComponentsRecorder(1, components));
        }
    }

    public static IEnumerable<object> GetComponentsResponse(this EntityRef entity, HttpContext context)
    {
        var owner = MemoryOwner<object?>.Allocate(entity.Descriptor.FieldOffsets.Count);
        context.Response.RegisterForDispose(owner);

        var array = owner.DangerousGetArray();
        entity.GetHList(new ComponentsRecorder(array));
        return array;
    }
}