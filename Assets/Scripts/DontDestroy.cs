using UnityEngine;

namespace DefaultNamespace
{
  public class DontDestroy : MonoBehaviour
  {
    private void Awake()
    {
      DontDestroyOnLoad(gameObject);
    }
  }
}