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
    private float _elapsedTime = 0;
    private bool _isOnToolsPanel;
    public string CurrentTool => _currentTool.gameObject.name;
    public event UnityAction OnToolChanged;
    public event UnityAction OnToolPanelEnter;
    public event UnityAction OnToolPanelExit;

    private void Awake()
    {
      _currentTool = _tools[3];
      
      foreach(var tool in _tools)
        tool.SetColor(_inactive);
      
      _currentTool.SetColor(_active);
    }

    private void Update()
    {
      if (!_isOnToolsPanel) return;
      _elapsedTime += Time.deltaTime;
      if(_elapsedTime >= 0.5f)
        OnToolsPanelExit();
    }

    public void SetToolActive(Tool tool)
    {
      SetCurrentTool(tool);
    }

    public void OnToolsPanelEnter()
    {
      _elapsedTime = 0;
      _isOnToolsPanel = true;
      OnToolPanelEnter?.Invoke();
    }

    public void OnToolsPanelExit()
    {
      _isOnToolsPanel = false;
      OnToolPanelExit?.Invoke();
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