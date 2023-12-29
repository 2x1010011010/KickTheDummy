using CharacterScripts;
using Tools.ToolsSystem;
using Tools.Weapon;
using UnityEngine;

namespace Infrastructure
{
  public class SceneHandler : MonoBehaviour
  {
    [SerializeField] private ToolsPanel _toolsPanel;
    [SerializeField] private WeaponSwitcher _weaponSwitcher;
    [SerializeField] private GameObject _swipeDetectionPanel;
    [SerializeField] private CharacterSpawner _spawner;
    private bool CanInterract = true;
    
    private void Start()
    {
      _weaponSwitcher.Initialize(_toolsPanel);
    }

    private void LateUpdate()
    {
      if (!CanInterract) return;
      
      if (Input.GetMouseButtonDown(0))
        _weaponSwitcher.CurrentWeapon.Action();

      if (!_toolsPanel.CurrentTool.Equals("Rifle"))
        return;
      
      if (Input.GetMouseButton(0))
        _weaponSwitcher.CurrentWeapon.Action();
    }

    private void OnEnable()
    {
      _toolsPanel.OnToolChanged += ChangeTool;
      _toolsPanel.OnToolPanelEnter += BlockGameInput;
      _toolsPanel.OnToolPanelExit += UnBlockGameInput;
      _spawner.OnCharacterSpawned += DisableCameraMovement;
      _spawner.OnCharacterDroped += EnableCameraMovement;
    }

    private void OnDisable()
    {
      _toolsPanel.OnToolChanged -= ChangeTool;
      _toolsPanel.OnToolPanelExit -= UnBlockGameInput;
      _toolsPanel.OnToolPanelEnter -= BlockGameInput;
      _spawner.OnCharacterDroped -= EnableCameraMovement;
      _spawner.OnCharacterSpawned -= DisableCameraMovement;
    }

    private void DisableCameraMovement()
    {
      _swipeDetectionPanel.SetActive(false);
    }

    private void EnableCameraMovement()
    {
      _swipeDetectionPanel.SetActive(true);
    }

    private void UnBlockGameInput()
    {
      CanInterract = true;
    }
    
    private void BlockGameInput()
    {
      CanInterract = false;
    }
    private void ChangeTool()
    {
      _weaponSwitcher.SwitchToNextTool();
    }
  }
}
