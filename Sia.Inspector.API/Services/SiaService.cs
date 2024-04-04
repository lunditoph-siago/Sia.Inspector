namespace Sia.Inspector.API;

public class SiaService(World world, Scheduler mainScheduler, Thread thread)
{
    public World World { get; } = world;
    public Scheduler MainScheduler { get; } = mainScheduler;
    public Thread Thread { get; } = thread;
    public object Lock { get; } = new();
}