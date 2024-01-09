using CameraSystem;
using CharacterScripts;
using Tools.ToolsSystem;
using Tools.Weapon;
using Tools.Weapon.Throwable;
using UnityEngine;

namespace Infrastructure
{
  public class SceneHandler : MonoBehaviour
  {
    [SerializeField] private ToolsPanel _toolsPanel;
    [SerializeField] private WeaponSwitcher _weaponSwitcher;
    [SerializeField] private CharacterSpawner _spawner;
    [SerializeField] private CameraMover _mover;
    private bool CanInterract = true;

    private void Start()
    {
      _weaponSwitcher.Initialize(_toolsPanel);
    }

    private void LateUpdate()
    {
      if (!CanInterract) return;
      if (Input.touchCount <= 0) return;
      
      if(Input.GetMouseButtonDown(0))
        _weaponSwitcher.CurrentWeapon.Action();
    }

    private void OnEnable()
    {
      _toolsPanel.OnToolChanged += ChangeTool;
      _toolsPanel.OnToolPanelEnter += BlockGameInput;
      _toolsPanel.OnToolPanelExit += UnBlockGameInput;
    }

    private void OnDisable()
    {
      _toolsPanel.OnToolChanged -= ChangeTool;
      _toolsPanel.OnToolPanelExit -= UnBlockGameInput;
      _toolsPanel.OnToolPanelEnter -= BlockGameInput;
    }
    

    private void UnBlockGameInput() =>
      CanInterract = true;
    
    private void BlockGameInput() =>
      CanInterract = false;

    private void ChangeTool() =>
      _weaponSwitcher.SwitchToNextTool();
  }
}
