using Game.ExplosionSystem;
using UnityEngine;

public class ChangeObjectsState : ExplodeReactor
{
    public override string Name => "CHANGE OBJECTS STATE";

    [SerializeField] private GameObject[] _objects;
    [SerializeField] private bool _state;

    public override void ReactOnExplode(ExplosionData reactData)
    {
        foreach (var obj in _objects)
            obj.SetActive(_state);
    }
}
