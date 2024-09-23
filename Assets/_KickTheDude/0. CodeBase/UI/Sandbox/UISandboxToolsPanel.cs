using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UISandboxToolsPanel : UIPanel
{
    [SerializeField, BoxGroup("SETUP")] private List<UIHighlightedButton> _toolButtons;
    [SerializeField, BoxGroup("DEBUG"), ReadOnly()] private UIHighlightedButton _currentHighlightedTool;

    private IPlayer _player;

    [Inject]
    private void Construct(IPlayer player)
    {
        _player = player;
    }

    public override void Show()
    {
        foreach(var toolButton in _toolButtons)
            toolButton.ButtonClicked += ToolButtonClicked;

        ApplyTool(0);

        base.Show();
    }

    public override void Hide()
    {
        foreach (var toolButton in _toolButtons)
            toolButton.ButtonClicked -= ToolButtonClicked;

        base.Hide();
    }

    private void ToolButtonClicked(UIButton uiButton)
    {
        ApplyTool(_toolButtons.IndexOf((UIHighlightedButton)uiButton));
    }

    private void ApplyTool(int toolID)
    {
        _currentHighlightedTool?.Unhighlight();

        _currentHighlightedTool = _toolButtons[toolID];
        _currentHighlightedTool.Highlight();
    }
}
