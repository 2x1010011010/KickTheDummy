using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tools.ToolsSystem
{
  public class ToolsPanel : MonoBehaviour
  {
    [SerializeField] private Color _active;
    [SerializeField] private Color _inactive;
    [SerializeField] private List<Tool> _tools;
    private Tool _currentTool;
    public Tool CurrentTool => _currentTool;

    public void SetToolActive()
    {
      SetCurrentTool();
    }

    private void SetCurrentTool(Tool chosenTool)
    {
      _currentTool = chosenTool;
      
      foreach(var tool in _tools)
        tool.SetColor(_inactive);
      
      _currentTool.SetColor(_active);
    }
  }
}