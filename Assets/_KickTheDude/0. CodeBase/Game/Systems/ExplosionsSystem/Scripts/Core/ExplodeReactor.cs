
public abstract class ExplodeReactor : IInteractorAction<IExplodeable>
{
    private IExplodeable _explodeable;

    public abstract string Name { get; }

    public virtual void Init(IExplodeable explodeable)
    {
        _explodeable = explodeable;

        _explodeable.Exploded += ReactOnExplode;
    }

    public void Dispose()
    {
        _explodeable.Exploded -= ReactOnExplode;
    }

    public abstract void ReactOnExplode(ExplosionData reactData);
}

