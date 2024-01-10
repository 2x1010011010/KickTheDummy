using CameraSystem;
using CharacterScripts;
using Tools.ToolsSystem;
using Tools.Weapon;
using Tools.Weapon.Gun;
using Tools.Weapon.Melee;
using UI;
using UnityEngine;

namespace Infrastructure
{
  public class SceneHandler : MonoBehaviour
  {
    [SerializeField] private ToolsPanel _toolsPanel;
    [SerializeField] private WeaponSwitcher _weaponSwitcher;
    [SerializeField] private CharacterSpawner _spawner;
    [SerializeField] private CameraMover _mover;
    [SerializeField] private SceneHUD _settingsWindow; 
    
    private bool CanInterract = true;

    private void Start()
    {
      _weaponSwitcher.Initialize(_toolsPanel);
    }

    private void LateUpdate()
    {
      if (!CanInterract) return;
      if (Input.touchCount <= 0) return;
      if (Input.GetTouch(0).phase == TouchPhase.Moved) return;
      
      if (Input.GetMouseButtonDown(0))
        _weaponSwitcher.CurrentWeapon.Action();
      
      if (Input.GetMouseButton(0) && (_weaponSwitcher.CurrentWeapon.GetType() == typeof(Rifle) || _weaponSwitcher.CurrentWeapon.GetType() == typeof(Hand)))
        _weaponSwitcher.CurrentWeapon.Action();
    }

    private void OnEnable()
    {
      _toolsPanel.OnToolChanged += ChangeTool;
      _toolsPanel.OnToolPanelEnter += BlockGameInput;
      _toolsPanel.OnToolPanelExit += UnBlockGameInput;
      _spawner.OnCharacterSpawned += BlockGameInput;
      _spawner.OnCharacterDroped += UnBlockGameInput;
      _settingsWindow.OnSettingsOpen += BlockGameInput;
      _settingsWindow.OnSettingsClosed += UnBlockGameInput;
    }

    private void OnDisable()
    {
      _toolsPanel.OnToolChanged -= ChangeTool;
      _toolsPanel.OnToolPanelExit -= UnBlockGameInput;
      _toolsPanel.OnToolPanelEnter -= BlockGameInput;
      _spawner.OnCharacterSpawned -= BlockGameInput;
      _spawner.OnCharacterDroped -= UnBlockGameInput;
      _settingsWindow.OnSettingsOpen -= BlockGameInput;
      _settingsWindow.OnSettingsClosed -= UnBlockGameInput;
    }
    

    private void UnBlockGameInput() =>
      CanInterract = true;
    
    private void BlockGameInput() =>
      CanInterract = false;

    private void ChangeTool() =>
      _weaponSwitcher.SwitchToNextTool();
  }
}
