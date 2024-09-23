using System.Collections;
using UnityEngine;
using Zenject;

public class SandboxBootstraper : MonoBehaviour
{
    private IUIService _uiService;
    private IPlayer _player;

    [Inject]
    private void Construct(IUIService uiService, IPlayer player)
    {
        _uiService = uiService;
        _player = player;

        Debug.Log("[BOOTSTRAPER] Sandbox bootstraper get dependensies!");
    }

    private void Start()
    {
        Initialize();
    }

    private async void Initialize()
    {
        await _uiService.OpenWindow<UILoadingWindow>();
        await _player.Initialize();
        await _uiService.OpenWindow<UISandboxHUDWindow>();

        _uiService.CloseWindow<UILoadingWindow>();
    }
}
