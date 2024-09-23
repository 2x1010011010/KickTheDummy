using System;

public enum DismemberType
{
    Cut,
    Tear
}

public interface IDismemberable
{
    event Action<IDismemberable, DismemberType> Dismembered;

    void Dismember(DismemberType dismemberType);
}
