namespace Sia.Inspector.API.Reponses;

public static class EntityDescriptorExtensions
{
    public static IEnumerable<string> GetComponentsResponse(this EntityDescriptor descriptor)
        => descriptor.Offsets.Keys.Select(type => type.ToString());
}