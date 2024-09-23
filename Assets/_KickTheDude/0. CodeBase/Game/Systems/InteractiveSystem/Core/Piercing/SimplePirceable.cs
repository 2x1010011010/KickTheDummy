using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePirceable : MonoBehaviour, IPirceable
{
    [SerializeField] private Transform _pierceParent;

    public Transform PierceParent => _pierceParent;

    private List<IPiercer> _attachedPiercers = new List<IPiercer>();
    IReadOnlyList<IPiercer> IPirceable.AttachedPiercers => _attachedPiercers;

    private void OnValidate()
    {
        if (_pierceParent == null) _pierceParent = transform;
    }

    public void AddPiercer(IPiercer piercer)
    {
        _attachedPiercers.Add(piercer);
    }

    public void UnpierceAll()
    {
        
    }
}
