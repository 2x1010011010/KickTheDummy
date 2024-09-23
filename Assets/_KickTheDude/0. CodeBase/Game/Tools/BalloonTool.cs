using Game.InteractiveSystem;
using Game.ResourceSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class BalloonTool : SerializedMonoBehaviour, ITool
{
    public string Name => "BalloonTool";

    [SerializeField, FoldoutGroup("SETUP")] private LayerMask _rayLayerMask;
    [SerializeField, FoldoutGroup("SETUP")] private PropEntity _balloon;

    private IEntitiesFactory<InteractableObject> _entitiesFactory;

    public IInteractable Interactable { get; private set; }

    [Inject]
    private void Construct(IEntitiesFactory<InteractableObject> entitiesFactory)
    {
        _entitiesFactory = entitiesFactory;
    }

    public void Init(IInteractable initData)
    {
        Interactable = initData;
    }

    public void StopInteract()
    {
        StopUse();
    }

    public async void AttachBalloon(Ray ray)
    {
        RaycastHit catchedPhysicalObject;
        if (Physics.Raycast(ray.origin, ray.direction, out catchedPhysicalObject, 10f, _rayLayerMask, QueryTriggerInteraction.Ignore))
        {
            if (catchedPhysicalObject.collider.attachedRigidbody != null)
            {
                var balloonObject = await _entitiesFactory.CreateEntity(_balloon.InteractableObjectReference, catchedPhysicalObject.point, Quaternion.identity);

                if (balloonObject.TryGetInteractive(out Balloon balloon))
                    balloon.AttachTo(catchedPhysicalObject.collider.attachedRigidbody);
            }
        }
    }

    public void StartUse(Vector2 screenPosition, Vector2 direction)
    {
        AttachBalloon(Camera.main.ScreenPointToRay(Input.mousePosition));
    }

    public void StopUse()
    {
        
    }

    public void Dispose()
    {
        
    }
}
