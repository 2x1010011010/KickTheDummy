using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPirceable
{
    Transform PierceParent { get; }
    IReadOnlyList<IPiercer> AttachedPiercers { get; }

    void AddPiercer(IPiercer piercer);
    void UnpierceAll();
}
