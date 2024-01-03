using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
  public class ReloadButton : MonoBehaviour
  {
    public void Reload()
    {
      var currentSceneName = SceneManager.GetActiveScene().name;
      SceneManager.LoadScene(currentSceneName, LoadSceneMode.Single);
    }
  }
}
