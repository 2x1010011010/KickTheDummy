using System;

public interface IExplodeable
{
    event Action<ExplosionData> Exploded;

    void Explode();
}
