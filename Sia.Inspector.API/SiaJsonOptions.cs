namespace Sia.WebInspector.API;

using System.Text.Json;

public static class SiaJsonOptions
{
    public readonly static JsonSerializerOptions Addon = new(JsonSerializerDefaults.Web) {
        IncludeFields = true,
        IgnoreReadOnlyProperties = true
    };

    public readonly static JsonSerializerOptions Component = new(JsonSerializerDefaults.Web) {
        IncludeFields = true,
        IgnoreReadOnlyProperties = true
    };
}