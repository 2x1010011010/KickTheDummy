using System.Collections;
using Lean.Pool;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace _KickTheDude._0._CodeBase.Game.Tools
{
  public class BowTool : MonoBehaviour, ITool
  {
    [SerializeField, BoxGroup("SETUP")] private LeanGameObjectPool _bloodEffectsPool;
    
    [SerializeField, BoxGroup("ARROW SETTINGS")] private GameObject _arrowPrefab;
    [SerializeField, BoxGroup("ARROW SETTINGS")] private float _firerate; 
    
    [SerializeField, BoxGroup("ARROW PHYSICS")] private float _force;
    [SerializeField, BoxGroup("ARROW PHYSICS")] private float _mass;
    
    [SerializeField, BoxGroup("BLOOD PAINT PARAMETERS")] private Color _color = Color.red;
    [SerializeField, BoxGroup("BLOOD PAINT PARAMETERS")] private int _amount = 1;
    [SerializeField, BoxGroup("BLOOD PAINT PARAMETERS")] private float _size = 0.1f;
    [SerializeField, BoxGroup("BLOOD PAINT PARAMETERS")] private Texture _bloodTexture;
    [SerializeField, BoxGroup("BLOOD PAINT PARAMETERS")] private Texture _bloodNormalTexture;
    [SerializeField, BoxGroup("BLOOD PAINT PARAMETERS")] private float _textureSize = 0.1f;
    
    [SerializeField, BoxGroup("AUDIO PARAMETERS")] private AudioSource _audioSource;
    [SerializeField, BoxGroup("AUDIO PARAMETERS")] private AudioClip[] _clips;
    [SerializeField, BoxGroup("AUDIO PARAMETERS")] private AudioClip[] _shotEndClip;
    [SerializeField, BoxGroup("AUDIO PARAMETERS")] private Vector2 _pitchRandom = Vector2.one;
    
    private RaycastHit hit = new RaycastHit();
    private Coroutine _shooting; 
    private IUIService _uiService;
    
    [Inject]
    private void Construct(IUIService uiService)
    {
      _uiService = uiService;
    }
      
    public void StartUse(Vector2 screenPosition, Vector2 direction)
    {
      if (_uiService.IsPointerOverUI()) return;

      if (_shooting != null)
        StopUse();

      _shooting = StartCoroutine(Shooting());
    }

    public void StopUse()
    {
      if (_shooting != null)
      {
        StopCoroutine(_shooting);
      }
    }
    
    private IEnumerator Shooting()
    {
      while (true)
      {
        Shot();
        yield return new WaitForSeconds(60 / _firerate);
      }
    }

    private void Shot()
    {
      
    }
  }
}