using RootMotion.Dynamics;
using UnityEngine;
using UnityEngine.Events;

namespace CharacterScripts
{
  public class CharacterSpawner : MonoBehaviour
  {
    [SerializeField] private CharacterSpawnerSettings _settings;
    [SerializeField] private Transform _spawnerTransform;
    private GameObject _spawned;
    private Camera _camera;
    private bool _isSpawned = false;

    public event UnityAction OnCharacterSpawned;
    public event UnityAction OnCharacterDroped;

    private void Awake()
    {
      _camera = Camera.main;
    }

    private void Update()
    {
      if (!_spawned) return;

      if (Input.GetMouseButton(0))
      {
        var mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        _spawned.transform.position = new Vector3(mousePosition.x, 0f, mousePosition.y);
      }

      if (Input.GetMouseButtonUp(0))
      {
        _isSpawned = false;
        OnCharacterDroped?.Invoke();
      }
    }

    public void Spawn()
    {
      var mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
      var cameraPosition = _camera.transform.position;
      _spawned = Instantiate(_settings.Prefab, _spawnerTransform);
      _isSpawned = true;
      
      _spawned.GetComponentInChildren<PuppetMaster>().Teleport(cameraPosition,Quaternion.LookRotation(-Vector3.forward), false);
      
      OnCharacterSpawned?.Invoke();
    }
  }
}
