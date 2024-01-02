using UnityEngine;
using UnityEngine.Events;

namespace CharacterScripts
{
  public class CharacterSpawner : MonoBehaviour
  {
    [SerializeField] private CharacterSpawnerSettings _settings;
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
      _spawned = Instantiate(_settings.Prefab, transform.position, Quaternion.LookRotation(-Vector3.forward));
      _isSpawned = true;
      OnCharacterSpawned?.Invoke();
    }
  }
}
