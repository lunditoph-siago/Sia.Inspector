using Sia;
using Sia.Inspector.API;
using Sia.Inspector.API.Examples;


var world = ExampleWorld.Create(out var scheduler);
Context<World>.Current = world;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(
    options => options.AddPolicy(
        "cors",
        policyBuilder => {
            policyBuilder.SetIsOriginAllowed(_ => true);
            policyBuilder.AllowAnyHeader();
            policyBuilder.AllowAnyMethod();
            policyBuilder.AllowCredentials();
        }));

builder.Services.AddSiaServices(world, scheduler, out _);

var app = builder.Build();
app.UseCors("cors");
app.UseSiaEndpoints("/");

app.MapGet("/test/entity", (SiaService sia) => {
    lock (sia.Lock) {
        var testEntity = sia.World.CreateInArrayHost();
        testEntity.Dispose();
    }
});

app.MapGet("/test/tick", (SiaService sia) => {
    lock (sia.Lock) {
        Context<World>.With(sia.World, () => scheduler.Tick());
    }
});

app.Run();