using Game.InteractiveSystem;
using UnityEngine;

public class SimpleLook : IInteractive<IInteractable>, ILookable
{
    public string Name => "SIMPLE LOOK";

    [SerializeField] private Transform _root;

    public IInteractable Interactable { get; private set; }

    public void Init(IInteractable initData)
    {
        Interactable = initData;
    }

    public void Dispose()
    {
        
    }

    public void StopInteract()
    {

    }

    public void LookAt(Vector3 point)
    {
        _root.LookAt(point);
    }

    public void LookInput(Vector3 input)
    {
        
    }
}
