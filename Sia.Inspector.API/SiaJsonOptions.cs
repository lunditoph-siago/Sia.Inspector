namespace Sia.Inspector.API;

using System.Text.Encodings.Web;
using System.Text.Json;

public static class SiaJsonOptions
{
    public readonly static JsonSerializerOptions Default = new(JsonSerializerDefaults.Web) {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public readonly static JsonSerializerOptions Addon = new(JsonSerializerDefaults.Web) {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        IncludeFields = true,
        IgnoreReadOnlyProperties = true
    };

    public readonly static JsonSerializerOptions Component = new(JsonSerializerDefaults.Web) {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        IncludeFields = true,
        IgnoreReadOnlyProperties = true
    };

    public readonly static JsonSerializerOptions Event = new(JsonSerializerDefaults.Web) {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        IncludeFields = true,
        IgnoreReadOnlyProperties = false
    };
}