using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Tools.ToolsSystem
{
  public class ToolsPanel : MonoBehaviour
  {
    [SerializeField] private Color _active;
    [SerializeField] private Color _inactive;
    [SerializeField] private List<Tool> _tools;
    private Tool _currentTool;
    public string CurrentTool => _currentTool.gameObject.name;
    public event UnityAction OnToolChanged;

    private void Awake()
    {
      _currentTool = _tools[0];
      
      foreach(var tool in _tools)
        tool.SetColor(_inactive);
      
      _currentTool.SetColor(_active);
    }

    public void SetToolActive(Tool tool)
    {
      SetCurrentTool(tool);
    }

    private void SetCurrentTool(Tool chosenTool)
    {
      _currentTool = chosenTool;
      
      foreach(var tool in _tools)
        tool.SetColor(_inactive);
      
      _currentTool.SetColor(_active);
      OnToolChanged?.Invoke();
    }
  }
}