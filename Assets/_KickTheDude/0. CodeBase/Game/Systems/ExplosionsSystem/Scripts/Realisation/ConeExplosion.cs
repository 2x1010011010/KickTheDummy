public class ConeExplosion : Explodeable
{
    public override string Name => "CONE EXPLOSION";

    protected override ExplosionContact[] GetExplosionContactsWhenExplode()
    {
        return null;
    }
}

