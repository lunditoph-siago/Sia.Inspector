namespace Sia.WebInspector.API;

using Sia.WebInspector.API.Endpoints;

public static class SiaEndpointsExtensions
{
    public static WebApplication UseSiaEndpoints(this WebApplication app, string prefix)
    {
        return app
            .RegisterWorldEndpoints(prefix)
            .RegisterAddonEndpoints(prefix)
            .RegisterHostEndpoints(prefix)
            .RegisterEntityEndpoints(prefix);
    }
}