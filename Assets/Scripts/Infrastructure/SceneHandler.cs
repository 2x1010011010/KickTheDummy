using System;
using Tools.ToolsSystem;
using Tools.Weapon;
using UnityEngine;

namespace Infrastructure
{
  public class SceneHandler : MonoBehaviour
  {
    [SerializeField] private ToolsPanel _toolsPanel;
    [SerializeField] private WeaponSwitcher _weaponSwitcher;
    
    private void Start()
    {
      _weaponSwitcher.Initialize(_toolsPanel);
    }

    private void OnEnable()
    {
      _toolsPanel.OnToolChanged += ChangeTool;
    }
    
    private void OnDisable()
    {
      _toolsPanel.OnToolChanged -= ChangeTool;
    }
    
    private void ChangeTool()
    {
      _weaponSwitcher.SwitchToNextTool();
    }
  }
}
