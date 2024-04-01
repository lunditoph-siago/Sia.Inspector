using Sia;
using Sia.WebInspector.API;
using Sia.WebInspector.API.Examples;

var world = ExampleWorld.Create();
Context<World>.Current = world;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSiaServices(world);

var app = builder.Build();
app.UseSiaEndpoints("/");
app.Run();