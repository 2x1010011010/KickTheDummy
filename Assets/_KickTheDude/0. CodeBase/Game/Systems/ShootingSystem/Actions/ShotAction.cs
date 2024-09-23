public abstract class ShotAction : IInteractorAction<IShootable>
{
    protected IShootable _shootable;

    public abstract string Name { get; }

    public virtual void Init(IShootable reactSource)
    {
        _shootable = reactSource;

        _shootable.Shoted += ReactOnShot;
    }

    public void Dispose()
    {
        _shootable.Shoted -= ReactOnShot;
    }

    public abstract void ReactOnShot(ShotData reactData);
}