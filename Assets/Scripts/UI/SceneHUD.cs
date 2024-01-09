using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
  public class SceneHUD : MonoBehaviour
  {
    [SerializeField] private List<GameObject> _panels;
    [SerializeField] private GameObject _settings;

    public event UnityAction OnSettingsOpen;
    public event UnityAction OnSettingsClosed;

    public void SwitchPanels(bool isSettingsActive)
    {
      foreach(var item in _panels)
        item.SetActive(!isSettingsActive);
      _settings.SetActive(isSettingsActive);

      if (isSettingsActive)
        OnSettingsOpen?.Invoke();
      else 
        OnSettingsClosed?.Invoke();
    }
  }
}