using System;

public enum DetachType
{
    Tear,
    Cut,
    Smash
}

public interface IDetachable
{
    event Action<IDetachable, DetachType> DetachSignal;

    bool Detached { get; }

    void Detach(DetachType detachType);
    void ReduceAttach(int reduceValue, DetachType detachType);
}
