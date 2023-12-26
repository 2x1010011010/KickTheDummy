using UnityEngine;

namespace UI
{
  public class SceneHUD : MonoBehaviour
  {
    [SerializeField] private GameObject _movePanel;
    [SerializeField] private GameObject _rotatePanel;
    
    //write public properties that return references to GameObject
    public GameObject MovePanel => _movePanel;
    public GameObject RotatePanel => _rotatePanel;
  }
}