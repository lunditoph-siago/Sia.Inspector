namespace Sia.WebInspector.API.Reponses;

public static class EntityDescriptorExtensions
{
    public static IEnumerable<string> GetComponentsResponse(this EntityDescriptor descriptor)
        => descriptor.FieldOffsets.Keys.Select(type => type.ToString());
}