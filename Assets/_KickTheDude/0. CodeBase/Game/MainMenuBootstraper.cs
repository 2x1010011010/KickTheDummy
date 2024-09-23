using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MainMenuBootstraper : MonoBehaviour
{
    private IUIService _uiService;

    [Inject]
    private void Construct(IUIService uiService)
    {
        _uiService = uiService;

        Debug.Log("[BOOTSTRAPER] Main menu bootstraper get dependensies!");
    }

    private void Start()
    {
        _uiService.OpenWindow<UIMainMenuWindow>();
    }
}
