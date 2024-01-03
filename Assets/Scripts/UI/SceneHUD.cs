using System.Collections.Generic;
using UnityEngine;

namespace UI
{
  public class SceneHUD : MonoBehaviour
  {
    [SerializeField] private List<GameObject> _panels;
    [SerializeField] private GameObject _settings;

    public void SwitchPanels(bool isSettingsActive)
    {
      foreach(var item in _panels)
        item.SetActive(!isSettingsActive);
      _settings.SetActive(isSettingsActive);
    }
  }
}