using Game.InteractiveSystem;
using Game.ResourceSystem;
using UnityEngine;
using Zenject;

public class SpawnTool : MonoBehaviour, ITool
{
    public string Name => "SpawnTool";

    [SerializeField] private PropEntity _resourceForSpawn;
    [SerializeField] private LayerMask _spawnMask;
    [SerializeField] private AudioSource _toolSource;
    [SerializeField] private AudioClip _spawnClip;

    private IEntitiesFactory<InteractableObject> _entitiesFactory;
    private IUIService _uiService;
    private IEffectsFactory _effectsFactory;

    private RaycastHit _hit;

    public IInteractable Interactable { get; private set; }

    public void Init(IInteractable initData)
    {
        Interactable = initData;
    }

    public void StopInteract()
    {
        StopUse();
    }

    [Inject]
    private void Construct(IEntitiesFactory<InteractableObject> entitiesFactory, IUIService uiService, IEffectsFactory effectsFactory)
    {
        _entitiesFactory = entitiesFactory;
        _uiService = uiService;
        _effectsFactory = effectsFactory;
    }

    public async void StartUse(Vector2 screenPosition, Vector2 direction)
    {
        if (_uiService.IsPointerOverUI()) return;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out _hit, 10f, _spawnMask, QueryTriggerInteraction.Ignore))
        {
            var obj = await _entitiesFactory.CreateEntity(_resourceForSpawn.InteractableObjectReference, _hit.point, Quaternion.identity);
            
            if (obj.TryGetComponent(out TurretShooter turretShooter))
                turretShooter.Init(_effectsFactory);
        }

        _toolSource.PlayOneShot(_spawnClip);
    }

    public void StopUse() { }

    public void Dispose()
    {
        
    }
}
