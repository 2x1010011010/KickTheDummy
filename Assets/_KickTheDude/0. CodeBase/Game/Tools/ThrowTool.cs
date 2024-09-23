using Game.InteractiveSystem;
using Game.ResourceSystem;
using UnityEngine;
using Zenject;

public class ThrowTool : MonoBehaviour, ITool
{
    public string Name => "ThrowTool";

    [SerializeField] private PropEntity _resourceForSpawn;
    [SerializeField] private AudioSource _toolSource;
    [SerializeField] private AudioClip _throwClip;
    [SerializeField] private float _throwForce = 100;
    [SerializeField] private float _throwUpForce = 100;
    [SerializeField, Min(0)] private Vector2 _torque = new Vector2(5, 10);

    private IEntitiesFactory<InteractableObject> _entitiesFactory;
    private IUIService _uiService;

    public IInteractable Interactable { get; private set; }

    [Inject]
    private void Construct(IEntitiesFactory<InteractableObject> entitiesFactory, IUIService uiService, IPlayer player)
    { 
        _entitiesFactory = entitiesFactory;
        _uiService = uiService;
    }

    public void Init(IInteractable initData)
    {
        Interactable = initData;
    }

    public void StopInteract()
    {
        StopUse();
    }

    public async void StartUse(Vector2 screenPosition, Vector2 direction)
    {
        if (_uiService.IsPointerOverUI()) return;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        var obj = await _entitiesFactory.CreateEntity(_resourceForSpawn.InteractableObjectReference, ray.origin, Quaternion.identity);

        obj.RootRigidbody.AddForce(ray.direction * _throwForce, ForceMode.Impulse);
        obj.RootRigidbody.AddForce(Camera.main.transform.up * _throwUpForce, ForceMode.Impulse);
        obj.RootRigidbody.AddRelativeTorque(new Vector3(
            Random.Range(-1, 1) < 0 ? -1 : 1, 
            /*Random.Range(-1, 1) < 0 ? -1 : 1*/ 0, 
            Random.Range(-1, 1) < 0 ? -1 : 1) 
            * Random.Range(_torque.x, _torque.y), ForceMode.Impulse);

        _toolSource.PlayOneShot(_throwClip);
    }

    public void StopUse()
    {
        
    }

    public void Dispose()
    {
        
    }
}
