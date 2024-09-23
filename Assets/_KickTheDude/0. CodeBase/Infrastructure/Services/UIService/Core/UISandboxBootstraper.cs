using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UISandboxBootstraper : UIScenesBootstraper
{
    private IUIService _uiService;

    [Inject]
    private void Construct(IUIService uiService)
    {
        _uiService = uiService;
    }

    public override void Bootstrap()
    {
        _uiService.CloseAllWindows();
        //_uiService.OpenWindow<UISandboxMainButtonsWindow>();
        //_uiService.OpenWindow<UIPlayerWindow>();
    }
}
