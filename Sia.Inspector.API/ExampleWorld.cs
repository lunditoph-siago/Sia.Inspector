namespace Sia.Inspector.API.Examples;

using Sia.Reactors;
using System.Numerics;

public partial record struct Position([Sia] Vector3 Value);
public partial record struct Rotation([Sia] Quaternion Value);
public partial record struct Scale([Sia] Vector3 Value);

[SiaBundle]
public partial record struct Transform(Position Position, Rotation Rotation, Scale Scale);

public readonly record struct ObjectId(int Value)
{
    public static implicit operator ObjectId(int id)
        => new(id);
}

public record struct Name([Sia] string Value)
{
    public static implicit operator Name(string name)
        => new(name);
}

[SiaBundle]
public partial record struct GameObject(Sid<ObjectId> Id, Name Name);

public partial record struct HP([Sia] int Value);

public class HealthUpdateSystem()
    : SystemBase(
        matcher: Matchers.Of<HP>())
{
    public override void Execute(World world, Scheduler scheduler, IEntityQuery query)
    {
        foreach (var entity in query) {
            var hp = new HP.View(entity);
            hp.Value--;
        }
    }
}

[AfterSystem<HealthUpdateSystem>]
public class DeathSystem()
    : SystemBase(
        matcher: Matchers.Of<HP>())
{
    public override void Execute(World world, Scheduler scheduler, IEntityQuery query)
    {
        foreach (var entity in query) {
            if (entity.Get<HP>().Value <= 0) {
                entity.Dispose();
                Console.WriteLine("Dead!");
            }
        }
    }
}

public static class TestObject
{
    public static EntityRef Create(World world)
    {
        var transform = new Transform {
            Position = new Position {
                Value = Vector3.Zero
            },
            Rotation = new Rotation {
                Value = Quaternion.Identity
            },
            Scale = new Scale {
                Value = Vector3.One
            }
        };
        var gameObject = new GameObject {
            Id = new Sid<ObjectId>(0),
            Name = "Test"
        };
        return world.CreateInArrayHost(HList.Create(new HP(100)))
            .AddBundle(transform)
            .AddBundle(gameObject);
    }

    public static EntityRef CreateWithDynBundle(World world)
    {
        var transform = new Transform {
            Position = new Position {
                Value = Vector3.Zero
            },
            Rotation = new Rotation {
                Value = Quaternion.Identity
            },
            Scale = new Scale {
                Value = Vector3.One
            }
        };
        var gameObject = new GameObject {
            Id = new Sid<ObjectId>(0),
            Name = "Test"
        };

        return world.CreateInArrayHost()
            .AddBundle(
                new DynBundle()
                    .Add(new HP(100))
                    .AddBundle(transform)
                    .AddBundle(gameObject)
                    .Remove<Scale>());
    }
}

public static partial class ExampleWorld
{
    public static World Create(out Scheduler scheduler)
    {
        var world = new World();
        Context<World>.Current = world;
        scheduler = new Scheduler();

        SystemChain.Empty
            .Add<HealthUpdateSystem>()
            .Add<DeathSystem>()
            .RegisterTo(world, scheduler);

        TestObject.Create(world);
        TestObject.CreateWithDynBundle(world);

        return world;
    }
}