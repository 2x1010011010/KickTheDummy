using System.Collections.Generic;
using System.Linq;
using Tools.ToolsSystem;
using UnityEngine;

namespace Tools.Weapon
{
  public class WeaponSwitcher : MonoBehaviour
  {
    [SerializeField] private List<GameObject> _tools;
    private ToolsPanel _toolsPanel;
    private GameObject _currentTool = null;
  
    public void Initialize(ToolsPanel panel)
    {
      _toolsPanel = panel;
      SwitchToNextTool();
    }

    public void SwitchToNextTool()
    {
      foreach (var tool in _tools.Where(tool => tool.name == _toolsPanel.CurrentTool))
      {
        _currentTool?.SetActive(false);
        _currentTool = tool;
        _currentTool.SetActive(true);
        break;
      }
    }
  }
}
