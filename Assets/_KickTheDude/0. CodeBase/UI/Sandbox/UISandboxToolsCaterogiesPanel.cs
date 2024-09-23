using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class UISandboxToolsCaterogiesPanel : UIPanel
{
    [SerializeField, BoxGroup("SETUP")] private Transform _toolsButtonsRoot;
    [SerializeField, BoxGroup("SETUP")] private GameObject _delimeter;
    [SerializeField, BoxGroup("SETUP")] private GameObject _grid;
    [SerializeField, BoxGroup("SETUP")] private Dictionary<UIHighlightedButton, ToolCategory> _toolCategoryButtons = new Dictionary<UIHighlightedButton, ToolCategory>();

    [SerializeField, BoxGroup("DEBUG"), ReadOnly] private Dictionary<ToolCategory, List<UISandboxToolButton>> _toolButtons;

    private IUIService _uiService;
    private IUIFactory _uiFactory;
    private IPlayer _player;

    private bool Initialized;

    private UIHighlightedButton _selectedButtonCategory;

    [Inject]
    private void Construct(IUIService uiService, IUIFactory uiFactory, IPlayer player)
    {
        _uiService = uiService;
        _uiFactory = uiFactory;
        _player = player;
    }

    public async void Initialize()
    {
        foreach(var toolButton in _toolCategoryButtons)
            toolButton.Key.ButtonClicked += ToolCategoryButtonClicked;

        foreach (var toolCategory in Enum.GetValues(typeof(ToolCategory)))
            _toolButtons.Add((ToolCategory)toolCategory, new List<UISandboxToolButton>());

        foreach (var tool in _player.Tools)
        {
            var newButton = await _uiFactory.CreateButton<UISandboxToolButton>(_toolsButtonsRoot);
            newButton.Construct(tool.Key, _uiService);

            if(_toolButtons.TryGetValue(tool.Key.ToolCategory, out List<UISandboxToolButton> toolButtons))
                toolButtons.Add(newButton);

            newButton.ButtonClicked += ToolButtonClicked;
        }

        ToolCategoryButtonClicked(_toolCategoryButtons.First().Key);

        _delimeter.SetActive(false);
        _grid.SetActive(false);

        Initialized = true;
    }

    private void Dispose()
    {
        foreach (var buttons in _toolButtons.Values)
            foreach (var button in buttons)
                button.ButtonClicked -= ToolButtonClicked;

        foreach (var toolButton in _toolCategoryButtons)
            toolButton.Key.ButtonClicked -= ToolCategoryButtonClicked;
    }

    public override void Show()
    {
        if (!Initialized) Initialize();

        base.Show();
    }

    public override void Hide()
    {
        Dispose();

        base.Hide();
    }

    private void ToolCategoryButtonClicked(UIButton uiButton)
    {
        var button = (UIHighlightedButton)uiButton;
        var category = _toolCategoryButtons.GetValueOrDefault(button);

        ChangeSelectedCategoryButton(button);
        ActivateButtonsByCategory(category);
        ChangeGridViewState(category);
    }

    private void ChangeSelectedCategoryButton(UIHighlightedButton uiHighlightedButton)
    {
        foreach (var button in _toolCategoryButtons.Keys)
            if (button != uiHighlightedButton)
                button.Unhighlight();
            else
                button.Highlight();

        _selectedButtonCategory = uiHighlightedButton;
    }

    private void ActivateButtonsByCategory(ToolCategory toolCategory)
    {
        foreach(var pair in _toolButtons)
        {
            foreach (var button in pair.Value)
                if (pair.Key == toolCategory)
                    button.Activate();
                else
                    button.Deactivate();
        }
    }

    private void ChangeGridViewState(ToolCategory toolCategory)
    {
        if(_toolButtons.TryGetValue(toolCategory, out List<UISandboxToolButton> list))
        {
            if(list.Count > 0)
            {
                _delimeter.SetActive(true);
                _grid.SetActive(true);
            }
            else
            {
                _delimeter.SetActive(false);
                _grid.SetActive(false);
            }
        }
    }

    private void ToolButtonClicked(UIButton uiButton)
    {
        var button = (UISandboxToolButton)uiButton;

        _player.Tools.TryGetValue(button.Tool, out ITool tool);
        _player.SetActiveTool(tool);

        _delimeter.SetActive(false);
        _grid.SetActive(false);
    }
}
